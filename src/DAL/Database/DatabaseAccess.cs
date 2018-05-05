﻿namespace HoU.GuildBot.DAL.Database
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;
    using Model;
    using Shared.DAL;
    using Shared.Objects;

    [UsedImplicitly]
    public class DatabaseAccess : IDatabaseAccess
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly AppSettings _appSettings;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public DatabaseAccess(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Private Methods

        private HoUEntities GetEntities() => new HoUEntities(_appSettings.ConnectionString);

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IDatabaseAccess Members

        async Task IDatabaseAccess.AddUsers(IEnumerable<ulong> userIDs)
        {
            using (var entities = GetEntities())
            {
                var existingUserIDs = await entities.User.Select(m => m.DiscordUserID).ToArrayAsync().ConfigureAwait(false);
                var missingUserIDs = userIDs.Except(existingUserIDs.Select(m => (ulong)m));

                // Add missing users
                foreach (var missingUserID in missingUserIDs)
                {
                    entities.User.Add(new User
                    {
                        DiscordUserID = missingUserID
                    });
                }

                await entities.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        async Task<bool> IDatabaseAccess.AddUser(ulong userID)
        {
            using (var entities = GetEntities())
            {
                var decUserID = (decimal) userID;
                var userExists = await entities.User.AnyAsync(m => m.DiscordUserID == decUserID).ConfigureAwait(false);
                if (userExists)
                    return false;

                // Add missing user
                entities.User.Add(new User
                {
                    DiscordUserID = decUserID
                });

                await entities.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }
        }

        #endregion
    }
}
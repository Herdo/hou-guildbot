﻿namespace HoU.GuildBot.BLL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Objects;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class GuildUserInspector : IGuildUserInspector
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IGuildUserRegistry _guildUserRegistry;
        private readonly IDatabaseAccess _databaseAccess;
        private IDiscordAccess _discordAccess;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public GuildUserInspector(IGuildUserRegistry guildUserRegistry,
                                  IDatabaseAccess databaseAccess)
        {
            _guildUserRegistry = guildUserRegistry;
            _databaseAccess = databaseAccess;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IGuildUserInspector Members

        IDiscordAccess IGuildUserInspector.DiscordAccess
        {
            set => _discordAccess = value;
        }

        async Task<EmbedData> IGuildUserInspector.GetLastSeenInfo()
        {
            var ids = _guildUserRegistry.GetGuildMemberUserIds();
            // LastSeen = null equals the user is currently online
            var data = new List<(ulong UserID, string Username, bool IsOnline, DateTime? LastSeen)>(ids.Length);

            // Fetch data for online members
            var usernames = _discordAccess.GetUserNames(ids);
            foreach (var userID in ids)
            {
                var isOnline = _discordAccess.IsUserOnline(userID);
                if (isOnline)
                    data.Add((userID, usernames[userID], true, null));
            }

            // Fetch data for offline members
            var missingUserIDs = ids.Except(data.Select(m => m.UserID));
            var lastSeenData = await _databaseAccess.GetLastSeenInfoForUsers(missingUserIDs.ToArray()).ConfigureAwait(false);
            var noInfoFallback = new DateTime(2018, 1, 1);
            data.AddRange(lastSeenData.Select(m => (m.UserID, usernames[m.UserID], false, m.LastSeen ?? (DateTime?)noInfoFallback)));

            // Format
            var result = string.Join(Environment.NewLine,
                data.OrderByDescending(m => m.IsOnline)
                    .ThenByDescending(m => m.LastSeen)
                    .ThenBy(m => m.Username)
                    .Select(m => $"{(m.IsOnline ? "Online" : m.LastSeen.Value.ToString("yyyy-MM-dd HH:mm"))}: {m.Username}"));

            return await Task.FromResult(new EmbedData
            {
                Color = Colors.LightOrange,
                Title = "Last seen times for all guild members",
                Description = result
            }).ConfigureAwait(false);
        }

        async Task IGuildUserInspector.UpdateLastSeenInfo(ulong userID, bool wasOnline, bool isOnline)
        {
            // We're only updating the info when the user goes offline
            if (!(wasOnline && !isOnline))
                return; // If the user does not change from online to offline, we can return here

            // Only save status for guild members, not guests
            if (!_guildUserRegistry.IsGuildMember(userID))
                return;

            await _databaseAccess.UpdateUserInfoLastSeen(userID, DateTime.UtcNow).ConfigureAwait(false);
        }

        #endregion
    }
}
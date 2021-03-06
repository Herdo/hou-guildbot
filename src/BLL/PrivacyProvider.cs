﻿using System.Threading.Tasks;
using System.Timers;
using JetBrains.Annotations;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.BLL
{
    [UsedImplicitly]
    public class PrivacyProvider : IPrivacyProvider
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IDatabaseAccess _databaseAccess;
        private readonly Timer _timer;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public PrivacyProvider(IDatabaseAccess databaseAccess)
        {
            _databaseAccess = databaseAccess;
            _timer = new Timer
            {
                AutoReset = true,
                Interval = 86_400_000 // 1 day
            };
            _timer.Elapsed += Timer_Elapsed;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IPrivacyProvider Members

        void IPrivacyProvider.Start()
        {
            _timer.Start();
        }

        async Task IPrivacyProvider.DeleteUserRelatedData(User user)
        {
            await _databaseAccess.DeleteUserInfo(user).ConfigureAwait(false);
            await _databaseAccess.DeleteVacations(user).ConfigureAwait(false);
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Event Handler

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Fire & forget
            Task.Run(() => _databaseAccess.DeletePastVacations()).ConfigureAwait(false);
        }

        #endregion
    }
}
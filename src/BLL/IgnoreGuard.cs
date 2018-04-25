﻿namespace HoU.GuildBot.BLL
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using JetBrains.Annotations;
    using Shared.BLL;
    using Shared.Objects;

    [UsedImplicitly]
    public class IgnoreGuard : IIgnoreGuard
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IBotInformationProvider _botInformationProvider;
        private readonly Dictionary<ulong, DateTime> _ignoreList;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public IgnoreGuard(IBotInformationProvider botInformationProvider)
        {
            _botInformationProvider = botInformationProvider;
            _ignoreList = new Dictionary<ulong, DateTime>();
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IIgnoreGuard Members

        EmbedData IIgnoreGuard.TryAddToIgnoreList(ulong userId, string username, string remainderContent)
        {
            var regex = new Regex(@"for (?<minutes>\d+) minutes\.?");
            var match = regex.Match(remainderContent);
            if (!match.Success)
                return new EmbedData
                {
                    Title = Constants.InvalidCommandUsageTitle,
                    Color = Colors.Red,
                    Description = $"**{username}**: Correct command syntax: _ignore me for **45** minutes_"
                };

            // On success, we parse the minutes
            var minutes = int.Parse(match.Groups["minutes"].ToString());

            // Check if the minutes value is between 3 and 60 minutes
            if (minutes < 3 || minutes > 60)
            {
                return new EmbedData
                {
                    Title = Constants.InvalidCommandUsageTitle,
                    Color = Colors.Red,
                    Description = $"**{username}**: You cannot be ignored by less than 3 or more than 60 minutes."
                };
            }

            // Update or insert value
            var ignoreUntil = DateTime.Now.ToUniversalTime().AddMinutes(minutes);
            _ignoreList[userId] = ignoreUntil;
            return new EmbedData
            {
                Title = "Ignore complete",
                Color = Colors.Green,
                Description = $"**{username}**: You will be ignored for the next {minutes} minutes (until {ignoreUntil:dd.MM.yyyy HH:mm:ss} UTC) in the environment **{_botInformationProvider.GetEnvironmentName()}**."
            };
        }

        EmbedData IIgnoreGuard.TryRemoveFromIgnoreList(ulong userId, string username)
        {
            if (!_ignoreList.ContainsKey(userId))
                return null;

            _ignoreList.Remove(userId);
            return new EmbedData
            {
                Title = "Notice complete",
                Color = Colors.Green,
                Description = $"**{username}**: You will no longer be ignored in the environment **{_botInformationProvider.GetEnvironmentName()}**."
            };
        }

        bool IIgnoreGuard.ShouldIgnoreMessage(ulong userId)
        {
            if (!_ignoreList.TryGetValue(userId, out var ignoreUntil)) return false;
            if (ignoreUntil > DateTime.UtcNow)
                return true;
            _ignoreList.Remove(userId);
            return false;
        }

        #endregion
    }
}
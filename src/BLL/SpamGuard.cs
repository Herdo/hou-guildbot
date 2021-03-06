﻿using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.BLL
{
    [UsedImplicitly]
    public class SpamGuard : ISpamGuard
    {
        private readonly Dictionary<ulong, (byte SoftCap, byte HardCap)> _limits;
        private readonly ulong[] _channelIDsWithDisabledSpamProtection;
        private readonly Queue<string> _recentMessage;

        public SpamGuard(AppSettings appSettings)
        {
            _limits = appSettings.SpamLimits.ToDictionary(m => m.RestrictToChannelID ?? 0, m => (m.SoftCap, m.HardCap));
            _channelIDsWithDisabledSpamProtection = appSettings.ChannelIDsWithDisabledSpamProtection;
            _recentMessage = new Queue<string>(25);
        }

        SpamCheckResult ISpamGuard.CheckForSpam(ulong userId, ulong channelId, string message, int attachmentsCount)
        {
            // If the spam protection is disabled for the channel, there's no need to do any further checks.
            // The whole channel won't count into the queue.
            if (_channelIDsWithDisabledSpamProtection.Contains(channelId))
                return SpamCheckResult.NoSpam;

            if (string.IsNullOrEmpty(message) && attachmentsCount > 0)
            {
                // Attachment upload without optional comment
                return SpamCheckResult.NoSpam;
            }

            var builtMessage = $"{{{userId}}}-{{{channelId}}}-{{{message}}}";

            // If the maximum size is hit, remove the oldest message
            if (_recentMessage.Count == 25)
                _recentMessage.Dequeue();

            // Add the new message
            _recentMessage.Enqueue(builtMessage);

            // Get limits
            if (!_limits.TryGetValue(channelId, out var limits)) limits = _limits[0];

            // Check the message for soft and hard limit
            var messageCount = _recentMessage.Count(m => m == builtMessage);
            if (messageCount >= limits.HardCap)
                return SpamCheckResult.HardLimitHit;
            if (messageCount > limits.SoftCap && messageCount < limits.HardCap)
                return SpamCheckResult.BetweenSoftAndHardLimit;
            if (messageCount == limits.SoftCap)
                return SpamCheckResult.SoftLimitHit;
            return SpamCheckResult.NoSpam;
        }
    }
}
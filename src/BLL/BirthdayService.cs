﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Annotations;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Extensions;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.BLL;

[UsedImplicitly]
public class BirthdayService
{
    private const string BirthdayRoleIdKey = "BirthdayRoleId";
    private const string BirthdayAnnouncementChannelIdKey = "BirthdayAnnouncementChannelId";

    private readonly IBirthdayProvider _birthdayProvider;
    private readonly IDynamicConfiguration _dynamicConfiguration;
    private readonly IDiscordAccess _discordAccess;

    public BirthdayService(IBirthdayProvider birthdayProvider,
                           IDynamicConfiguration dynamicConfiguration,
                           IDiscordAccess discordAccess)
    {
        _birthdayProvider = birthdayProvider;
        _dynamicConfiguration = dynamicConfiguration;
        _discordAccess = discordAccess;
    }

    public async Task ApplyRoleAsync()
    {
        var birthdayRoleId = (DiscordRoleId)_dynamicConfiguration.DiscordMapping[BirthdayRoleIdKey];
        var birthdayUsers = await _birthdayProvider.GetBirthdaysAsync(DateOnly.FromDateTime(DateTime.UtcNow));
        var userIdsWithAppliedRole = new List<DiscordUserId>(birthdayUsers.Length);
        foreach (var userId in birthdayUsers)
        {
            if(!_discordAccess.CanManageRolesForUser(userId))
                continue;

            var (success, _) = await _discordAccess.TryAddNonMemberRole(userId, birthdayRoleId);
            if (success)
            {
                userIdsWithAppliedRole.Add(userId);
            }
            else
            {
                await _discordAccess.LogToDiscord($"{_discordAccess.GetLeadershipMention()}: "
                                                + $"Failed to apply role {birthdayRoleId.ToMention()} to {userId.ToMention()}.");
            }
        }

        if (userIdsWithAppliedRole.Count == 0)
            return;

        var birthdayAnnouncementChannelId = (DiscordChannelId)_dynamicConfiguration.DiscordMapping[BirthdayAnnouncementChannelIdKey];
        var messages = userIdsWithAppliedRole.Select(m => "HoU wishes a Happy Birthday to our latest Birthday Yata, "
                                                        + $"{m.ToMention()}. Have an amazing day filled with fun!!!")
                                             .ToArray();
        await _discordAccess.CreateBotMessagesInChannelAsync(birthdayAnnouncementChannelId, messages);
    }

    public async Task RevokeRoleAsync()
    {
        var birthdayRoleId = (DiscordRoleId)_dynamicConfiguration.DiscordMapping[BirthdayRoleIdKey];
        var birthdayUsers = await _birthdayProvider.GetBirthdaysAsync(DateOnly.FromDateTime(DateTime.UtcNow));
        foreach (var userId in birthdayUsers)
        {
            if (!_discordAccess.CanManageRolesForUser(userId))
                continue;

            var (success, _) = await _discordAccess.TryRevokeNonMemberRole(userId, birthdayRoleId);
            if (!success)
            {
                await _discordAccess.LogToDiscord($"{_discordAccess.GetLeadershipMention()}: "
                                                + $"Failed to revoke role {birthdayRoleId.ToMention()} from {userId.ToMention()}.");
            }
        }
    }
}
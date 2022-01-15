﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.DAL;

public interface IDiscordAccess
{
    /// <summary>
    /// Gets if the Discord access is connected to Discord.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets if the 'Hand of Unity' guild is available.
    /// </summary>
    bool IsGuildAvailable { get; }

    /// <summary>
    /// Tries to establish a connection to Discord.
    /// </summary>
    /// <param name="connectedHandler"><see cref="Func{TResult}"/> that will be invoked when the connection has been established.</param>
    /// <param name="disconnectedHandler"><see cref="Func{TResult}"/> that will be invoked when the connection is lost.</param>
    /// <exception cref="ArgumentNullException"><paramref name="connectedHandler"/> or <paramref name="disconnectedHandler"/> are <b>null</b>.</exception>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task Connect(Func<Task> connectedHandler, Func<Task> disconnectedHandler);

    /// <summary>
    /// Sets the <paramref name="gameName"/> as current bot game name.
    /// </summary>
    /// <param name="gameName">The game name.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task SetCurrentGame(string gameName);

    /// <summary>
    /// Logs the <paramref name="message"/> in the dedicated logging channel on Discord.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <exception cref="ArgumentNullException"><paramref name="message"/> is <b>null</b>.</exception>
    /// <exception cref="ArgumentException"><paramref name="message"/> is empty or only whitespaces.</exception>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task LogToDiscord(string message);

    /// <summary>
    /// Checks if the user with the <paramref name="userId"/> is currently online.
    /// </summary>
    /// <param name="userId">The Id of the user to check.</param>
    /// <returns><b>True</b>, if the user is online, otherwise <b>false</b>.</returns>
    bool IsUserOnline(DiscordUserId userId);

    /// <summary>
    /// Gets the user names for the given <paramref name="userIds"/>.
    /// </summary>
    /// <param name="userIds">The user Ids to get the names for.</param>
    /// <returns>A mapping from a userId to a display name.</returns>
    Dictionary<DiscordUserId, string> GetUserNames(IEnumerable<DiscordUserId> userIds);

    /// <summary>
    /// Sets the non-member related <paramref name="targetRole"/> for the given <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The user Id.</param>
    /// <param name="targetRole">The target role.</param>
    /// <returns>True, if the role was added, otherwise false.</returns>
    Task<bool> TryAddNonMemberRole(DiscordUserId userId,
                                   Role targetRole);

    /// <summary>
    /// Sets the non-member related <paramref name="targetRole"/> for the given <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The user Id.</param>
    /// <param name="targetRole">The target role.</param>
    /// <returns>True, if the role was added, otherwise false. Includes the role name.</returns>
    Task<(bool Success, string RoleName)> TryAddNonMemberRole(DiscordUserId userId,
                                                              DiscordRoleId targetRole);

    /// <summary>
    /// Revokes the non-member related <paramref name="targetRole"/> from the given <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The user Id.</param>
    /// <param name="targetRole">The target role.</param>
    /// <returns>True, if the role was revoked, otherwise false.</returns>
    Task<bool> TryRevokeNonMemberRole(DiscordUserId userId,
                                      Role targetRole);

    /// <summary>
    /// Revokes the non-member related <paramref name="targetRole"/> from the given <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The user Id.</param>
    /// <param name="targetRole">The target role.</param>
    /// <returns>True, if the role was revoked, otherwise false. Includes the role name.</returns>
    Task<(bool Success, string RoleName)> TryRevokeNonMemberRole(DiscordUserId userId,
                                                                 DiscordRoleId targetRole);

    /// <summary>
    /// Assigns the <paramref name="roleId"/> to the given <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The user Id.</param>
    /// <param name="roleId">The Id of the role to assign to the user.</param>
    /// <returns>True, if the role was assigned, otherwise false.</returns>
    Task<bool> TryAssignRoleAsync(DiscordUserId userId, DiscordRoleId roleId);

    /// <summary>
    /// Revokes the <paramref name="roleId"/> from the given <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The user Id.</param>
    /// <param name="roleId">The Id of the role to revoke from the user.</param>
    /// <returns>True, if the role was revoked, otherwise false.</returns>
    Task<bool> TryRevokeGameRole(DiscordUserId userId, DiscordRoleId roleId);

    /// <summary>
    /// Checks if the bot can manage roles for a specific <paramref name="userId"/>, depending on the guilds role configuration.
    /// </summary>
    /// <param name="userId">The Id of the user to change roles for.</param>
    /// <returns><b>True</b>, if the bot can manage roles for the user, otherwise <b>false</b>.</returns>
    bool CanManageRolesForUser(DiscordUserId userId);

    /// <summary>
    /// Gets the mention string for the give <paramref name="roleName"/>.
    /// </summary>
    /// <param name="roleName">The name of the role to get the mention for.</param>
    /// <exception cref="InvalidOperationException">No role with the given <paramref name="roleName"/> exists.</exception>
    /// <returns>The mention string.</returns>
    string GetRoleMention(string roleName);

    /// <summary>
    /// Gets all messages posted by the bot to the given <paramref name="channelId"/>.
    /// </summary>
    /// <param name="channelId">The Id of the channel to search for bot messages in.</param>
    /// <returns>An array of found messages.</returns>
    Task<TextMessage[]> GetBotMessagesInChannel(DiscordChannelId channelId);

    /// <summary>
    /// Deletes all messages of the bot in the given <paramref name="channelId"/>.
    /// </summary>
    /// <param name="channelId">The Id of the channel to delete the messages in.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task DeleteBotMessagesInChannel(DiscordChannelId channelId);

    /// <summary>
    /// Deletes a specific <paramref name="messageId"/> of the bot in the given <paramref name="channelId"/>.
    /// </summary>
    /// <param name="channelId">The Id of the channel to delete the message in.</param>
    /// <param name="messageId">The Id of the message to delete.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task DeleteBotMessageInChannelAsync(DiscordChannelId channelId, ulong messageId);

    /// <summary>
    /// Creates the <paramref name="messages"/> in the given <paramref name="channelId"/>.
    /// </summary>
    /// <param name="channelId">The Id of the channel to create the <paramref name="messages"/> in.</param>
    /// <param name="messages">The messages to create.</param>
    /// <returns>An array of the message Ids.</returns>
    Task<ulong[]> CreateBotMessagesInChannelAsync(DiscordChannelId channelId, string[] messages);
    
    /// <summary>
    /// Creates the <paramref name="messages"/> with their <see cref="SelectMenuComponent"/>s in the given <paramref name="channelId"/>.
    /// </summary>
    /// <param name="channelId">The Id of the channel to create the <paramref name="messages"/> in.</param>
    /// <param name="messages">The messages to create with their <see cref="SelectMenuComponent"/>s.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="messages"/> contain a message with:
    /// - more than 25 <see cref="ButtonComponent"/>s in total
    /// or
    /// - more than 5 <see cref="ButtonComponent"/>s per action row
    /// or
    /// - more than 5 <see cref="SelectMenuComponent"/>s
    /// or
    /// - more than 1 <see cref="SelectMenuComponent"/> per action row
    /// or
    /// - more than 25 <see cref="SelectMenuComponent.Options"/> per <see cref="SelectMenuComponent"/>.</exception>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task CreateBotMessagesInChannelAsync(DiscordChannelId channelId, (string Content, ActionComponent[] Components)[] messages);

    /// <summary>
    /// Creates the <paramref name="message"/> in the servers welcome channel.
    /// </summary>
    /// <param name="message">The message to create.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task CreateBotMessageInWelcomeChannel(string message);

    /// <summary>
    /// Counts the guild members having the <paramref name="roleIds"/>.
    /// </summary>
    /// <param name="roleIds">The Ids of the roles to count.</param>
    /// <returns>The amount of guild members having the <paramref name="roleIds"/>.</returns>
    int CountGuildMembersWithRoles(DiscordRoleId[] roleIds);

    /// <summary>
    /// Counts the guild members having the <paramref name="roleIds"/> and not having the <paramref name="roleIdsToExclude"/>.
    /// </summary>
    /// <param name="roleIds">The Ids of the roles to count.</param>
    /// <param name="roleIdsToExclude">The role Ids that will skip a count of the guild member.</param>
    /// <returns>The amount of guild members having the <paramref name="roleIds"/> and not the <paramref name="roleIdsToExclude"/>.</returns>
    int CountGuildMembersWithRoles(DiscordRoleId[]? roleIds, DiscordRoleId[] roleIdsToExclude);

    /// <summary>
    /// Counts the guild members having the <paramref name="roleNames"/>.
    /// </summary>
    /// <param name="roleNames">The names of the roles to count.</param>
    /// <returns>The amount of guild members having the <paramref name="roleNames"/>.</returns>
    int CountGuildMembersWithRoles(string[] roleNames);

    /// <summary>
    /// Gets the current display name for the <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user to get the current display name for.</param>
    /// <returns>The nickname, if it's set, otherwise the username.</returns>
    string GetCurrentDisplayName(DiscordUserId userId);

    /// <summary>
    /// Gets the name of the channel for the <paramref name="discordChannelId"/>.
    /// </summary>
    /// <param name="discordChannelId">The Id of the discord channel to get the name for.</param>
    /// <returns>The name of the channel.</returns>
    string GetChannelLocationAndName(DiscordChannelId discordChannelId);

    /// <summary>
    /// Creates a temporary voice channel.
    /// </summary>
    /// <param name="voiceChannelsCategoryId">The Id of the category where the voice channel should be placed.</param>
    /// <param name="name">The name of the voice channel.</param>
    /// <param name="maxUsers">The maximum number of users allowed into the voice channel.</param>
    /// <returns>The Id of the created channel, or an error while creating it.</returns>
    Task<(DiscordChannelId VoiceChannelId, string? Error)> CreateVoiceChannel(DiscordChannelId voiceChannelsCategoryId,
                                                                              string name,
                                                                              int maxUsers);

    /// <summary>
    /// Reorders the <paramref name="channelIds"/> to be in order above the <paramref name="positionAboveChannelId"/>.
    /// </summary>
    /// <param name="channelIds">The Ids of the channels to reorder.</param>
    /// <param name="positionAboveChannelId">The Id of the channel to place the <paramref name="channelIds"/> above.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task ReorderChannelsAsync(DiscordChannelId[] channelIds,
                              DiscordChannelId positionAboveChannelId);
        
    /// <summary>
    /// Deletes the voice channel.
    /// </summary>
    /// <param name="voiceChannelId">The Id of the voice channel to delete.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task DeleteVoiceChannel(DiscordChannelId voiceChannelId);
    
    /// <summary>
    /// Gets the Id of the avatar for the <paramref name="userId"/>, if the user has any avatar set.
    /// </summary>
    /// <param name="userId">The <see cref="DiscordUserId"/> of the user to get the avatar Id for.</param>
    /// <returns>The avatar Id, or <b>null</b>.</returns>
    string? GetAvatarId(DiscordUserId userId);

    /// <summary>
    /// Gets all users that have one or more of the <paramref name="allowedRoles"/>.
    /// </summary>
    /// <param name="allowedRoles">The roles that are allowed for users in the result set.</param>
    /// <returns>All users with their allowed roles and detailed information.</returns>
    UserModel[] GetUsersInRoles(string[] allowedRoles);

    /// <summary>
    /// Gets the leadership mention string.
    /// </summary>
    /// <returns>A mention for Leaders and Officers.</returns>
    string GetLeadershipMention();

    /// <summary>
    /// Sends the <paramref name="embedData"/> as a notification in the UnitsNotificationsChannel.
    /// </summary>
    /// <param name="embedData">The <see cref="EmbedData"/> to send.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task SendUnitsNotificationAsync(EmbedData embedData);

    /// <summary>
    /// Sends the <paramref name="embedData"/> as a notification in the UnitsNotificationsChannel.
    /// </summary>
    /// <param name="embedData">The <see cref="EmbedData"/> to send.</param>
    /// <param name="usersToNotify">The users to notify about the <paramref name="embedData"/>.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task SendUnitsNotificationAsync(EmbedData embedData,
                                    DiscordUserId[] usersToNotify);

    /// <summary>
    /// Gets all users in the given <paramref name="voiceChannelIds"/>.
    /// </summary>
    /// <param name="voiceChannelIds">The Ids of the voice channels to check.</param>
    /// <returns>A dictionary containing the voice channel id as key, and the users in that channel as value.</returns>
    Dictionary<string, List<DiscordUserId>> GetUsersInVoiceChannels(string[] voiceChannelIds);

    /// <summary>
    /// Gets all the <see cref="DiscordUserId"/>s of people having the <paramref name="roleId"/>.
    /// </summary>
    /// <param name="roleId">The Id of the role to check.</param>
    /// <returns>The list of <see cref="DiscordUserId"/> in the role, or empty.</returns>
    List<DiscordUserId> GetUsersIdsInRole(DiscordRoleId roleId);

    /// <summary>
    /// Ensures that the <see cref="AvailableGame.DisplayName"/> properties of the <paramref name="games"/> are set.
    /// </summary>
    /// <param name="games">The games to set the display names for.</param>
    void EnsureDisplayNamesAreSet(IEnumerable<AvailableGame> games);

    /// <summary>
    /// Ensures that the <see cref="AvailableGameRole.DisplayName"/> properties of the <paramref name="gameRoles"/> are set.
    /// </summary>
    /// <param name="gameRoles">The game roles to set the display names for.</param>
    void EnsureDisplayNamesAreSet(IEnumerable<AvailableGameRole> gameRoles);

    /// <summary>
    /// Gets the current roles for the given <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The <see cref="DiscordUserId"/> of the user to get the roles for.</param>
    /// <returns>An array of all current roles the user has.</returns>
    DiscordRoleId[] GetUserRoles(DiscordUserId userId);
}
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.DAL
{
    public interface IDatabaseAccess
    {
        Task<User[]> GetAllUsers();

        /// <summary>
        /// Adds a given set of user IDs to the database.
        /// </summary>
        /// <param name="userIDs">The IDs to add.</param> 
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        /// <remarks>User IDs that are already present on the database won't be added.</remarks>
        Task EnsureUsersExist(IEnumerable<DiscordUserID> userIDs);

        /// <summary>
        /// Adds a user ID to the database.
        /// </summary>
        /// <param name="userID">The ID to add.</param>
        /// <returns>The <see cref="User"/> object that was either queried from the database or added.</returns>
        Task<(User User, bool IsNew)> GetOrAddUser(DiscordUserID userID);

        /// <summary>
        /// Gets all messages listed on the database.
        /// </summary>
        /// <returns>An array of tupples, containing the message name (key), a description and the current content.</returns>
        Task<(string Name, string Description, string Content)[]> GetAllMessages();

        /// <summary>
        /// Gets the current content for a message with a specific <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name (key) of the message.</param>
        /// <returns>The message content, if found, otherwise <b>null</b>.</returns>
        Task<string> GetMessageContent(string name);

        /// <summary>
        /// Sets the <paramref name="content"/> for a message with a specific <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name (key) of the message.</param>
        /// <param name="content">The new content of the message.</param>
        /// <returns><b>true</b>, if the <paramref name="name"/> exists, otherwise <b>false</b>.</returns>
        Task<bool> SetMessageContent(string name, string content);

        /// <summary>
        /// Adds a vacation duration for a specific <paramref name="user"/>, if the vacation is not colliding with another vacation of the same user.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="start">The start date of the vacation.</param>
        /// <param name="end">The end date of the vacation.</param>
        /// <param name="note">An optional note for the vacation.</param>
        /// <returns><b>True</b>, if the vacation could be added without any collisions, otherwise <b>false</b>.</returns>
        Task<bool> AddVacation(User user, DateTime start, DateTime end, string note);

        /// <summary>
        /// Deletes a matching vacation for the <paramref name="user"/>, <paramref name="start"/> and <paramref name="end"/>.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="start">The start date of the vacation.</param>
        /// <param name="end">The end date of the vacation.</param>
        /// <returns><b>True</b>, if a matching vacation was found and deleted, otherwise <b>false</b>.</returns>
        Task<bool> DeleteVacation(User user, DateTime start, DateTime end);
        
        /// <summary>
        /// Deletes all vacations that are in the past.
        /// </summary>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task DeletePastVacations();

        /// <summary>
        /// Deletes all vacations having a relation with the <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task DeleteVacations(User user);

        /// <summary>
        /// Gets all upcoming vacations.
        /// </summary>
        /// <returns>An array of tupples, containing the user ID, the vacation start and end, as well as an optional note.</returns>
        Task<(DiscordUserID UserID, DateTime Start, DateTime End, string Note)[]> GetVacations();

        /// <summary>
        /// Gets all vacations for a specific <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user to get the vacations for.</param>
        /// <returns>An array of tupples, containing the user ID, the vacation start and end, as well as an optional note.</returns>
        Task<(DiscordUserID UserID, DateTime Start, DateTime End, string Note)[]> GetVacations(User user);

        /// <summary>
        /// Gets all vacations for a specific <paramref name="date"/>.
        /// </summary>
        /// <param name="date">The date to get the vacations for.</param>
        /// <returns>An array of tupples, containing the user ID, the vacation start and end, as well as an optional note.</returns>
        Task<(DiscordUserID UserID, DateTime Start, DateTime End, string Note)[]> GetVacations(DateTime date);

        /// <summary>
        /// Gets all available games.
        /// </summary>
        /// <returns>An array of available games.</returns>
        Task<AvailableGame[]> GetAvailableGames();

        /// <summary>
        /// Updates the <paramref name="lastSeen"/> info for the given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user to update the last seen info for.</param>
        /// <param name="lastSeen">The timestamp to use as info.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task UpdateUserInfoLastSeen(User user, DateTime lastSeen);

        /// <summary>
        /// Gets the last seen info for the given <paramref name="users"/>.
        /// </summary>
        /// <param name="users">The users to fetch the last seen info for.</param>
        /// <exception cref="ArgumentNullException"><paramref name="users"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="users"/> is empty.</exception>
        /// <returns>An array of tipples, containing the user ID and the last seen timestamp. The timestamp is <b>null</b>, if no info is available.</returns>
        Task<(InternalUserID UserID, DateTime? LastSeen)[]> GetLastSeenInfoForUsers(User[] users);

        /// <summary>
        /// Deletes the user info related to the <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task DeleteUserInfo(User user);

        /// <summary>
        /// Tries to get the ID of the game associated with the <paramref name="shortName"/>.
        /// </summary>
        /// <param name="shortName">The short name of the game.</param>
        /// <returns>The ID of the game, if it was found, otherwise <b>null</b>.</returns>
        Task<short?> TryGetGameID(string shortName);

        /// <summary>
        /// Tries to get the ID of the game role associated with the <paramref name="discordRoleID"/>.
        /// </summary>
        /// <param name="discordRoleID">The ID of the Discord role.</param>
        /// <returns>The ID and current name of the game role, if it was found, otherwise <b>null</b>.</returns>
        Task<(short ID, string CurrentName)?> TryGetGameRole(ulong discordRoleID);

        /// <summary>
        /// Tries to add the game.
        /// </summary>
        /// <param name="userID">The ID of the user who adds the game.</param>
        /// <param name="gameLongName">The long name of the game.</param>
        /// <param name="gameShortName">The short name of the game.</param>
        /// <param name="primaryGameDiscordRoleID">The optional ID of the primary game role.</param>
        /// <returns>A value tuple containing the success state, and, if the add fails, the error that occured.</returns>
        Task<(bool Success, string Error)> TryAddGame(InternalUserID userID,
                                                      string gameLongName,
                                                      string gameShortName,
                                                      ulong? primaryGameDiscordRoleID);

        /// <summary>
        /// Tries to edit the game.
        /// </summary>
        /// <param name="userID">The ID of the user who edits the game.</param>
        /// <param name="gameID">The ID of the game to edit.</param>
        /// <param name="updated">The updated class holding the new values.</param>
        /// <returns>A value tuple containing the success state, and, if the add fails, the error that occured.</returns>
        Task<(bool Success, string Error)> TryEditGame(InternalUserID userID,
                                                       short gameID,
                                                       AvailableGame updated);

        /// <summary>
        /// Tries to remove a game and all dependencies.
        /// </summary>
        /// <param name="gameID">The ID of the game to remove.</param>
        /// <returns>A value tuple containing the success state, and, if the add fails, the error that occured.</returns>
        Task<(bool Success, string Error)> TryRemoveGame(short gameID);

        /// <summary>
        /// Tries to add the game role.
        /// </summary>
        /// <param name="userID">The ID of the user who adds the role.</param>
        /// <param name="gameID">The ID of the game to add the game role to.</param>
        /// <param name="roleName">The name of the role to add.</param>
        /// <param name="discordRoleID">The DiscordID of the role.</param>
        /// <returns>A value tuple containing the success state, and, if the add fails, the error that occured.</returns>
        Task<(bool Success, string Error)> TryAddGameRole(InternalUserID userID,
                                                          short gameID,
                                                          string roleName,
                                                          ulong discordRoleID);

        /// <summary>
        /// Tries to edit the game role.
        /// </summary>
        /// <param name="userID">The ID of the user who edits the role.</param>
        /// <param name="gameRoleID">The ID of the game role to edit.</param>
        /// <param name="newRoleName">The new name of the role.</param>
        /// <returns>A value tuple containing the success state, and, if the add fails, the error that occured.</returns>
        Task<(bool Success, string Error)> TryEditGameRole(InternalUserID userID, 
                                                           short gameRoleID,
                                                           string newRoleName);

        /// <summary>
        /// Tries to remove the game role.
        /// </summary>
        /// <param name="gameRoleID">The ID of the game role to remove.</param>
        /// <returns>A value tuple containing the success state, and, if the add fails, the error that occured.</returns>
        Task<(bool Success, string Error)> TryRemoveGameRole(short gameRoleID);
    }
}
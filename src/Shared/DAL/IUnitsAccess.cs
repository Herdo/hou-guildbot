﻿using System.Collections.Generic;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.DAL
{
    public interface IUnitsAccess
    {
        /// <summary>
        /// Queries the UNIT system for all valid role names that can be synced to the system.
        /// </summary>
        /// <param name="unitsSyncData">The data used to sync with the UNIT system.</param>
        /// <returns>An array of allowed role names in the UNIT system, or <b>null</b>, if the web createdVoiceChannelsRequest failed.</returns>
        Task<string[]> GetValidRoleNamesAsync(UnitsSyncData unitsSyncData);

        /// <summary>
        /// Sends the <paramref name="users"/> to the UNIT system.
        /// </summary>
        /// <param name="unitsSyncData">The data used to sync with the UNIT system.</param>
        /// <param name="users">The users to synchronize.</param>
        /// <returns>A <see cref="SyncAllUsersResponse"/>, if the <paramref name="users"/> were synchronized, otherwise <b>null</b>.</returns>
        Task<SyncAllUsersResponse> SendAllUsersAsync(UnitsSyncData unitsSyncData,
                                                     UserModel[] users);

        /// <summary>
        /// Sends the <paramref name="createdVoiceChannelsRequest"/> to the UNIT system.
        /// </summary>
        /// <param name="unitsSyncData">The data used to sync with the UNIT system.</param>
        /// <param name="createdVoiceChannelsRequest">The createdVoiceChannelsRequest containing the information about the created voice channels</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task SendCreatedVoiceChannelsAsync(UnitsSyncData unitsSyncData,
                                           SyncCreatedVoiceChannelsRequest createdVoiceChannelsRequest);

        /// <summary>
        /// Sends the <paramref name="currentAttendeesRequest"/> to the UNIT system.
        /// </summary>
        /// <param name="unitsSyncData"></param>
        /// <param name="currentAttendeesRequest"></param>
        /// <returns></returns>
        Task SendCurrentAttendeesAsync(UnitsSyncData unitsSyncData,
                                       SyncCurrentAttendeesRequest currentAttendeesRequest);
    }
}
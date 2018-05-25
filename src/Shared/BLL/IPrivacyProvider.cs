﻿namespace HoU.GuildBot.Shared.BLL
{
    using System.Threading.Tasks;

    public interface IPrivacyProvider
    {
        /// <summary>
        /// Starts a daily interval check for data to delete from the database.
        /// </summary>
        void Start();

        /// <summary>
        /// Deletes user related data upon leaving the server.
        /// </summary>
        /// <param name="userID">The ID of the user that left the server.</param>
        /// <remarks>Should be called as Fire-And-Forget, as cleaning up this data might take a while.</remarks>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task DeleteUserRelatedData(ulong userID);
    }
}
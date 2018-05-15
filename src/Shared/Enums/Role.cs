﻿namespace HoU.GuildBot.Shared.Enums
{
    using System;

    [Flags]
    public enum Role
    {
        NoRole = 0,
        /// <summary>
        /// Only users with the role <see cref="Developer"/> may use these commands.
        /// </summary>
        Developer = 1,
        /// <summary>
        /// Only users with the role <see cref="Leader"/> may use these commands.
        /// </summary>
        Leader = 2,
        /// <summary>
        /// Only users with the role <see cref="SeniorOfficer"/> may use these commands.
        /// </summary>
        SeniorOfficer = 4,
        /// <summary>
        /// Only users with the role <see cref="Officer"/> may use these commands.
        /// </summary>
        Officer = 8,
        /// <summary>
        /// Only users with the role <see cref="Member"/> may use these commands.
        /// </summary>
        Member = 16,
        /// <summary>
        /// Only users with the role <see cref="Recruit"/> may use these commands.
        /// </summary>
        Recruit = 32,
        /// <summary>
        /// Any user who is a guild member may use these commands.
        /// </summary>
        AnyGuildMember = Leader | SeniorOfficer | Officer | Member | Recruit,
        /// <summary>
        /// Not a guild member, but a <see cref="Guest"/> role.
        /// </summary>
        Guest = 64
    }
}
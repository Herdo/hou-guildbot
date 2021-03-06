﻿using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.Objects
{
    public class AppSettings
    {
        /// <summary>
        /// Gets or sets the bot token.
        /// </summary>
        public string BotToken { get; set; }

        /// <summary>
        /// Gets or sets the Discord ID of the "Hand of Unity" guild.
        /// </summary>
        public ulong HandOfUnityGuildId { get; set; }

        /// <summary>
        /// Gets the Discord ID of the channel used for logging.
        /// </summary>
        public DiscordChannelID LoggingChannelId { get; private set; }

        /// <summary>
        /// Property to bind the value of <see cref="LoggingChannelId"/> from the app settings.
        /// </summary>
        private ulong LoggingChannelIdValue
        {
            get => (ulong) LoggingChannelId;
            set => LoggingChannelId = (DiscordChannelID) value;
        }

        /// <summary>
        /// Gets the Discord ID of the community coordinator channel .
        /// </summary>
        public DiscordChannelID ComCoordinatorChannelId { get; private set; }

        /// <summary>
        /// Property to bind the value of <see cref="ComCoordinatorChannelId"/> from the app settings.
        /// </summary>
        private ulong ComCoordinatorChannelIdValue
        {
            get => (ulong) ComCoordinatorChannelId;
            set => ComCoordinatorChannelId = (DiscordChannelID) value;
        }

        /// <summary>
        /// Gets the Discord ID of the channel used for infos and public basic roles.
        /// </summary>
        public DiscordChannelID InfoAndRolesChannelId { get; private set; }

        /// <summary>
        /// Property to bind the value of <see cref="InfoAndRolesChannelId"/> from the app settings.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private ulong InfoAndRolesChannelIdValue
        {
            get => (ulong)InfoAndRolesChannelId;
            set => InfoAndRolesChannelId = (DiscordChannelID)value;
        }

        /// <summary>
        /// Gets the Discord ID of the channel used for promotion announcements.
        /// </summary>
        public DiscordChannelID PromotionAnnouncementChannelId { get; private set; }

        /// <summary>
        /// Property to bind the value of <see cref="PromotionAnnouncementChannelId"/> from the app settings.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private ulong PromotionAnnouncementChannelIdValue
        {
            get => (ulong)PromotionAnnouncementChannelId;
            set => PromotionAnnouncementChannelId = (DiscordChannelID)value;
        }

        /// <summary>
        /// Gets the Discord ID of the channel that is used for the 'Ashes of Creation' role feature.
        /// </summary>
        public DiscordChannelID AshesOfCreationRoleChannelId { get; private set; }

        /// <summary>
        /// Property to bind the value of <see cref="AshesOfCreationRoleChannelId"/> from the app settings.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private ulong AshesOfCreationRoleChannelIdValue
        {
            get => (ulong)AshesOfCreationRoleChannelId;
            set => AshesOfCreationRoleChannelId = (DiscordChannelID)value;
        }

        /// <summary>
        /// Gets the Discord ID of the channel that is used for the 'World of Warcraft' role feature.
        /// </summary>
        public DiscordChannelID WorldOfWarcraftRoleChannelId { get; private set; }

        /// <summary>
        /// Property to bind the value of <see cref="WorldOfWarcraftRoleChannelId"/> from the app settings.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private ulong WorldOfWarcraftRoleChannelIdValue
        {
            get => (ulong)WorldOfWarcraftRoleChannelId;
            set => WorldOfWarcraftRoleChannelId = (DiscordChannelID)value;
        }

        /// <summary>
        /// Gets the Discord ID of the channel that is used for the 'Games' role feature.
        /// </summary>
        public DiscordChannelID GamesRolesChannelId { get; private set; }

        /// <summary>
        /// Property to bind the value of <see cref="GamesRolesChannelId"/> from the app settings.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private ulong GamesRolesChannelIdValue
        {
            get => (ulong) GamesRolesChannelId;
            set => GamesRolesChannelId = (DiscordChannelID) value;
        }

        /// <summary>
        /// Gets the Discord ID of the channel that is used for notifications from the UNIT system.
        /// </summary>
        public DiscordChannelID UnitsNotificationsChannelId { get; private set; }

        /// <summary>
        /// Property to bind the value of <see cref="GamesRolesChannelId"/> from the app settings.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private ulong UnitsNotificationsChannelIdValue
        {
            get => (ulong)UnitsNotificationsChannelId;
            set => UnitsNotificationsChannelId = (DiscordChannelID)value;
        }

        /// <summary>
        /// Gets or sets the message ID used for friend and guest reactions.
        /// </summary>
        public ulong FriendOrGuestMessageId { get; set; }

        /// <summary>
        /// Gets or sets the message ID used for non-member game interest reactions.
        /// </summary>
        public ulong NonMemberGameInterestMessageId { get; set; }

        /// <summary>
        /// Gets or sets the category ID for voice channels.
        /// </summary>
        public ulong VoiceChannelCategoryId { get; set; }

        /// <summary>
        /// Gets or sets an array of <see cref="DesiredTimeZone"/> instances.
        /// </summary>
        public DesiredTimeZone[] DesiredTimeZones { get; set; }

        /// <summary>
        /// Gets or sets an array of <see cref="SpamLimit"/> instances.
        /// </summary>
        public SpamLimit[] SpamLimits { get; set; }

        /// <summary>
        /// Gets or sets an array of channel IDs that will have the spam protection disabled.
        /// </summary>
        public ulong[] ChannelIDsWithDisabledSpamProtection { get; set; }

        /// <summary>
        /// Gets or sets the UNITS sync access data.
        /// </summary>
        public UnitsSyncData[] UnitsAccess { get; set; }

        /// <summary>
        /// Gets or sets the ID of the voice channel above which the voice channels of UNITS events shall be created.
        /// </summary>
        public ulong UnitsEventVoiceChannelsPositionAboveChannelId { get; set; }

        /// <summary>
        /// Gets or sets the base address of the Unity Hub.
        /// </summary>
        public string UnityHubBaseAddress { get; set; }

        /// <summary>
        /// Gets or sets the secret used to authenticate with the Unity Hub.
        /// </summary>
        public string UnityHubAccessSecret { get; set; }

        /// <summary>
        /// Gets or sets the personal reminders to send periodically.
        /// </summary>
        public PersonalReminder[] PersonalReminders { get; set; }

        /// <summary>
        /// Gets or sets the ID of the role that's used as basement.
        /// </summary>
        public ulong BasementRoleId { get; set; }

        /// <summary>
        /// Gets or sets the primary connection string used to access database objects part of the solution.
        /// </summary>
        /// <remarks>Should either be a IP/TCP connection, or a named connection. For IP/TCP connections, see example.</remarks>
        /// <example>IPv4: "Server=169.100.10.154\\MSSQLSERVER,1433;Database=hou-guild;User Id=hou-guildbot;Password=PASSWORD;"
        /// IPv6: "Server=fe80::2011:f831:9281:1ffb%23\\MSSQLSERVER,1433;Database=hou-guild;User Id=hou-guildbot;Password=PASSWORD;"
        /// IPv6: "Server=fe80::2011:f831:9281:1ffb\\MSSQLSERVER,1433;Database=hou-guild;User Id=hou-guildbot;Password=PASSWORD;"</example>
        public string HandOfUnityConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the HangFire connection string used to access database objects part of HangFire.
        /// </summary>
        /// <remarks>Should either be a IP/TCP connection, or a named connection. For IP/TCP connections, see example.</remarks>
        /// <example>IPv4: "Server=169.100.10.154\\MSSQLSERVER,1433;Database=hou-guild;User Id=hou-guildbot;Password=PASSWORD;"
        /// IPv6: "Server=fe80::2011:f831:9281:1ffb%23\\MSSQLSERVER,1433;Database=hou-guild;User Id=hou-guildbot;Password=PASSWORD;"
        /// IPv6: "Server=fe80::2011:f831:9281:1ffb\\MSSQLSERVER,1433;Database=hou-guild;User Id=hou-guildbot;Password=PASSWORD;"</example>
        public string HangFireConnectionString { get; set; }

        public IConfiguration CompleteConfiguration { get; set; }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;
using Microsoft.Extensions.Logging;

namespace HoU.GuildBot.BLL
{
    public class UnitsBotClient : IUnitsBotClient
    {
        private readonly IDiscordAccess _discordAccess;
        private readonly IUnitsAccess _unitsAccess;
        private readonly AppSettings _appSettings;
        private readonly ILogger<UnitsBotClient> _logger;

        public UnitsBotClient(IDiscordAccess discordAccess,
                              IUnitsAccess unitsAccess,
                              AppSettings appSettings,
                              ILogger<UnitsBotClient> logger)
        {
            _discordAccess = discordAccess ?? throw new ArgumentNullException(nameof(discordAccess));
            _unitsAccess = unitsAccess ?? throw new ArgumentNullException(nameof(unitsAccess));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private static string GetDayOfMonthSuffix(int day)
        {
            if (day is >= 11 and <= 13)
                return "th";
            return (day % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th"
            };
        }

        private static EmbedData GetEventEmbed(string baseAddress,
                                               string actionOrTitle,
                                               RGB color)
        {
            var iconUrl = GetIconUrl(baseAddress);
            return new EmbedData
            {
                Author = "UNITS: Events",
                AuthorUrl = GetEventsUrl(baseAddress),
                AuthorIconUrl = iconUrl,
                ThumbnailUrl = iconUrl,
                Title = $":calendar_spiral: {actionOrTitle}",
                Color = color
            };
        }

        private static string GetIconUrl(string baseAddress) => $"{baseAddress}/images/logo.png";

        private static string GetEventsUrl(string baseAddress) => $"{baseAddress}/events";

        private static string GetEventUrl(string baseAddress,
                                          int appointmentId) =>
            $"{baseAddress}/events/{appointmentId}";

        private static void AddTimeField(List<EmbedField> fields,
                                         DateTime startTime,
                                         DateTime endTime,
                                         bool isAllDay,
                                         string fieldTitlePostfix)
        {
            // Time
            var duration = isAllDay ? endTime.Date.AddDays(1) - startTime.Date : endTime - startTime;
            if (isAllDay)
            {
                var timeString = new StringBuilder(startTime.ToString("ddd MMM dd"));
                timeString.Append(GetDayOfMonthSuffix(startTime.Day) + ", ");
                timeString.Append(startTime.ToString("yyyy"));
                if (duration.Days > 1)
                {
                    timeString.Append(" - ");
                    timeString.Append(endTime.ToString("ddd MMM dd"));
                    timeString.Append(GetDayOfMonthSuffix(endTime.Day) + ", ");
                    timeString.Append(endTime.ToString("yyyy"));
                }

                fields.Add(new EmbedField("Time" + fieldTitlePostfix, timeString.ToString(), false));
            }
            else
            {
                var timeString = new StringBuilder(startTime.ToString("ddd MMM dd"));
                timeString.Append(GetDayOfMonthSuffix(startTime.Day) + ", ");
                timeString.Append(startTime.ToString("yyyy"));
                timeString.Append(" ⋅ ");
                timeString.Append(startTime.ToString("h tt"));
                timeString.Append(" - ");
                timeString.Append(endTime.ToString("h tt"));
                timeString.Append(" UTC");
                fields.Add(new EmbedField("Time" + fieldTitlePostfix, timeString.ToString(), false));
            }
        }

        private static void AddLinksField(List<EmbedField> fields,
                                          string eventName,
                                          DateTime startTime)
        {

            // Links to converted time zone
            const string baseUrl = "https://www.timeanddate.com/worldclock/fixedtime.html";
            // msg = title URL encoded
            var msgParam = $"msg={Uri.EscapeUriString(eventName)}";
            // iso = ISO UTC time
            var isoParam = $"iso={startTime:yyyyMMdd}T{startTime:HHmmss}";
            // p1=1440 --> UTC time zone as base time
            var p1Param = "p1=1440";
            fields.Add(new EmbedField("Links", $"[Convert time zone]({baseUrl}?{msgParam}&{isoParam}&{p1Param})", false));
        }

        async Task IUnitsBotClient.ReceiveEventCreatedMessageAsync(string baseAddress,
                                                                   int appointmentId,
                                                                   string eventName,
                                                                   string author,
                                                                   DateTime startTime,
                                                                   DateTime endTime,
                                                                   bool isAllDay)
        {
            var fields = new List<EmbedField>();
            AddTimeField(fields, startTime, endTime, isAllDay, null);
            AddLinksField(fields, eventName, startTime);
            var embed = GetEventEmbed(baseAddress,
                                      eventName,
                                      Colors.BrightBlue);
            embed.Url = GetEventUrl(baseAddress, appointmentId);
            embed.Description = $"A [new event]({embed.Url}) was created in UNITS. " +
                                "Click to open the event in your browser.";
            embed.Fields = fields.ToArray();
            embed.FooterText = $"Created by {author}{Environment.NewLine}Local time";
            embed.Timestamp = startTime;
            await _discordAccess.SendUnitsNotificationAsync(embed);
        }

        async Task IUnitsBotClient.ReceiveEventRescheduledMessageAsync(string baseAddress, 
                                                                       int appointmentId,
                                                                       string eventName,
                                                                       DateTime startTimeOld,
                                                                       DateTime endTimeOld,
                                                                       DateTime startTimeNew,
                                                                       DateTime endTimeNew,
                                                                       bool isAllDay,
                                                                       DiscordUserID[] usersToNotify)
        {
            var fields = new List<EmbedField>();
            AddTimeField(fields, startTimeOld, endTimeOld, isAllDay, " (Old)");
            AddTimeField(fields, startTimeNew, endTimeNew, isAllDay, " (New)");
            AddLinksField(fields, eventName, startTimeNew);
            var embed = GetEventEmbed(baseAddress,
                                      eventName,
                                      Colors.Orange);
            embed.Url = GetEventUrl(baseAddress, appointmentId);
            embed.Description = $"An existing event was [rescheduled to a new occurence]({embed.Url}). " +
                                "If you're being mentioned, you've signed up to the old occurence. " +
                                "Click to open the new event occurence in your browser and to sign-up again!";
            embed.Fields = fields.ToArray();

            if (usersToNotify != null && usersToNotify.Any())
            {
                await _discordAccess.SendUnitsNotificationAsync(embed, usersToNotify);
            }
            else
            {
                await _discordAccess.SendUnitsNotificationAsync(embed);
            }
        }

        async Task IUnitsBotClient.ReceiveEventCanceledMessageAsync(string baseAddress,
                                                                    string eventName,
                                                                    DateTime startTime,
                                                                    DateTime endTime,
                                                                    bool isAllDay,
                                                                    DiscordUserID[] usersToNotify)
        {
            var fields = new List<EmbedField>();
            AddTimeField(fields, startTime, endTime, isAllDay, null);
            var embed = GetEventEmbed(baseAddress,
                                      eventName,
                                      Colors.Red);
            embed.Description = "An existing event was canceled in UNITS. " +
                                "If you're being mentioned, you've signed up to the canceled event.";
            embed.Fields = fields.ToArray();

            if (usersToNotify != null && usersToNotify.Any())
            {
                await _discordAccess.SendUnitsNotificationAsync(embed, usersToNotify);
            }
            else
            {
                await _discordAccess.SendUnitsNotificationAsync(embed);
            }
        }

        async Task IUnitsBotClient.ReceiveEventAttendanceConfirmedMessageAsync(string baseAddress,
                                                                               int appointmentId,
                                                                               DiscordUserID userToNotify)
        {
            var embed = GetEventEmbed(baseAddress,
                                      "Event attendance confirmed",
                                      Colors.Green);
            embed.Url = GetEventUrl(baseAddress, appointmentId);
            embed.Description = $"Your [event attendance]({embed.Url}) has been confirmed. " +
                                "Click to open the event in your browser.";
            await _discordAccess.SendUnitsNotificationAsync(embed, new[] {userToNotify});
        }

        async Task IUnitsBotClient.ReceiveEventStartingSoonMessageAsync(string baseAddress,
                                                                        int appointmentId,
                                                                        DateTime startTime,
                                                                        DiscordUserID[] usersToNotify)
        {
            var minutes = (int)Math.Ceiling((startTime - DateTime.UtcNow).TotalMinutes);
            var embed = GetEventEmbed(baseAddress,
                                      "Event starting soon",
                                      Colors.LightOrange);
            embed.Url = GetEventUrl(baseAddress, appointmentId);
            embed.Description = $"Your [event]({embed.Url}) is starting in {minutes} minutes. " +
                                "Click to open the event in your browser.";

            if (usersToNotify != null && usersToNotify.Any())
            {
                await _discordAccess.SendUnitsNotificationAsync(embed, usersToNotify);
            }
            else
            {
                await _discordAccess.SendUnitsNotificationAsync(embed);
            }
        }

        async Task IUnitsBotClient.ReceiveCreateEventVoiceChannelsMessageAsync(string baseAddress,
                                                                               int appointmentId,
                                                                               bool createGeneralVoiceChannel,
                                                                               byte maxAmountOfGroups,
                                                                               byte? maxGroupSize)
        {
            var unitsSyncData = _appSettings.UnitsAccess.SingleOrDefault(m => m.BaseAddress == baseAddress);
            if (unitsSyncData == null)
            {
                _logger.LogError($"Cannot find matching sync-endpoint in {nameof(AppSettings)}.{nameof(AppSettings.UnitsAccess)} " +
                                 "for base address {BaseAddress}.", baseAddress);
                return;
            }

            var voiceChannels = new List<EventVoiceChannel>();
            if (createGeneralVoiceChannel)
                voiceChannels.Add(new EventVoiceChannel(appointmentId));
            if (maxAmountOfGroups > 0 && maxGroupSize != null)
                for (byte groupNumber = 1; groupNumber <= maxAmountOfGroups; groupNumber++)
                    voiceChannels.Add(new EventVoiceChannel(appointmentId, groupNumber, maxGroupSize.Value));

            var failedVoiceChannels = new List<EventVoiceChannel>();
            foreach (var eventVoiceChannel in voiceChannels)
            {
                var (voiceChannelId, error) = await _discordAccess.CreateVoiceChannel(_appSettings.VoiceChannelCategoryId,
                                                                                      eventVoiceChannel.DisplayName,
                                                                                      eventVoiceChannel.MaxUsersInChannel);
                if (error != null)
                {
                    failedVoiceChannels.Add(eventVoiceChannel);
                    _logger.LogWarning("Failed to create voice channel for event '{AppointmentId}: {Error}'",
                                       appointmentId,
                                       error);
                }
                else
                {
                    eventVoiceChannel.DiscordVoiceChannelIdValue = voiceChannelId;
                }
                await Task.Delay(200);
            }

            foreach (var eventVoiceChannel in failedVoiceChannels)
                voiceChannels.Remove(eventVoiceChannel);

            if (voiceChannels.Any())
            {
                await _discordAccess.ReorderChannelsAsync(voiceChannels.Select(m => m.DiscordVoiceChannelIdValue).ToArray(),
                                                     _appSettings.UnitsEventVoiceChannelsPositionAboveChannelId);

                await _unitsAccess.SendCreatedVoiceChannelsAsync(unitsSyncData,
                                                              new SyncCreatedVoiceChannelsRequest(appointmentId, voiceChannels));
            }
        }

        async Task IUnitsBotClient.ReceiveDeleteEventVoiceChannelsMessageAsync(string[] voiceChannelIds)
        {
            if (voiceChannelIds == null)
                return;
            
            foreach (var voiceChannelIdStr in voiceChannelIds)
            {
                if (!ulong.TryParse(voiceChannelIdStr, out var voiceChannelId))
                    continue;

                try
                {
                    await _discordAccess.DeleteVoiceChannel(voiceChannelId);
                    _logger.LogInformation("Deleted voice channel {VoiceChannelId}.", voiceChannelId);
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e,
                                       "Failed to delete voice channel {VoiceChannelId}.",
                                       voiceChannelId);
                }
            }
        }

        async Task IUnitsBotClient.ReceiveGetCurrentAttendeesMessageAsync(string baseAddress,
                                                                          int appointmentId,
                                                                          int checkNumber,
                                                                          string[] voiceChannelIds)
        {
            if (voiceChannelIds == null || voiceChannelIds.Length == 0)
                return;

            var unitsSyncData = _appSettings.UnitsAccess.SingleOrDefault(m => m.BaseAddress == baseAddress);
            if (unitsSyncData == null)
            {
                _logger.LogError($"Cannot find matching sync-endpoint in {nameof(AppSettings)}.{nameof(AppSettings.UnitsAccess)} " +
                                 "for base address {BaseAddress}.", baseAddress);
                return;
            }

            var voiceChannelUsers = _discordAccess.GetUsersInVoiceChannels(voiceChannelIds);
            if (voiceChannelUsers.Any())
            {
                var request = new SyncCurrentAttendeesRequest(appointmentId,
                                                              checkNumber,
                                                              voiceChannelUsers.Select(m => new VoiceChannelAttendees(m.Key,
                                                                                           m.Value))
                                                                               .ToList());
                await _unitsAccess.SendCurrentAttendeesAsync(unitsSyncData,
                                                             request);
            }
        }
    }
}
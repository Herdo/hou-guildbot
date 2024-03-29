﻿namespace HoU.GuildBot.Shared.BLL;

public interface IUnitsBotClient
{
    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveEventCreatedMessageAsync(string baseAddress,
                                         int appointmentId,
                                         string eventName,
                                         string author,
                                         DateTime startTime,
                                         DateTime endTime,
                                         bool isAllDay,
                                         [CanBeNull] string cardUrl);

    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveEventRescheduledMessageAsync(string baseAddress, 
                                             int appointmentId,
                                             string eventName,
                                             DateTime startTimeOld,
                                             DateTime endTimeOld,
                                             DateTime startTimeNew,
                                             DateTime endTimeNew,
                                             bool isAllDay,
                                             DiscordUserId[] usersToNotify);

    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveEventCanceledMessageAsync(string baseAddress,
                                          string eventName,
                                          DateTime startTime,
                                          DateTime endTime,
                                          bool isAllDay,
                                          DiscordUserId[] usersToNotify);

    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveEventAttendanceConfirmedMessageAsync(string baseAddress,
                                                     int appointmentId,
                                                     string eventName,
                                                     DiscordUserId[] usersToNotify);

    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveEventStartingSoonMessageAsync(string baseAddress,
                                              int appointmentId,
                                              DateTime startTime,
                                              DiscordUserId[] usersToNotify);

    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveCreateEventVoiceChannelsMessageAsync(string baseAddress,
                                                     int appointmentId,
                                                     bool createGeneralVoiceChannel,
                                                     byte maxAmountOfGroups,
                                                     byte? maxGroupSize);

    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveDeleteEventVoiceChannelsMessageAsync(string[] voiceChannelIds);

    // See UnitsSignalRClient.RegisterHandlers
    [UsedImplicitly]
    Task ReceiveGetCurrentAttendeesMessageAsync(string baseAddress,
                                                int appointmentId,
                                                int checkNumber,
                                                string[] voiceChannelIds);
}
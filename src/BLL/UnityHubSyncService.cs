﻿namespace HoU.GuildBot.BLL;

[UsedImplicitly]
public class UnityHubSyncService
{
    //private readonly IDiscordAccess _discordAccess;
    //private readonly IUnityHubAccess _unityHubAccess;
    //private readonly ILogger<UnityHubSyncService> _logger;

    //public UnityHubSyncService(IDiscordAccess discordAccess,
    //                           IUnityHubAccess unityHubAccess,
    //                           ILogger<UnityHubSyncService> logger)
    //{
    //    _discordAccess = discordAccess ?? throw new ArgumentNullException(nameof(discordAccess));
    //    _unityHubAccess = unityHubAccess ?? throw new ArgumentNullException(nameof(unityHubAccess));
    //    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    //}

    //private void SanitizeUsers(IEnumerable<UserModel> users)
    //{
    //    foreach (var userModel in users)
    //    {
    //        // Remove "a_" prefix of animated avatar IDs
    //        if (userModel.AvatarId != null && userModel.AvatarId.StartsWith("a_"))
    //            userModel.AvatarId = userModel.AvatarId.Substring(2);
    //    }
    //}

    public Task SyncAllUsers()
    {
        return Task.CompletedTask;
        // TODO: Currently not supported.
        //if (string.IsNullOrWhiteSpace(_appSettings.UnityHubBaseAddress))
        //{
        //    _logger.LogWarning("UnityHub base address not configured.");
        //    return;
        //}

        //if (string.IsNullOrWhiteSpace(_appSettings.UnityHubAccessSecret))
        //{
        //    _logger.LogWarning("UnityHub access secret not configured.");
        //    return;
        //}

        //if (!_discordAccess.IsConnected || !_discordAccess.IsGuildAvailable)
        //    return;

        //var allowedRoles = _unityHubAccess.GetValidRoleNames();
        //var users = _discordAccess.GetUsersInRoles(allowedRoles);
        //if (users.Any())
        //{
        //    SanitizeUsers(users);
        //    var result = await _unityHubAccess.SendAllUsersAsync(users);
        //    if (result)
        //    {
        //        _logger.LogInformation("Successfully synchronized users with the Unity Hub.");
        //    }
        //    else
        //    {
        //        const string error = "Failed to synchronize users with the Unity Hub.";
        //        _logger.LogError(error);
        //        var leadershipMention = _discordAccess.GetLeadershipMention();
        //        await _discordAccess.LogToDiscord($"**{leadershipMention} - {error}**");
        //    }
        //}
        //else
        //{
        //    _logger.LogWarning("Failed to synchronize all users: {Reason}.", "unable to fetch allowed users");
        //}
    }
}
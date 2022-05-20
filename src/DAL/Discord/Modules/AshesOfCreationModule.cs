﻿using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using HoU.GuildBot.DAL.Discord.Preconditions;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.StrongTypes;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace HoU.GuildBot.DAL.Discord.Modules;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[Group("aoc", "Ashes of Creation related commands.")]
public class AshesOfCreationModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IImageProvider _imageProvider;
    private readonly ILogger<AshesOfCreationModule> _logger;

    public AshesOfCreationModule(IImageProvider imageProvider,
                                 ILogger<AshesOfCreationModule> logger)
    {
        _imageProvider = imageProvider;
        _logger = logger;
    }

    [SlashCommand("profile", "Shows the profile image for the current user.", runMode: RunMode.Async)]
    [AllowedRoles(Role.Developer)] // TODO: Change to any guild member
    public async Task GetProfileCardAsync()
    {
        await DeferAsync();

        try
        {
            await using var imageStream = await _imageProvider.CreateProfileImage((DiscordUserId)Context.User.Id,
                                                                                  Context.User.GetAvatarUrl(ImageFormat.Png));
            await Context.Interaction.FollowupWithFileAsync(imageStream,
                                                            $"{Context.User.Discriminator}_{DateTime.UtcNow:yyyyMMddHHmmss}.png");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create UNITS profile card image.");
            await FollowupAsync("Failed to create image.");
        }
    }

    [SlashCommand("archetype-combinations", "Shows a overview table of all possible archetype combinations in AoC.", runMode: RunMode.Async)]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task GetArchetypeCombinationsImageAsync()
    {
        await DeferAsync();

        try
        {
            await using var imageStream = _imageProvider.LoadClassListImage();
            await Context.Interaction.FollowupWithFileAsync(imageStream, "archetype-combinations.jpg");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to create the archetype combinations image.");
            await FollowupAsync("Failed to create image.");
        }
    }

    [SlashCommand("chart", "Shows various charts related to AoC member roles.", runMode: RunMode.Async)]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task GetAocRolesChartAsync(ChartType chartType)
    {
        await DeferAsync();
        try
        {
            Func<Stream> getImage = chartType switch
            {
                ChartType.Classes => () => _imageProvider.CreateAocClassDistributionImage(),
                ChartType.Races => () => _imageProvider.CreateAocRaceDistributionImage(),
                ChartType.PlayStyles => () => _imageProvider.CreateAocPlayStyleDistributionImage(),
                ChartType.GuildPreference => () => _imageProvider.CreateAocGuildPreferenceDistributionImage(),
                _ => throw new NotSupportedException($"Chart type {chartType} is not supported.")
            };
            await using var imageStream = getImage();
            await Context.Interaction.FollowupWithFileAsync(imageStream, "chart.png");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create {ChartType} chart.", chartType);
            await FollowupAsync("Failed to create chart.");
        }
    }

    /// <summary>
    /// Chart types that can be generated by <see cref="AshesOfCreationModule.GetAocRolesChartAsync"/>.
    /// </summary>
    public enum ChartType
    {
        Classes = 1,
        Races = 2,
        PlayStyles = 3,
        GuildPreference = 4
    }
}
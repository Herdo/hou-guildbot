﻿namespace HoU.GuildBot.DAL.Discord.Modules;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[Group("la", "Lost Ark related commands.")]
public class LostArkModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IImageProvider _imageProvider;
    private readonly ILogger<LostArkModule> _logger;

    public LostArkModule(IImageProvider imageProvider,
                         ILogger<LostArkModule> logger)
    {
        _imageProvider = imageProvider;
        _logger = logger;
    }

    [SlashCommand("chart", "Shows classes charts for Lost ark.", runMode: RunMode.Async)]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task GetLostArkRolesChartAsync(ChartType chartType = ChartType.PlayStyles)
    {
        await DeferAsync();
        try
        {
            Func<Stream> getImage = chartType switch
            {
                ChartType.PlayStyles => () => _imageProvider.CreateLostArkPlayStyleDistributionImage(),
                _ => throw new NotSupportedException($"Chart type {chartType} is not supported.")
            };
            await using var imageStream = getImage();
            await Context.Interaction.FollowupWithFileAsync(imageStream, "chart.png");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create Lost Ark class distribution chart");
            await FollowupAsync("Failed to create chart.");
        }
    }

    /// <summary>
    /// Chart types that can be generated by <see cref="LostArkModule.GetLostArkRolesChartAsync"/>.
    /// </summary>
    public enum ChartType
    {
        PlayStyles = 1
    }
}
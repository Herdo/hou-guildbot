﻿namespace HoU.GuildBot.DAL.Discord.Modules;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[Group("tnl", "Throne and Liberty related commands.")]
public class ThroneAndLibertyModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IImageProvider _imageProvider;
    private readonly ILogger<ThroneAndLibertyModule> _logger;

    public ThroneAndLibertyModule(IImageProvider imageProvider,
                                  ILogger<ThroneAndLibertyModule> logger)
    {
        _imageProvider = imageProvider;
        _logger = logger;
    }

    [SlashCommand("chart", "Shows various charts related to TnL member roles.", runMode: RunMode.Async)]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task GetTnlRolesChartAsync(ChartType chartType)
    {
        await DeferAsync();
        try
        {
            Func<Stream> getImage = chartType switch
            {
                ChartType.RolePreference => () => _imageProvider.CreateTnlRolePreferenceDistributionImage(),
                ChartType.Weapon => () => _imageProvider.CreateTnlWeaponDistributionImage(),
                _ => throw new NotSupportedException($"Chart type {chartType} is not supported.")
            };
            await using var imageStream = getImage();
            await Context.Interaction.FollowupWithFileAsync(imageStream, "chart.png");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create {ChartType} chart", chartType);
            await FollowupAsync("Failed to create chart.");
        }
    }

    /// <summary>
    /// Chart types that can be generated by <see cref="ThroneAndLibertyModule.GetTnlRolesChartAsync"/>.
    /// </summary>
    public enum ChartType
    {
        RolePreference = 1,
        Weapon = 2
    }
}
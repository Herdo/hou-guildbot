﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using JetBrains.Annotations;
using HoU.GuildBot.DAL.Discord.Preconditions;
using HoU.GuildBot.Shared.Attributes;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.Extensions;
using HoU.GuildBot.Shared.Objects;
using Microsoft.Extensions.Logging;

namespace HoU.GuildBot.DAL.Discord.Modules
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class InfoModule : ModuleBaseHoU
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IBotInformationProvider _botInformationProvider;
        private readonly ITimeInformationProvider _timeInformationProvider;
        private readonly ILogger<InfoModule> _logger;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public InfoModule(IBotInformationProvider botInformationProvider,
                          ITimeInformationProvider timeInformationProvider,
                          ILogger<InfoModule> logger)
        {
            _botInformationProvider = botInformationProvider;
            _timeInformationProvider = timeInformationProvider;
            _logger = logger;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Commands

        [Command("info")]
        [CommandCategory(CommandCategory.Administration, 1)]
        [Name("Get bot information")]
        [Summary("Gets information about the current bot instance.")]
        [Alias("information")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.Developer | Role.Leader)]
        public async Task InfoAsync()
        {
            _logger.LogDebug("Received \"info\" command request ...");
            var data = _botInformationProvider.GetData();
            var embed = data.ToEmbed();
            await ReplyAsync(string.Empty, false, embed).ConfigureAwait(false);
        }

        [Command("time")]
        [CommandCategory(CommandCategory.MemberInformation, 5)]
        [Name("Get guild times")]
        [Summary("Gets a list of guild time zones.")]
        [Alias("times", "timezone", "timezones", "guildtime", "guild time", "guildtimes", "guild times")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.AnyGuildMember)]
        public async Task TimeAsync()
        {
            var tz = _timeInformationProvider.GetCurrentTimeFormattedForConfiguredTimezones();
            var markdownBuilder = new StringBuilder()
                                 .AppendLine("```md")
                                 .AppendLine("Current Guild Times")
                                 .AppendLine("===================");
            foreach (var s in tz)
                markdownBuilder.AppendLine(s);
            markdownBuilder.AppendLine("```");
            await ReplyAsync(markdownBuilder.ToString()).ConfigureAwait(false);
        }

        [Command("fonts")]
        [CommandCategory(CommandCategory.Administration, 13)]
        [Name("List available fonts")]
        [Summary("Lists all available fonts.")]
        [Alias("listfonts", "list fonts")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.Developer)]
        public async Task FontsAsync()
        {
            var fonts = _botInformationProvider.GetAvailableFonts();
            var messages = new List<string>();
            foreach (var kvp in fonts)
            {
                var markdownBuilder = new StringBuilder()
                                     .AppendLine("```md")
                                     .AppendLine($"Available Fonts ({kvp.Key + 1}/{fonts.Keys.Count})")
                                     .AppendLine("===============");
                foreach (var f in kvp.Value)
                    markdownBuilder.AppendLine(f);
                markdownBuilder.AppendLine("```");
                messages.Add(markdownBuilder.ToString());
            }
            await messages.PerformBulkOperation(async s => await ReplyAsync(s).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Command("restart")]
        [CommandCategory(CommandCategory.Administration, 14)]
        [Name("Restarts the bot")]
        [Summary("Triggers a shutdown of the bot, resulting in a restart due to the container configuration.")]
        [Alias("reboot", "shutdown")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.Developer)]
        public async Task Restart()
        {
            _logger.LogInformation("Shutdown triggered by 'restart' command. Application will shut down in 10 seconds ...");
            await ReplyAsync("Shutting down in 10 seconds. Restart will be performed automatically.");
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                ApplicationLifecycle.End();
            }).ConfigureAwait(false);
        }

        #endregion
    }
}
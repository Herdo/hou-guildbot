﻿using System.Threading.Tasks;
using global::Discord.Commands;
using JetBrains.Annotations;
using HoU.GuildBot.DAL.Discord.Preconditions;
using HoU.GuildBot.Shared.Attributes;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.Extensions;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.DAL.Discord.Modules
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class HelpModule : ModuleBaseHoU
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IHelpProvider _helpProvider;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public HelpModule(IHelpProvider helpProvider)
        {
            _helpProvider = helpProvider;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Commands

        [Command("help")]
        [CommandCategory(CommandCategory.Help, 1)]
        [Name("Get command help")]
        [Summary("Provides help for commands.")]
        [Remarks("If no further arguments are provided, this command will list all available commands.")]
        [Alias("?")]
        [RequireContext(ContextType.DM | ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysDirect)]
        [RolePrecondition(Role.AnyGuildMember)]
        public Task HelpAsync([Remainder] string helpRequest = null)
        {
            var data = _helpProvider.GetHelp((DiscordUserID) Context.User.Id, helpRequest);
#pragma warning disable 4014 // Fire & Forget
            Task.Run(async () => await data.PerformBulkOperation(async t =>
            {
                var embed = t.EmbedData?.ToEmbed();
                await ReplyPrivateAsync(t.Message, embed).ConfigureAwait(false);
            }).ConfigureAwait(false));
#pragma warning restore 4014 // Fire & Forget
            return Task.CompletedTask;
        }

        #endregion
    }
}
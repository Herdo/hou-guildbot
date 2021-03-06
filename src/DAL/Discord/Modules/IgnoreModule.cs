﻿using System.Threading.Tasks;
using global::Discord.Commands;
using JetBrains.Annotations;
using HoU.GuildBot.DAL.Discord.Preconditions;
using HoU.GuildBot.Shared.Attributes;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.DAL.Discord.Modules
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class IgnoreModule : ModuleBaseHoU
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IIgnoreGuard _ignoreGuard;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public IgnoreModule(IIgnoreGuard ignoreGuard)
        {
            _ignoreGuard = ignoreGuard;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Commands

        [Command("ignore me")]
        [CommandCategory(CommandCategory.Administration, 2)]
        [Name("Start ignoring the user")]
        [Summary("Requests the bot instance to ignore further commands from the user for a certain duration.")]
        [Remarks("The minimum ignore duration is 3 minutes, the maximum ignore duration is 60 minutes.")]
        [Alias("ignoreme")]
        [RequireContext(ContextType.DM | ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysDirect)]
        [RolePrecondition(Role.Developer)]
        public async Task IgnoreMeAsync([Remainder] string remainder)
        {
            var result = _ignoreGuard.TryAddToIgnoreList((DiscordUserID)Context.User.Id, Context.User.Username, remainder);
            var embed = result.ToEmbed();
            await ReplyPrivateAsync(string.Empty, embed).ConfigureAwait(false);
        }

        [Command("notice me")]
        [CommandCategory(CommandCategory.Administration, 3)]
        [Name("Stop ignore the user")]
        [Summary("Requests the bot instance to accept commands from the current user again.")]
        [Remarks("If the user was not ignored by the bot, he won't receive any message. The bot will only respond during the ignored time.")]
        [Alias("noticeme")]
        [RequireContext(ContextType.DM | ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysDirect)]
        [RolePrecondition(Role.Developer)]
        public async Task NoticeMeAsync()
        {
            var result = _ignoreGuard.TryRemoveFromIgnoreList((DiscordUserID)Context.User.Id, Context.User.Username);
            if (result != null)
            {
                var embed = result.ToEmbed();
                await ReplyPrivateAsync(string.Empty, embed).ConfigureAwait(false);
            }
        }

        #endregion
    }
}
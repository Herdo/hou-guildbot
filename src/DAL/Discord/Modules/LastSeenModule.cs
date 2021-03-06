﻿using System.Text;
using System.Threading.Tasks;
using global::Discord.Commands;
using JetBrains.Annotations;
using HoU.GuildBot.DAL.Discord.Preconditions;
using HoU.GuildBot.Shared.Attributes;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.Enums;

namespace HoU.GuildBot.DAL.Discord.Modules
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class LastSeenModule : ModuleBaseHoU
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IUserInfoProvider _userInfoProvider;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public LastSeenModule(IUserInfoProvider userInfoProvider)
        {
            _userInfoProvider = userInfoProvider;
        }

        #endregion
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Commands

        [Command("last seen", RunMode = RunMode.Async)]
        [CommandCategory(CommandCategory.MemberInformation, 2)]
        [Name("Shows last seen timestamps")]
        [Summary("Lists all users and the timestamp of their last text message.")]
        [Remarks("List is ordered by descanding activity (those who are away the longest are on the top).")]
        [Alias("lastseen")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.Leader | Role.Officer)]
        public async Task LastSeenAsync()
        {
            var message = await ReplyAsync("Loading...").ConfigureAwait(false);

            var data = await _userInfoProvider.GetLastSeenInfo().ConfigureAwait(false);

            var buffer = new StringBuilder();

            foreach (var s in data)
            {
                if (buffer.Length + s.Length < 2000)
                {
                    buffer.AppendLine(s);
                }
                else
                {
                    // Flush buffer
                    await ReplyAsync(buffer.ToString()).ConfigureAwait(false);
                    buffer.Clear();
                    buffer.AppendLine(s);
                }
            }

            if (buffer.Length > 0)
                await ReplyAsync(buffer.ToString()).ConfigureAwait(false);

            await message.DeleteAsync().ConfigureAwait(false);
        }

        #endregion
    }
}
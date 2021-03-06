﻿using System;
using System.Threading.Tasks;
using global::Discord.Commands;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.DAL.Discord.Preconditions
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RolePreconditionAttribute : PreconditionAttribute
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Properties

        public Role AllowedRoles { get; }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public RolePreconditionAttribute(Role allowedRoles)
        {
            AllowedRoles = allowedRoles;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Base Overrides

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var userStore = (IUserStore)services.GetService(typeof(IUserStore));
            if (!userStore.TryGetUser((DiscordUserID)context.User.Id, out var user))
                return Task.FromResult(PreconditionResult.FromError("Couldn't determine user permission roles."));
            var isAllowed = (AllowedRoles & user.Roles) != Role.NoRole;
            return Task.FromResult(isAllowed
                                       ? PreconditionResult.FromSuccess()
                                       : PreconditionResult.FromError($"**{context.User.Username}**: This command is not available for your current roles."));
        }

        #endregion
    }
}
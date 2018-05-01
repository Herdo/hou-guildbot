﻿namespace HoU.GuildBot.DAL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Logging;
    using Preconditions;
    using Shared.Attributes;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Enums;
    using Shared.Exceptions;
    using Shared.Objects;
    using CommandInfo = Discord.Commands.CommandInfo;

    [UsedImplicitly]
    public class DiscordAccess : IDiscordAccess
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly ILogger<DiscordAccess> _logger;
        private readonly AppSettings _appSettings;
        private readonly IServiceProvider _serviceProvider;
        private readonly ISpamGuard _spamGuard;
        private readonly IIgnoreGuard _ignoreGuard;
        private readonly ICommandRegistry _commandRegistry;
        private readonly IGuildUserRegistry _guildUserUserRegistry;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly Queue<string> _pendingMessages;

        private bool _guildAvailable;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public DiscordAccess(ILogger<DiscordAccess> logger,
                             AppSettings appSettings,
                             IServiceProvider serviceProvider,
                             ISpamGuard spamGuard,
                             IIgnoreGuard ignoreGuard,
                             ICommandRegistry commandRegistry,
                             IGuildUserRegistry guildUserUserRegistry)
        {
            _logger = logger;
            _appSettings = appSettings;
            _serviceProvider = serviceProvider;
            _spamGuard = spamGuard;
            _ignoreGuard = ignoreGuard;
            _commandRegistry = commandRegistry;
            _guildUserUserRegistry = guildUserUserRegistry;
            _commands = new CommandService();
            _client = new DiscordSocketClient();
            _pendingMessages = new Queue<string>();

            _client.Log += Log;
            _commands.Log += Log;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Private Methods

        /// <summary>
        /// Gets the "Hand of Unity" <see cref="SocketGuild"/> object.
        /// </summary>
        /// <returns>A <see cref="SocketGuild"/> instance for "Hand of Unity".</returns>
        private SocketGuild GetGuild() => _client.GetGuild(_appSettings.HandOfUnityGuildId);

        private async Task<bool> IsSpam(SocketMessage userMessage)
        {
            // If the message was received on a direct message channel, it's never spam
            if (userMessage.Channel is IPrivateChannel)
                return false;
            var checkResult = _spamGuard.CheckForSpam(userMessage.Author.Id, userMessage.Channel.Id, userMessage.Content);
            if (checkResult == SpamCheckResult.NoSpam)
                return false;

            var g = GetGuild();
            var leaderRole = GetRoleByName("Leader", g);
            var officerRole = GetRoleByName("Officer", g);

            switch (checkResult)
            {
                case SpamCheckResult.SoftLimitHit:
                {
                    var embedBuilder = new EmbedBuilder()
                                       .WithColor(Color.DarkPurple)
                                       .WithTitle("Spam warning")
                                       .WithDescription("Please refrain from further spamming in this channel.");
                    var embed = embedBuilder.Build();
                    await userMessage
                          .Channel.SendMessageAsync($"{userMessage.Author.Mention} - {leaderRole.Mention} and {officerRole.Mention} have been notified.", false, embed)
                          .ConfigureAwait(false);
                    return true;
                }
                case SpamCheckResult.BetweenSoftAndHardLimit:
                {
                    return true;
                }
                case SpamCheckResult.HardLimitHit:
                {
                    var guildUser = g.GetUser(userMessage.Author.Id);
                    try
                    {
                        await guildUser.KickAsync("Excesive spam.", RequestOptions.Default).ConfigureAwait(false);
                    }
                    catch (Discord.Net.HttpException e) when (e.HttpCode == HttpStatusCode.Forbidden)
                    {
                        await LogToDiscordInternal(
                                $"{leaderRole.Mention}, {officerRole.Mention}: Failed to kick user {guildUser.Mention}, because the bot is not permitted to kick a user with a higher rank.")
                            .ConfigureAwait(false);
                        return true;
                    }
                    catch (Exception e)
                    {
                        await LogToDiscordInternal(
                                $"{leaderRole.Mention}, {officerRole.Mention}: Failed to kick user {guildUser.Mention} due to an unexpected error: {e.Message}")
                            .ConfigureAwait(false);
                        return true;
                    }

                    await LogToDiscordInternal($"{leaderRole.Mention}, {officerRole.Mention}:Kicked user {guildUser.Mention} from the server due to excesive spam.").ConfigureAwait(false);
                    return true;
                }
            }

            return false;
        }

        private bool ShouldIgnore(SocketMessage userMessage)
        {
            return _ignoreGuard.ShouldIgnoreMessage(userMessage.Author.Id)
                && !userMessage.Content.Contains("notice me"); // Required to disable ignore duration early
        }

        private async Task ProcessMessage(SocketUserMessage userMessage)
        {
            var argPos = 0;
            if (userMessage.HasStringPrefix("hou!", ref argPos, StringComparison.OrdinalIgnoreCase) // Take action when the prefix is at the beginning
             || userMessage.HasMentionPrefix(_client.CurrentUser, ref argPos)) // Take action when the bot is mentioned
            {
                // If the message is a command, check for ignore-entries
                if (ShouldIgnore(userMessage)) return;

                var context = new SocketCommandContext(_client, userMessage);
                try
                {
                    var result = await _commands.ExecuteAsync(context, argPos, _serviceProvider).ConfigureAwait(false);
                    if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                    {
                        // Handle error during command execution
                        var embedBuilder = new EmbedBuilder()
                                           .WithColor(Color.Red)
                                           .WithTitle("Error during command execution")
                                           .WithDescription(result.ErrorReason);
                        var embed = embedBuilder.Build();
                        _logger.LogWarning(result.ErrorReason);
                        await userMessage.Channel.SendMessageAsync(string.Empty, false, embed).ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unexpected error during command processing");
                }
            }
        }

        private void RegisterCommands()
        {
            Shared.Objects.CommandInfo ToSharedCommandInfo(CommandInfo ci)
            {
                // Get required context
                var rt = RequestType.Undefined;
                var requiredContext = ci.Preconditions.OfType<RequireContextAttribute>().SingleOrDefault();
                if (requiredContext != null)
                {
                    if (requiredContext.Contexts.HasFlag(ContextType.Guild))
                        rt = rt | RequestType.GuildChannel;
                    if (requiredContext.Contexts.HasFlag(ContextType.DM))
                        rt = rt | RequestType.DirectMessage;
                }

                // Get response type
                var responseType = ci.Attributes.OfType<ResponseContextAttribute>().SingleOrDefault();
                var resp = responseType?.ResponseType ?? ResponseType.Undefined;

                // Get required role
                var requiredRoles = ci.Preconditions.OfType<RolePreconditionAttribute>().SingleOrDefault();
                var rr = requiredRoles?.AllowedRoles ?? Role.NoRole;

                // Create POCO
                return new Shared.Objects.CommandInfo(ci.Name, ci.Aliases.ToArray(), rt, resp, rr)
                {
                    Summary = ci.Summary,
                    Remarks = ci.Remarks
                };
            }
            _commandRegistry.RegisterAndValidateCommands(_commands.Commands.Select(ToSharedCommandInfo).ToArray());
        }

        private static Role SocketRoleToRole(IReadOnlyCollection<SocketRole> roles)
        {
            var r = Role.NoRole;
            foreach (var socketRole in roles)
            {
                if (Enum.TryParse(socketRole.Name, out Role role))
                    r = r | role;
            }

            return r;
        }

        private async Task LogToDiscordInternal(string message)
        {
            var g = GetGuild();
            var lc = g.GetTextChannel(_appSettings.LoggingChannelId);
            if (lc == null)
            {
                // Channel can be null because the guild is currently unavailable.
                // In this case, store the messages in a queue and send them later.
                // Otherwise throw.
                if (_guildAvailable)
                    throw new ChannelNotFoundException(_appSettings.LoggingChannelId);
                _pendingMessages.Enqueue(message);
                return;
            }

            await lc.SendMessageAsync(message).ConfigureAwait(false);
        }

        private SocketGuildUser GetGuildUserById(ulong userId)
        {
            var g = GetGuild();
            return g.GetUser(userId);
        }

        private IRole GetRoleByName(string name, SocketGuild guild = null)
        {
            var g = guild ?? GetGuild();
            return g.Roles.Single(m => m.Name == name);
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IDiscordAccess Members

        async Task IDiscordAccess.Connect(Func<Task> connectedHandler, Func<Exception, Task> disconnectedHandler)
        {
            Func<Task> ClientConnected()
            {
                return async () =>
                {
                    await _commands.AddModulesAsync(typeof(DiscordAccess).Assembly).ConfigureAwait(false);
                    RegisterCommands();
                    _client.MessageReceived += Client_MessageReceived;
                    await connectedHandler().ConfigureAwait(false);
                };
            }

            Func<Exception, Task> ClientDisconnected()
            {
                return exception =>
                {
                    _client.MessageReceived -= Client_MessageReceived;
                    return disconnectedHandler(exception);
                };
            }

            if (connectedHandler == null)
                throw new ArgumentNullException(nameof(connectedHandler));
            if (disconnectedHandler == null)
                throw new ArgumentNullException(nameof(disconnectedHandler));

            try
            {
                _logger.LogInformation("Connecting to Discord...");
                _client.Connected -= ClientConnected();
                _client.Connected += ClientConnected();
                _client.Disconnected -= ClientDisconnected();
                _client.Disconnected += ClientDisconnected();
                _client.GuildAvailable -= Client_GuildAvailable;
                _client.GuildAvailable += Client_GuildAvailable;
                _client.GuildUnavailable -= Client_GuildUnavailable;
                _client.GuildUnavailable += Client_GuildUnavailable;
                _client.UserJoined -= Client_UserJoined;
                _client.UserJoined += Client_UserJoined;
                _client.UserLeft -= Client_UserLeft;
                _client.UserLeft += Client_UserLeft;
                _client.GuildMemberUpdated -= Client_GuildMemberUpdated;
                _client.GuildMemberUpdated += Client_GuildMemberUpdated;

                await _client.LoginAsync(TokenType.Bot, _appSettings.BotToken).ConfigureAwait(false);
                await _client.StartAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error while connecting to Discord.");
            }
        }

        async Task IDiscordAccess.SetCurrentGame(string gameName)
        {
            await _client.SetGameAsync(gameName).ConfigureAwait(false);
        }

        async Task IDiscordAccess.LogToDiscord(string message)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException(nameof(message));

            await LogToDiscordInternal(message).ConfigureAwait(false);
        }

        async Task IDiscordAccess.AssignRole(ulong userId, Role role)
        {
            var gu = GetGuildUserById(userId);
            var r = GetRoleByName(role.ToString());
            await gu.AddRoleAsync(r).ConfigureAwait(false);
            await LogToDiscordInternal($"Assigned role '{role}' to {gu.Username}.").ConfigureAwait(false);
        }

        async Task IDiscordAccess.RevokeRole(ulong userId, Role role)
        {
            var gu = GetGuildUserById(userId);
            var r = GetRoleByName(role.ToString());
            await gu.RemoveRoleAsync(r).ConfigureAwait(false);
            await LogToDiscordInternal($"Revoked role '{role}' from {gu.Username}.").ConfigureAwait(false);
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Event Handler

        private Task Log(LogMessage arg)
        {
            switch (arg.Severity)
            {
                case LogSeverity.Critical:
                    _logger.LogCritical(arg.Exception, arg.Message);
                    break;
                case LogSeverity.Error:
                    _logger.LogError(arg.Exception, arg.Message);
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(arg.Exception, arg.Message);
                    break;
                case LogSeverity.Info:
                    _logger.LogInformation(arg.Exception, arg.Message);
                    break;
                case LogSeverity.Verbose:
                    _logger.LogTrace(arg.Exception, arg.Message);
                    break;
                case LogSeverity.Debug:
                    _logger.LogDebug(arg.Exception, arg.Message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Task.CompletedTask;
        }

        private async Task Client_GuildAvailable(SocketGuild guild)
        {
            if (guild.Id != _appSettings.HandOfUnityGuildId)
                return;

            _guildAvailable = true;
            _guildUserUserRegistry.AddGuildUsers(guild.Users.Select(m => (m.Id, SocketRoleToRole(m.Roles))));

            while (_pendingMessages.Count > 0)
            {
                var pm = _pendingMessages.Dequeue();
                await LogToDiscordInternal(pm).ConfigureAwait(false);
            }
        }

        private Task Client_GuildUnavailable(SocketGuild guild)
        {
            if (guild.Id != _appSettings.HandOfUnityGuildId)
                return Task.CompletedTask;

            _guildAvailable = false;
            _guildUserUserRegistry.RemoveGuildUsers(guild.Users.Select(m => m.Id));

            return Task.CompletedTask;
        }

        private Task Client_UserJoined(SocketGuildUser guildUser)
        {
            if (guildUser.Guild.Id != _appSettings.HandOfUnityGuildId)
                return Task.CompletedTask;

            _guildUserUserRegistry.AddGuildUser(guildUser.Id, SocketRoleToRole(guildUser.Roles));

            return Task.CompletedTask;
        }

        private Task Client_UserLeft(SocketGuildUser guildUser)
        {
            if (guildUser.Guild.Id != _appSettings.HandOfUnityGuildId)
                return Task.CompletedTask;

            _guildUserUserRegistry.RemoveGuildUser(guildUser.Id);

            return Task.CompletedTask;
        }

        private Task Client_GuildMemberUpdated(SocketGuildUser oldGuildUser, SocketGuildUser newGuildUser)
        {
            if (oldGuildUser.Guild.Id != _appSettings.HandOfUnityGuildId)
                return Task.CompletedTask;
            if (newGuildUser.Guild.Id != _appSettings.HandOfUnityGuildId)
                return Task.CompletedTask;
            if (oldGuildUser.Id != newGuildUser.Id)
                return Task.CompletedTask;

            _guildUserUserRegistry.UpdateGuildUser(newGuildUser.Id, SocketRoleToRole(newGuildUser.Roles));

            return Task.CompletedTask;
        }

        private async Task Client_MessageReceived(SocketMessage message)
        {
            // If the message is not a user message, we don't need to handle it
            SocketUserMessage userMessage;
            if ((userMessage = message as SocketUserMessage) == null) return;

            // If the message is from this bot, or any other bot, we don't need to handle it
            if (userMessage.Author.Id == _client.CurrentUser.Id || userMessage.Author.IsBot) return;
            
            // Check for spam
            if (await IsSpam(userMessage).ConfigureAwait(false)) return;
            
            // If the message is no spam, process message
            await ProcessMessage(userMessage).ConfigureAwait(false);
        }

        #endregion
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Objects;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Serilog;

namespace HoU.GuildBot.DAL.UNITS
{
    public class UnitsSignalRClient : IUnitsSignalRClient
    {
        private readonly IUnitsBearerTokenManager _bearerTokenManager;
        private readonly IUnitsBotClient _botClient;
        private readonly ILogger<UnitsSignalRClient> _logger;
        private const string BotHubRoute = "/bot-hub";

        private readonly Dictionary<string, HubConnection> _hubConnections;
        private readonly Dictionary<string, bool> _requiresTokenRefresh;

        private readonly Dictionary<string, HttpClient> _authHttpClients;

        public UnitsSignalRClient(IUnitsBearerTokenManager bearerTokenManager,
                                  IUnitsBotClient botClient,
                                  ILogger<UnitsSignalRClient> logger)
        {
            _bearerTokenManager = bearerTokenManager ?? throw new ArgumentNullException(nameof(bearerTokenManager));
            _botClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _hubConnections = new Dictionary<string, HubConnection>();
            _requiresTokenRefresh = new Dictionary<string, bool>();
            _authHttpClients = new Dictionary<string, HttpClient>();
        }

        private HttpClient GetHttpClient(string baseAddress)
        {
            if (_authHttpClients.TryGetValue(baseAddress, out var httpClient))
                return httpClient;

            var httpClientHandler = new HttpClientHandler
            {
#if DEBUG
                ServerCertificateCustomValidationCallback = (message,
                                                             certificate2,
                                                             arg3,
                                                             arg4) => true
#endif
            };
            httpClient = new HttpClient(httpClientHandler) { BaseAddress = new Uri(baseAddress) };
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept
                      .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _authHttpClients[baseAddress] = httpClient;
            return httpClient;
        }

        private void RegisterHandlers(HubConnection connection,
                                      string baseAddress)
        {
            var methods = typeof(IUnitsBotClient)
                         .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                         .Where(m => m.Name.StartsWith("Receive")
                                     && m.ReturnType == typeof(Task))
                         .ToArray();
            foreach (var method in methods)
            {
                var allParameters = method.GetParameters();
                var firstParameter = allParameters.FirstOrDefault();
                var withBaseAddress = firstParameter != null
                                      && firstParameter.Name == "baseAddress"
                                      && firstParameter.ParameterType == typeof(string);
                if (withBaseAddress)
                {
                    connection.On(method.Name,
                                  allParameters.Skip(1).Select(m => m.ParameterType).ToArray(),
                                  async parameters =>
                                  {
                                      if (method.Invoke(_botClient, new object[] { baseAddress }.Concat(parameters).ToArray()) is Task response)
                                          await response;
                                  });
                }
                else
                {
                    connection.On(method.Name,
                                  allParameters.Select(m => m.ParameterType).ToArray(),
                                  async parameters =>
                                  {
                                      if (method.Invoke(_botClient, parameters) is Task response)
                                          await response;
                                  });
                }
            }
        }

        async Task IUnitsSignalRClient.ConnectAsync(UnitsSyncData unitsSyncData)
        {
            if (_hubConnections.ContainsKey(unitsSyncData.BaseAddress))
                return;

            var hubRoute = unitsSyncData.BaseAddress + BotHubRoute;
            var connection = new HubConnectionBuilder()
                            .WithUrl(hubRoute,
                                     options =>
                                     {
#if DEBUG
                                         options.WebSocketConfiguration = conf =>
                                         {
                                             conf.RemoteCertificateValidationCallback = (sender,
                                                                                         certificate,
                                                                                         chain,
                                                                                         errors) => true;
                                         };
                                         options.HttpMessageHandlerFactory = handler =>
                                         {
                                             if (handler is HttpClientHandler clientHandler)
                                             {
                                                 clientHandler.ServerCertificateCustomValidationCallback = (message,
                                                                                                             certificate2,
                                                                                                             arg3,
                                                                                                             arg4) => true;
                                             }

                                             return handler;
                                         };
#endif
                                         options.AccessTokenProvider = async () =>
                                         {
                                             var token =
                                                 await _bearerTokenManager.GetBearerTokenAsync(GetHttpClient(unitsSyncData.BaseAddress),
                                                                                               unitsSyncData.BaseAddress,
                                                                                               unitsSyncData.Secret,
                                                                                               _requiresTokenRefresh
                                                                                                   [unitsSyncData.BaseAddress]);
                                             _requiresTokenRefresh[unitsSyncData.BaseAddress] = false;
                                             return token;
                                         };
                                     })
                            .ConfigureLogging(logging =>
                            {
                                logging.SetMinimumLevel(LogLevel.Trace);
                                logging.AddSerilog();
                            })
                            .Build();
            connection.Closed += async (error) =>
            {
                _logger.LogWarning("Connection to UNITS at {BaseUrl} closed. Trying to reconnect ...", unitsSyncData.BaseAddress);
                do
                {
                    try
                    {
                        await Task.Delay(new Random().Next(0, 5) * 10_000);
                        if (connection.State != HubConnectionState.Connecting)
                            await connection.StartAsync();
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to re-connect to UNITS at {BaseUrl}.", unitsSyncData.BaseAddress);
                        if (e is HttpRequestException httpRequestException && httpRequestException.Message.Contains("401"))
                        {
                            _requiresTokenRefresh[unitsSyncData.BaseAddress] = true;
                        }
                    }
                } while (connection.State != HubConnectionState.Connected);
            };

            RegisterHandlers(connection,
                             unitsSyncData.BaseAddress);

            _requiresTokenRefresh.Add(unitsSyncData.BaseAddress, false);
            _hubConnections.Add(unitsSyncData.BaseAddress, connection);

            _logger.LogInformation("Connecting to UNITS at {BaseUrl} ...", unitsSyncData.BaseAddress);
            do
            {
                try
                {
                    await connection.StartAsync();
                    
                    if (connection.State == HubConnectionState.Connected)
                        _logger.LogInformation("Connected to UNITS at {BaseUrl}.", unitsSyncData.BaseAddress);
                }
                catch (Exception e)
                {
                    var delaySeconds = new Random().Next(5, 20);
                    _logger.LogError(e, "Failed to initially connect to UNITS at {BaseUrl}. Trying again in {Seconds} seconds ...",
                                     unitsSyncData.BaseAddress,
                                     delaySeconds);
                    await Task.Delay(delaySeconds * 1000);
                }
            } while (connection.State != HubConnectionState.Connected);
        }
    }
}
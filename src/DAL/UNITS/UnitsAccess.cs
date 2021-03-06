﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Extensions;
using HoU.GuildBot.Shared.Objects;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HoU.GuildBot.DAL.UNITS
{
    public class UnitsAccess : IUnitsAccess
    {
        private readonly IUnitsBearerTokenManager _bearerTokenManager;
        private readonly ILogger<UnitsAccess> _logger;

        public UnitsAccess(IUnitsBearerTokenManager bearerTokenManager,
                           ILogger<UnitsAccess> logger)
        {
            _bearerTokenManager = bearerTokenManager ?? throw new ArgumentNullException(nameof(bearerTokenManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task UseHttpClient(Func<HttpClient, Task<HttpResponseMessage>> invokeHttpRequest,
                                         Func<HttpResponseMessage, Task> handleResult,
                                         string baseAddress,
                                         string secret)
        {
            using (var httpClientHandler = new HttpClientHandler())
            {
#if DEBUG
                httpClientHandler.ServerCertificateCustomValidationCallback = (httpRequestMessage,
                                                                               certificate,
                                                                               chain,
                                                                               policyErrors) => true;
#endif
                using (var httpClient = new HttpClient(httpClientHandler) {BaseAddress = new Uri(baseAddress)})
                {
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var tokenSet = await _bearerTokenManager.GetAndSetBearerToken(httpClient, baseAddress, secret, false);
                    if (!tokenSet)
                    {
                        // If the Bearer token is not set, the HTTP request will fail anyway.
                        // Instead of invoking it, just return.
                        return;
                    }

                    var response = await invokeHttpRequest(httpClient);

                    if (response == null || response.IsSuccessStatusCode)
                    {
                        // If the result is null, or a success, invoke the result handler.
                        await handleResult(response);
                        return;
                    }

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        // If the first response status code is Unauthorized (401), the token might either be expired, or invalid.
                        var isExpired = response.Headers.TryGetValues("Token-Expired", out var expiredTokenValues) &&
                                        expiredTokenValues.Any(m => m?.ToLowerInvariant() == "true");
                        if (isExpired)
                        {
                            // If the token is expired, refresh the token.
                            tokenSet = await _bearerTokenManager.GetAndSetBearerToken(httpClient, baseAddress, secret, true);
                            if (!tokenSet)
                            {
                                // If the Bearer token is not set, the HTTP request will fail anyway.
                                // Instead of invoking it, just return.
                                return;
                            }

                            // If the token is set, perform the request again.
                            response = await invokeHttpRequest(httpClient);
                        }
                    }

                    // If the status code is no success status code, and not Unauthorized (401), or was 401 and was invoked again with a refreshed token, handle the result here.
                    await handleResult(response);
                }
            }
        }

        async Task<string[]> IUnitsAccess.GetValidRoleNamesAsync(UnitsSyncData unitsSyncData)
        {
            const string requestPath = "/bot-api/discordsync/valid-role-names";

            string[] methodResult = null;

            async Task<HttpResponseMessage> ExecuteHttpCall(HttpClient httpClient)
            {
                try
                {
                    return await httpClient.GetAsync(requestPath);
                }
                catch (HttpRequestException e)
                {
                    var baseExceptionMessage = e.GetBaseException().Message;
                    _logger.LogRequestError(unitsSyncData.BaseAddress, requestPath, baseExceptionMessage);
                    return null;
                }
            }

            async Task HandleResponseMessage(HttpResponseMessage responseMessage)
            {
                if (responseMessage == null)
                    return;
                if (!responseMessage.IsSuccessStatusCode)
                {
                    await _logger.LogRequestErrorAsync(unitsSyncData.BaseAddress, requestPath, responseMessage);
                    return;
                }

                var stringContent = await responseMessage.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<string[]>(stringContent);
                methodResult = result;
            }

            await UseHttpClient(ExecuteHttpCall, HandleResponseMessage, unitsSyncData.BaseAddress, unitsSyncData.Secret);

            return methodResult;
        }

        async Task<SyncAllUsersResponse> IUnitsAccess.SendAllUsersAsync(UnitsSyncData unitsSyncData,
                                                                        UserModel[] users)
        {
            const string requestPath = "/bot-api/discordsync/all-users";
            SyncAllUsersResponse methodResult = null;

            var request = new SyncAllUsersRequest
            {
                Users = users
            };
            var serialized = JsonConvert.SerializeObject(request);
            var requestContent = new StringContent(serialized, Encoding.UTF8, "application/json");

            await UseHttpClient(ExecuteHttpCall, HandleResponseMessage, unitsSyncData.BaseAddress, unitsSyncData.Secret);

            return methodResult;

            async Task<HttpResponseMessage> ExecuteHttpCall(HttpClient httpClient)
            {
                try
                {
                    return await httpClient.PostAsync(requestPath, requestContent);
                }
                catch (HttpRequestException e)
                {
                    var baseExceptionMessage = e.GetBaseException().Message;
                    _logger.LogRequestError(unitsSyncData.BaseAddress, requestPath, baseExceptionMessage);
                    return null;
                }
            }

            async Task HandleResponseMessage(HttpResponseMessage responseMessage)
            {
                if (responseMessage == null)
                    return;
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    methodResult = JsonConvert.DeserializeObject<SyncAllUsersResponse>(responseContent);
                }
                else
                {
                    await _logger.LogRequestErrorAsync(unitsSyncData.BaseAddress, requestPath, responseMessage);
                }
            }
        }

        async Task IUnitsAccess.SendCreatedVoiceChannelsAsync(UnitsSyncData unitsSyncData,
                                                              SyncCreatedVoiceChannelsRequest createdVoiceChannelsRequest)
        {
            const string requestPath = "/bot-api/discordsync/created-voice-channels";
            var serialized = JsonConvert.SerializeObject(createdVoiceChannelsRequest);
            var requestContent = new StringContent(serialized, Encoding.UTF8, "application/json");

            await UseHttpClient(ExecuteHttpCall, HandleResponseMessage, unitsSyncData.BaseAddress, unitsSyncData.Secret);

            async Task<HttpResponseMessage> ExecuteHttpCall(HttpClient httpClient)
            {
                try
                {
                    return await httpClient.PostAsync(requestPath, requestContent);
                }
                catch (HttpRequestException e)
                {
                    var baseExceptionMessage = e.GetBaseException().Message;
                    _logger.LogRequestError(unitsSyncData.BaseAddress, requestPath, baseExceptionMessage);
                    return null;
                }
            }

            async Task HandleResponseMessage(HttpResponseMessage responseMessage)
            {
                if (responseMessage == null)
                    return;
                if (!responseMessage.IsSuccessStatusCode)
                    await _logger.LogRequestErrorAsync(unitsSyncData.BaseAddress, requestPath, responseMessage);
            }
        }

        async Task IUnitsAccess.SendCurrentAttendeesAsync(UnitsSyncData unitsSyncData,
                                                          SyncCurrentAttendeesRequest currentAttendeesRequest)
        {
            const string requestPath = "/bot-api/discordsync/current-attendees";
            var serialized = JsonConvert.SerializeObject(currentAttendeesRequest);
            var requestContent = new StringContent(serialized, Encoding.UTF8, "application/json");

            await UseHttpClient(ExecuteHttpCall, HandleResponseMessage, unitsSyncData.BaseAddress, unitsSyncData.Secret);

            async Task<HttpResponseMessage> ExecuteHttpCall(HttpClient httpClient)
            {
                try
                {
                    return await httpClient.PostAsync(requestPath, requestContent);
                }
                catch (HttpRequestException e)
                {
                    var baseExceptionMessage = e.GetBaseException().Message;
                    _logger.LogRequestError(unitsSyncData.BaseAddress, requestPath, baseExceptionMessage);
                    return null;
                }
            }

            async Task HandleResponseMessage(HttpResponseMessage responseMessage)
            {
                if (responseMessage == null)
                    return;
                if (!responseMessage.IsSuccessStatusCode)
                    await _logger.LogRequestErrorAsync(unitsSyncData.BaseAddress, requestPath, responseMessage);
            }
        }
    }
}
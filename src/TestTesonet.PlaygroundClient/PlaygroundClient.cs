using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using TestTesonet.Clients.Models;

namespace TestTesonet.Clients
{
    public class PlaygroundClient : IPlaygroundClient
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly HttpClient _httpClient;
        private string _token;

        public PlaygroundClient(string baseAddress)
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(baseAddress) };
        }

        public bool LoggedIn => !string.IsNullOrWhiteSpace(_token);

        public async Task<bool> Authenticate(string username, string password)
        {
            _logger.Debug($"Authenticating for user: {username}");
            var response = await _httpClient.PostAsync("tokens", new StringContent(JsonConvert.SerializeObject(new { username, password }), Encoding.UTF8, "application/json"));
            
            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.Error($"Failed to authenticate. Response: {responseContent}");
                throw new Exception(responseContent);
            }

            var tokenResponse = await response.Content.ReadAsAsync<TokenResponse>();
            _token = tokenResponse.Token;

            return true;
        }

        public async Task<List<Server>> GetServers()
        {
            if (!LoggedIn)
            {
                throw new AuthenticationException("Client not authenticated.");
            }

            _logger.Debug("Getting servers list.");
            _httpClient.SetBearerToken(_token);

            var response = await _httpClient.GetAsync("servers");

            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.Error($"Failed to get servers list. Response: {responseContent}");
                throw new Exception(responseContent);
            }

            return await response.Content.ReadAsAsync<List<Server>>();
        }

        public void DropToken()
        {
            // this should be done more gracefully, so that if token would be sniffed by someone - it would be expired or something...
            _token = null;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}

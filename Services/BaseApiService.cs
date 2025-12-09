using System;
using System.Net.Http;
using Toyo_cable_UI.Constants;
using Toyo_cable_UI.Helpers;

namespace Toyo_cable_UI.Services
{
    public abstract class BaseApiService
    {
        protected readonly HttpClient HttpClient;
        private string? _authToken;

        protected BaseApiService()
        {
            HttpClient = CreateHttpClient();
            ConfigureHeaders();
        }

        private static HttpClient CreateHttpClient()
        {
            return new HttpClient
            {
                BaseAddress = new Uri(ApiEndpoints.BaseUrl)
            };
        }

        private void ConfigureHeaders()
        {
            HttpClient.DefaultRequestHeaders.Clear();
            HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            if (!string.IsNullOrEmpty(TokenManager.Token))
            {
                SetAuthToken(TokenManager.Token);
            }
        }

        public bool IsAuthenticated => TokenManager.IsAuthenticated;

        public void SetAuthToken(string token)
        {
            _authToken = token;
            HttpClient.DefaultRequestHeaders.Remove("Authorization");
            HttpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public void ClearAuth()
        {
            _authToken = null;
            HttpClient.DefaultRequestHeaders.Remove("Authorization");
        }
    }
}

/*
 public class OrderService : BaseApiService
    {
        public OrderService() : base()
        {
        }
        public async Task CreateOrder()
        {
            var response = await HttpClient.PostAsync();
        }
    }
 */
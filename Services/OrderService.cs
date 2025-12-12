using CloudinaryDotNet;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Toyo_cable_UI.Constants;
using Toyo_cable_UI.Helpers;
using Toyo_cable_UI.Models;
using Toyo_cable_UI.Models.DTOs.OrderDto;

namespace Toyo_cable_UI.Services
{
    public class OrderService
    {
        // define http client
        private readonly static HttpClient _httpClient = new HttpClient()
        {
            BaseAddress = new Uri(ApiEndpoints.BaseUrl)
        };
        private string? _authToken;

        public OrderService()
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            if (!string.IsNullOrEmpty(TokenManager.Token))
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenManager.Token);
            }
        }

        public bool IsAuthenticated => TokenManager.IsAuthenticated;

        public void SetAuthToken(string token)
        {
            _authToken = token;
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }

        public void ClearAuth()
        {
            _authToken = null;
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
        }

        // create an order
        public async Task<Order?> CreateOrderAsync(Order order)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.Orders, order);

                // Log the status code for debugging
                System.Diagnostics.Debug.WriteLine($"Create Order Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Order>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Create Order Error: {errorContent}");
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Create Order Exception: {ex.Message}");
                throw;
            }
        }

        // get Orders
        public async Task<List<Order>?> GetOrdersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(ApiEndpoints.Orders);
                System.Diagnostics.Debug.WriteLine($"Get Order Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<Order>>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Get Order Error: {errorContent}");
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Get Order Exception: {ex.Message}");
                throw;
            }

        }

        // Update Order
        public async Task<Order?> UpdateOrderAsync(Guid id, Order order)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(ApiEndpoints.OrderWithId(id), order);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Order>();
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Add this method to your existing OrderService class

        public async Task<Order?> GetOrderByIdAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync(ApiEndpoints.OrderWithId(id));

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var order = JsonSerializer.Deserialize<Order>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return order;
                }
                else
                {
                    // Log error or handle unsuccessful response
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Log exception
                System.Diagnostics.Debug.WriteLine($"Error fetching order details: {ex.Message}");
                return null;
            }
        }

    }
}

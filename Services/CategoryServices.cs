using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Toyo_cable_UI.Constants;
using Toyo_cable_UI.Helpers;
using Toyo_cable_UI.Models;

namespace Toyo_cable_UI.Services
{
    public class CategoryServices
    {
        private static readonly HttpClient _httpClient = new HttpClient()
        {
            BaseAddress = new Uri(ApiEndpoints.BaseUrl)
        };

        private static string? _authToken;

        public CategoryServices()
        {
            // Set default headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            // Add authorization header if token exists
            if (!string.IsNullOrEmpty(TokenManager.Token))
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", TokenManager.Token);
            }
        }

        // Check if user is authenticated
        public bool IsAuthenticated => TokenManager.IsAuthenticated;

        // Set authentication token
        public void SetAuthToken(string token)
        {
            _authToken = token;
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }

        // Clear authentication
        public void ClearAuth()
        {
            _authToken = null;
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
        }

        // Create & return the created category:
        public async Task<Category?> CreateCategoryAsync(Category category)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.Categories, category);

                // Log the status code for debugging
                System.Diagnostics.Debug.WriteLine($"Create Category Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Category>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Create Category Error: {errorContent}");
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Create Category Exception: {ex.Message}");
                throw;
            }
        }

        // get categories
        public async Task<List<Category>?> GetCategoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(ApiEndpoints.Categories);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<Category>>();
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
            
        }

        // Update category
        public async Task<Category?> UpdateCategoryAsync(Guid id, Category category)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(ApiEndpoints.UpdateCategory(id), category);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Category>();
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Delete category
        public async Task<bool> DeleteCategoryAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(ApiEndpoints.UpdateCategory(id));

                return response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
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
    public class ProductServices
    {
        // define http client
        private readonly static HttpClient _httpClient = new HttpClient()
        {
            BaseAddress = new Uri(ApiEndpoints.BaseUrl)
        };
        private string? _authToken;

        public ProductServices()
        {
            // set default headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            // if token is exist, add token to authorization headers
            if (!string.IsNullOrEmpty(TokenManager.Token))
            {
                _httpClient.DefaultRequestHeaders.Remove("Authorization");
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer" ,TokenManager.Token);
            }
        }

        // check if user is authenticated
        public bool IsAuthenticated => TokenManager.IsAuthenticated;

        // set authentication token
        public void setAuthToken(string token)
        {
            _authToken = token;
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization" ,$"Bearer, {token}");
        }

        // clear authentication
        public void ClearAuth()
        {
            _authToken = null;
            _httpClient.DefaultRequestHeaders.Remove("Authorization");
        }

        // create a product
        public async Task<Products?> CreateProductsAsync(Products? products)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.Products, products);

                // debug the response
                System.Diagnostics.Debug.WriteLine($"Product response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Products>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Create Product Error: {errorContent}");
                }

                return null;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Create Product Exception: {ex.Message}");
                throw;
            }
        }

        // get all products
        public async Task<List<Products>?> GetProductsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(ApiEndpoints.Products);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<Products>>();
                }
                return null;
                
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        // update product
        public async Task<Products?> UpdateProductAsync(Guid id ,Products? product)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(ApiEndpoints.ProductWithId(id), product);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Products>();
                }
                return null;
            }
            catch(Exception)
            {
                return null;
            }
        }

        // delete a product
        public async Task<bool> DeleteProductAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(ApiEndpoints.ProductWithId(id));

                return response.IsSuccessStatusCode;
            }
            catch(Exception)
            {
                return false;
            }
        }
    }
}

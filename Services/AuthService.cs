using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Toyo_cable_UI.Constants;
using Toyo_cable_UI.Models;
using Toyo_cable_UI.Models.DTOs;

namespace Toyo_cable_UI.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private string? _authToken;
        private User? _currentUser;

        public AuthService()
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri(ApiEndpoints.BaseUrl)
            };

            // set default headers
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        // property to check if user is authenticated
        public bool IsAuthenticated => !string.IsNullOrEmpty(_authToken);

        // get current logged-in user
        public User? CurrentUser => _currentUser;

        // Login Method
        public async Task<(bool success, string message, User? user)> LoginAsync(string Email, string Password)
        {
            try
            {
                // create login request
                var loginRequest = new LoginRequestDto
                {
                    Email = Email,
                    Password = Password
                };

                // send post request
                var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.Login, loginRequest);

                if (response.IsSuccessStatusCode)
                {
                    // read the response
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

                    if(loginResponse != null && !string.IsNullOrEmpty(loginResponse.JwtToken))
                    {
                        //store token
                        _authToken = loginResponse.JwtToken;

                        // create user object
                        _currentUser = new User
                        {
                            Email = Email,
                            JwtToken = loginResponse.JwtToken

                        };

                        // add token to all future requests
                        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);

                    }

                    return (true, "Login Successful", _currentUser);
                }
                else if(response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return (false, errorMessage.Contains("Invalid") ? "Invalid email or password" : errorMessage, null);
                }
                else if(response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    return (false, "Server error. Please try again later", null);
                }
                else
                {
                    return (false, $"Error: {response.StatusCode}", null);
                }
            }
            catch (HttpRequestException)
            {
                return (false, "Cannot connect to server. Please check your connection and ensure the API is running.", null);
            }
            catch(Exception ex)
            {
                return (false, $"An error occurred: {ex.Message}", null);
            }
        }
    }
}

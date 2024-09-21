using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace MezzexEyeApplication.Components.Pages
{
    public partial class Login : IDisposable
    {
        private string Username { get; set; }
        private string Password { get; set; }
        private string LoginMessage { get; set; } // Message for login success/failure
        private string LoginMessageCssClass { get; set; } // CSS class for alert styling
        private List<string> UsernamesCache = new();
        private List<string> suggestions = new();
        private string SystemName { get; set; }

        [Inject] IJSRuntime JS { get; set; } // Inject JavaScript interop for localStorage access
        [Inject] private HttpClient Http { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await PreloadUsernames();
            await LoadSystemNameFromLocalStorage();
        }

        private async Task PreloadUsernames()
        {
            try
            {
                UsernamesCache = await Http.GetFromJsonAsync<List<string>>("/api/AccountApi/getUsernames");
            }
            catch (Exception ex)
            {
                LoginMessage = $"Error loading usernames: {ex.Message}";
                LoginMessageCssClass = "alert-danger";
            }
        }

        private void OnInput(ChangeEventArgs e)
        {
            string input = e.Value.ToString();
            if (!string.IsNullOrEmpty(input))
            {
                suggestions = UsernamesCache
                    .Where(u => u.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                    .Take(5)
                    .ToList();
            }
            else
            {
                suggestions.Clear();
            }
        }

        private void SelectUsername(string selectedUsername)
        {
            Username = selectedUsername;
            suggestions.Clear();
        }

        private async Task HandleLogin()
        {
            // Get the computer's name (machine name)
            var machineName = Environment.MachineName;

            // Extract the part of the email before the "@" to use as part of the system name
            var userPart = Username.Contains("@") ? Username.Split('@')[0] : Username;

            // Combine the machine name and the extracted part of the username
            SystemName = $"{userPart}-{machineName}";

            // Save the system name locally using JavaScript interop (localStorage)
            await JS.InvokeVoidAsync("localStorage.setItem", "systemName", SystemName);

            // Prepare the login request data
            var data = new { Email = Username, Password, SystemName };

            try
            {
                var response = await Http.PostAsJsonAsync("/api/AccountApi/login", data);
                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

                if (response.IsSuccessStatusCode && result.ContainsKey("message") && result["message"].ToString() == "Login successful")
                {
                    LoginMessage = "Login successful!";
                    LoginMessageCssClass = "alert-success"; // Green alert for success

                }
                else
                {
                    LoginMessage = result.ContainsKey("message") ? result["message"].ToString() : "Login failed.";
                    LoginMessageCssClass = "alert-danger"; // Red alert for failure
                }
            }
            catch (Exception ex)
            {
                LoginMessage = $"Error during login: {ex.Message}";
                LoginMessageCssClass = "alert-danger"; // Red alert for error
            }
        }

        private async Task LoadSystemNameFromLocalStorage()
        {
            try
            {
                // Try to retrieve the system name from localStorage
                SystemName = await JS.InvokeAsync<string>("localStorage.getItem", "systemName");

                if (!string.IsNullOrEmpty(SystemName))
                {
                    LoginMessage = $"Welcome back! System Name: {SystemName}";
                    LoginMessageCssClass = "alert-info"; // Informational message
                }
            }
            catch (Exception ex)
            {
                LoginMessage = $"Error loading system name: {ex.Message}";
                LoginMessageCssClass = "alert-danger";
            }
        }

        private void ShutdownSystem()
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "shutdown",
                    Arguments = "/s /t 1",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
            }
            catch (Exception ex)
            {
                LoginMessage = $"Error: {ex.Message}";
                LoginMessageCssClass = "alert-danger";
            }
        }

        private void RestartSystem()
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "shutdown",
                    Arguments = "/r /t 1",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
            }
            catch (Exception ex)
            {
                LoginMessage = $"Error: {ex.Message}";
                LoginMessageCssClass = "alert-danger";
            }
        }

        public void Dispose()
        {
            // Clean up any resources here if needed.
            // This method is part of the IDisposable interface implementation.
        }
    }
}

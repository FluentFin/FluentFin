using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Media.Protection.PlayReady;
using Windows.Security.Credentials;
using Windows.Storage;
using Newtonsoft.Json;
using Microsoft.VisualBasic;

namespace FluentFin
{
    public class JellyfinLibrary
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string CollectionType { get; set; }
    }

    public class JellyfinClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _apiKey;

        public JellyfinClient(string baseUrl, string apiKey)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("X-MediaBrowser-Token", _apiKey);
        }

        public async Task<bool> IsServerAvailable(string serverUrl)
        {
            try
            {
                using HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, serverUrl));
                return response.IsSuccessStatusCode;
            }
            catch
            {
                // Handle the exception
                return false;
            }
        }

        public async Task<string?> AuthenticateUserAsync(string serverUrl, string username, string password)
        {
            using var client = new HttpClient();

            var authorizationHeader =
                "MediaBrowser Client=\"FluentFin\", " + //app name
                "Device=\"Laptop\", " +       // Use actual device name
                "DeviceId=\"12345\", " +      // Unique device identifier
                "Version=\"1.0.0\"";

            client.DefaultRequestHeaders.Add("X-Emby-Authorization", authorizationHeader);

            var requestBody = new
            {
                Username = username,
                Pw = password
            };

            // Serialize the object to JSON
            string jsonBody = System.Text.Json.JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            // Print the actual JSON body before sending
            Debug.WriteLine($"Request: {jsonBody}");

            var response = await client.PostAsync($"{serverUrl}/Users/AuthenticateByName", content);
            string responseText = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"Response: {response}");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonResponse);
                if (doc.RootElement.TryGetProperty("AccessToken", out var accessTokenElement) &&
                    accessTokenElement.ValueKind != JsonValueKind.Null &&
                    accessTokenElement.GetString() is string token)
                {
                    Debug.WriteLine($"Token value is: {token}");
                    ApplicationData.Current.LocalSettings.Values["serverUrl_Key"] = serverUrl;
                    var vault = new PasswordVault();
                    vault.Add(new PasswordCredential("FluentFin", "AccessToken", token));
                    Debug.WriteLine("Token has been saved");
                    return token;
                }
            }
            return null;
        }

        public string RetrieveString(string key)
        {
            var value = ApplicationData.Current.LocalSettings.Values[key] as string;
            return value ?? string.Empty;  // Returns an empty string if the key doesn't exist
        }

        public string? RetrieveToken()
        {
            var vault = new PasswordVault();
            var credentials = vault.RetrieveAll();

            if (credentials.Count > 0)
            {
                var cred = credentials[0];
                cred.RetrievePassword();
                return cred.Password;
            }

            return null;
        }

        public async Task<List<JellyfinLibrary>> GetLibrariesAsync()
        {
            string url = $"{_baseUrl}/Items?IncludeItemTypes=Library";

            var authorizationHeader =
                "MediaBrowser Client=\"FluentFin\", " + //app name
                "Device=\"Laptop\", " +       // Use actual device name
                "DeviceId=\"12345\", " +      // Unique device identifier
                "Version=\"1.0.0\"";

            _httpClient.DefaultRequestHeaders.Add("X-Emby-Authorization", authorizationHeader);

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string jsonResponse = await response.Content.ReadAsStringAsync();

                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                List<JellyfinLibrary> libraries = new();

                foreach (JsonElement item in doc.RootElement.GetProperty("Items").EnumerateArray())
                {
                    libraries.Add(new JellyfinLibrary
                    {
                        Name = item.GetProperty("Name").GetString(),
                        Id = item.GetProperty("Id").GetString(),
                        CollectionType = item.GetProperty("CollectionType").GetString()
                    });
                }

                return libraries;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching libraries: {ex.Message}");
                return new List<JellyfinLibrary>();
            }
        }

        public async Task<List<Movie>> GetMoviesInLibraryAsync(string libraryId)
        {
            // Construct the request URL with additional fields for year and poster
            string url = $"{_baseUrl}/Items?ParentId={libraryId}&IncludeItemTypes=Movie&Recursive=true&SortBy=SortName&SortOrder=Ascending&Fields=Id,Name,PremiereDate";

            using (HttpClient client = new HttpClient())
            {
                var authorizationHeader =
                "MediaBrowser Client=\"FluentFin\", " + //app name
                "Device=\"Laptop\", " +       // Use actual device name
                "DeviceId=\"12345\", " +      // Unique device identifier
                "Version=\"1.0.0\"";

                client.DefaultRequestHeaders.Add("X-Emby-Authorization", authorizationHeader);

                // Add authorization header
                client.DefaultRequestHeaders.Add("X-Emby-Token", _apiKey);

                Debug.WriteLine($"URL: {url}, token: {_apiKey}");

                // Send the GET request
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    // Parse the JSON response into a dynamic object
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<dynamic>(responseBody);

                    List<Movie> MoviesList = new List<Movie>();
                    // Iterate over items and print the movie details (including year and poster)
                    foreach (var item in data.Items)
                    {
                        string title = item.Name;
                        string id = item.Id;

                        // ✅ FIX: Safely parse PremiereDate
                        string year = "N/A";
                        if (item.PremiereDate != null)
                        {
                            string? premiereDateStr = item.PremiereDate.ToString(); // Convert to string
                            if (!string.IsNullOrEmpty(premiereDateStr) && premiereDateStr.Length >= 9)
                            {
                                year = premiereDateStr.Substring(6, 4); // Extract year
                            }
                        }

                        string posterUrl = "N/A";
                        posterUrl = $"{_baseUrl}/Items/{id}/Images/Primary";
                        Debug.WriteLine($"Movie Title: {title}, ID: {id}, Year: {year}, Poster: {posterUrl}");
                        MoviesList.Add(new Movie { Title = title, Year = year, PosterUrl = posterUrl });
                    }
                    return MoviesList;
                }
                else
                {
                    Debug.WriteLine("Error fetching movies: " + response.ReasonPhrase);
                    return null;
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Protocols;

namespace Nikse.SubtitleEdit.Controls.Interfaces
{
    public class TrueBarAPI
    {
        private readonly HttpClient _httpClient;

        public TrueBarAPI()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> IsApiServerReachableAsync()
        {
            try
            {
                var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, "https://prepisi-auth.true-bar.si"));
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false; // If an exception occurs, assume the server is unreachable
            }
        }

        public async Task<string> Login(string username, string password)
        {
            try
            {
                // Prepare the request data
                var requestData = new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "username", username },
                    { "password", password },
                    { "client_id", "truebar-client" }
                };

                var requestContent = new FormUrlEncodedContent(requestData);

                // Send the POST request
                HttpResponseMessage response = await _httpClient.PostAsync(
                    "https://prepisi-auth.true-bar.si/auth/realms/truebar/protocol/openid-connect/token",
                    requestContent
                );

                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP errors
                return $"{ex.Message}";
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return $"{ex.Message}";
            }
        }

        public async Task<string> GetProfileConfigurationAsync(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync("https://prepisi-api.true-bar.si/api/client/configuration");

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> UploadFileAsync(string accessToken, string filePath)
        {
            try
            {
                using (var formData = new MultipartFormDataContent())
                {
                    // Add the file to the form data
                    var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(filePath));
                    fileContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("multipart/form-data");
                    formData.Add(fileContent, "file", System.IO.Path.GetFileName(filePath));

                    // Add the access token to the request headers
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                    // Send the POST request
                    HttpResponseMessage response = await _httpClient.PostAsync("https://prepisi-api.true-bar.si/api/client/upload?async=true", formData);

                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP errors
                return $"{ex.Message}";
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                return $"{ex.Message}";
            }

        }

        public async Task<string> CheckSessionStatusAsync(string sessionId, string accessToken)
        {
            // _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            // var response = await _httpClient.GetAsync($"https://prepisi-api.true-bar.si/api/client/sessions/{sessionId}");
            // response.EnsureSuccessStatusCode();
            // return await response.Content.ReadAsStringAsync();
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                // Get request
                var response = await _httpClient.GetAsync($"https://prepisi-api.true-bar.si/api/client/sessions/{sessionId}");

                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                return $"{ex.Message}";
            }
            catch (Exception ex)
            {
                return $"{ex.Message}";
            }
        }


        public async Task<string> GetSessionTranscriptAsync(string sessionId, string accessToken)
        {
            // _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            // var response = await _httpClient.GetAsync($"https://prepisi-api.true-bar.si/api/client/sessions/{sessionId}/transcripts");
            // response.EnsureSuccessStatusCode();
            // return await response.Content.ReadAsStringAsync();
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                var response = await _httpClient.GetAsync($"https://prepisi-api.true-bar.si/api/client/sessions/{sessionId}/transcripts");

                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                return $"{ex.Message}";
            }
            catch (Exception ex)
            {
                return $"{ex.Message}";
            }
        }
    }
}
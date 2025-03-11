using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Protocols;

namespace Nikse.SubtitleEdit.Controls.Interfaces
{
    public class TrueBarSubtitlerAPI
    {
        private readonly HttpClient _httpClient;

        public TrueBarSubtitlerAPI()
        {
            _httpClient = new HttpClient();
        }


        public async Task<string> Login(string client_id, string client_secret)
        {
            try
            {
                // Prepare the request data
                var requestData = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", client_id },
                { "client_secret", client_secret }
            };

                var requestContent = new FormUrlEncodedContent(requestData);

                // Send the POST request
                HttpResponseMessage response = await _httpClient.PostAsync(
                    "https://staging-subtitler-auth.true-bar.si/realms/subtitler/protocol/openid-connect/token",
                    requestContent
                );

                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                // Handle HTTP errors
                return $"{{\"error\": \"{ex.Message}\"}}";
            }
            catch (Exception ex)
            {
                // Handle other errors
                return $"{{\"error\": \"{ex.Message}\"}}";
            }
        }

        public async Task<string> UploadFileAsync(string access_token, string filename)
        {
            using (var formData = new MultipartFormDataContent())
            {
                var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(filename));
                fileContent.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse("multipart/form-data");
                formData.Add(fileContent, "audio_file", System.IO.Path.GetFileName(filename));

                // Add access token to the request headers
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access_token);

                try
                {
                    HttpResponseMessage response = await _httpClient.PostAsync(
                        "https://staging-subtitler.true-bar.si/v1/captions/longrunningmake?source_language_id=sl-SI&do_voice_activity_detection=true&do_punctuation=true&do_denormalization=true&do_speaker_change_detection=false",
                        formData
                    );

                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadAsStringAsync();
                }
                catch (HttpRequestException ex)
                {
                    // Handle HTTP errors
                    return $"{{\"error\": \"{ex.Message}\"}}";
                }
                catch (Exception ex)
                {
                    // Handle other errors
                    return $"{{\"error\": \"{ex.Message}\"}}";
                }
            }
        }

        public async Task<string> CheckJobStatus(string access_token, string job_id) {
            try {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access_token);
                var response = await _httpClient.GetAsync($"https://staging-subtitler.true-bar.si/v1/operations/{job_id}");

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            } catch (HttpRequestException ex) {
                // Handle HTTP errors
                return $"{{\"error\": \"{ex.Message}\"}}";
            } catch (Exception ex) {
                // Handle other errors
                return $"{{\"error\": \"{ex.Message}\"}}";
            }
        }
    }   
}
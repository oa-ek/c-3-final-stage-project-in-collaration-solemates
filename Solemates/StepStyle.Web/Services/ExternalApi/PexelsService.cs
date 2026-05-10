using System.Text.Json;
using StepStyle.Web.Models.DTOs;
using Microsoft.Extensions.Configuration;

namespace StepStyle.Web.Services.ExternalApi
{
    public class PexelsService : IPexelsService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public PexelsService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<PexelsDto?> SearchPhotosAsync(string query, int perPage = 1)
        {
            var client = _httpClientFactory.CreateClient("PexelsClient");
            var baseUrl = _configuration["ExternalApis:Pexels:BaseUrl"];
            var apiKey = _configuration["ExternalApis:Pexels:ApiKey"];

            try
            {
                client.DefaultRequestHeaders.Add("Authorization", apiKey!);

                var requestUrl = $"{baseUrl}search?query={query}&per_page={perPage}";

                var response = await client.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<PexelsDto>(jsonString);
                    return result;
                }
            }
            catch (Exception ex)
            {
            }

            return null;
        }
    }
}
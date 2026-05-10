using System.Text.Json;
using StepStyle.Web.Models.DTOs;
using Microsoft.Extensions.Configuration;

namespace StepStyle.Web.Services.ExternalApi
{
    public class RestCountryService : ICountryService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public RestCountryService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<CountryDto?> GetCountryByNameAsync(string countryName)
        {
            var client = _httpClientFactory.CreateClient("CountryClient");
            var baseUrl = _configuration["ExternalApis:RestCountries:BaseUrl"];

            try
            {
                var response = await client.GetAsync($"{baseUrl}name/{countryName}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();

                    var countries = JsonSerializer.Deserialize<IEnumerable<CountryDto>>(jsonString);
                    return countries?.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
            }

            return null; 
        }
    }
}
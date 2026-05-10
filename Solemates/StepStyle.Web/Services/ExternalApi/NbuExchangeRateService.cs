using System.Text.Json;
using StepStyle.Web.Models.DTOs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory; 

namespace StepStyle.Web.Services.ExternalApi
{
    public class NbuExchangeRateService : IExchangeRateService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache; 

        public NbuExchangeRateService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IMemoryCache memoryCache)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _memoryCache = memoryCache;
        }

        public async Task<IEnumerable<ExchangeRateDto>> GetExchangeRatesAsync()
        {
            string cacheKey = "NbuRatesCache";

            if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<ExchangeRateDto>? cachedRates))
            {
                return cachedRates!;
            }

            var client = _httpClientFactory.CreateClient("NbuClient");
            var baseUrl = _configuration["ExternalApis:Nbu:BaseUrl"];

            try
            {
                var response = await client.GetAsync(baseUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var rates = JsonSerializer.Deserialize<IEnumerable<ExchangeRateDto>>(jsonString);

                    if (rates != null)
                    {
                        var cacheOptions = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(TimeSpan.FromHours(1));

                        _memoryCache.Set(cacheKey, rates, cacheOptions);

                        return rates;
                    }
                }
            }
            catch (Exception) 
            {
               
            }

            return new List<ExchangeRateDto>();
        }
    }
}
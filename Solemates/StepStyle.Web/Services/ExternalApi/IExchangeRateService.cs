using StepStyle.Web.Models.DTOs;

namespace StepStyle.Web.Services.ExternalApi
{
    public interface IExchangeRateService
    {
        Task<IEnumerable<ExchangeRateDto>> GetExchangeRatesAsync();
    }
}
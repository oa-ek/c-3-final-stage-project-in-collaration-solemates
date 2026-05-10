using StepStyle.Web.Models.DTOs;

namespace StepStyle.Web.Services.ExternalApi
{
    public interface ICountryService
    {
        Task<CountryDto?> GetCountryByNameAsync(string countryName);
    }
}
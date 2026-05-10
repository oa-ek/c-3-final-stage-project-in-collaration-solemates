using StepStyle.Web.Models.DTOs;

namespace StepStyle.Web.Services.ExternalApi
{
    public interface IPexelsService
    {
        Task<PexelsDto?> SearchPhotosAsync(string query, int perPage = 1);
    }
}
using System.Text.Json.Serialization;

namespace StepStyle.Web.Models.DTOs
{
    public class PexelsDto
    {
        [JsonPropertyName("total_results")]
        public int TotalResults { get; set; }

        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("photos")]
        public List<PexelsPhotoDto> Photos { get; set; } = new();
    }

    public class PexelsPhotoDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("photographer")]
        public string? Photographer { get; set; }

        [JsonPropertyName("alt")]
        public string? Alt { get; set; }

        [JsonPropertyName("src")]
        public PexelsSourceDto? Src { get; set; }
    }

    public class PexelsSourceDto
    {
        [JsonPropertyName("original")]
        public string? Original { get; set; }

        [JsonPropertyName("large")]
        public string? Large { get; set; }

        [JsonPropertyName("medium")]
        public string? Medium { get; set; }

        [JsonPropertyName("small")]
        public string? Small { get; set; }

        [JsonPropertyName("landscape")]
        public string? Landscape { get; set; }
    }
}
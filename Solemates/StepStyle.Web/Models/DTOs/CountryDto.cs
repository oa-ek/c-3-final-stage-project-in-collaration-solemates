using System.Text.Json.Serialization;

namespace StepStyle.Web.Models.DTOs
{
    public class CountryDto
    {
        [JsonPropertyName("name")]
        public CountryNameDto? Name { get; set; }

        [JsonPropertyName("cca2")]
        public string? CountryCode { get; set; }

        [JsonPropertyName("flags")]
        public CountryFlagsDto? Flags { get; set; }

        [JsonPropertyName("currencies")]
        public Dictionary<string, CurrencyInfoDto>? Currencies { get; set; }
    }

    public class CountryNameDto
    {
        [JsonPropertyName("common")]
        public string? Common { get; set; }

        [JsonPropertyName("official")]
        public string? Official { get; set; }
    }

    public class CountryFlagsDto
    {
        [JsonPropertyName("png")]
        public string? Png { get; set; }

        [JsonPropertyName("svg")]
        public string? Svg { get; set; }

        [JsonPropertyName("alt")]
        public string? Alt { get; set; }
    }

    public class CurrencyInfoDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("symbol")]
        public string? Symbol { get; set; }
    }
}
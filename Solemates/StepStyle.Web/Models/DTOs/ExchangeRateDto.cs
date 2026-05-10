using System.Text.Json.Serialization;

namespace StepStyle.Web.Models.DTOs
{
    public class ExchangeRateDto
    {
        [JsonPropertyName("r030")]
        public int CurrencyCode { get; set; }

        [JsonPropertyName("txt")]
        public string? CurrencyName { get; set; }

        [JsonPropertyName("rate")]
        public decimal Rate { get; set; }

        [JsonPropertyName("cc")]
        public string? Cc { get; set; } 

        [JsonPropertyName("exchangedate")]
        public string? ExchangeDate { get; set; }
    }
}
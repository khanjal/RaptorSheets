using System.Text.Json.Serialization;

namespace GigRaptorLib.Entities
{
    public class TripEntity : AmountEntity
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("key")]
        public string Key { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("service")]
        public string Service { get; set; }

        [JsonPropertyName("number")]
        public int? Number { get; set; }

        [JsonPropertyName("exclude")]
        public bool Exclude { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("place")]
        public string Place { get; set; }

        [JsonPropertyName("pickupTime")]
        public string Pickup { get; set; }

        [JsonPropertyName("dropoffTime")]
        public string Dropoff { get; set; }

        [JsonPropertyName("duration")]
        public string Duration { get; set; }

        [JsonPropertyName("startOdometer")]
        public decimal? OdometerStart { get; set; }

        [JsonPropertyName("endOdometer")]
        public decimal? OdometerEnd { get; set; }

        [JsonPropertyName("distance")]
        public decimal? Distance { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("startAddress")]
        public string StartAddress { get; set; }

        [JsonPropertyName("endAddress")]
        public string EndAddress { get; set; }

        [JsonPropertyName("endUnit")]
        public string EndUnit { get; set; }

        [JsonPropertyName("orderNumber")]
        public string OrderNumber { get; set; }

        [JsonPropertyName("region")]
        public string Region { get; set; }

        [JsonPropertyName("note")]
        public string Note { get; set; }

        [JsonPropertyName("amountPerTime")]
        public decimal? AmountPerTime { get; set; }

        [JsonPropertyName("amountPerDistance")]
        public decimal? AmountPerDistance { get; set; }

        [JsonPropertyName("saved")]
        public bool Saved { get; set; }
    }
}
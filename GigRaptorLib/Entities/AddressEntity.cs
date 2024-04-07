using System.Text.Json.Serialization;

namespace GigRaptorLib.Entities
{
    public class AddressEntity : AmountEntity
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("visits")]
        public int Visits { get; set; }

        [JsonPropertyName("distance")]
        public decimal Distance { get; set; }
    }
}
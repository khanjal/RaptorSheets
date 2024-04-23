using System.Text.Json.Serialization;

namespace GigRaptorLib.Entities
{
    public class SheetEntity
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("addresses")]
        public List<AddressEntity> Addresses { get; set; } = new List<AddressEntity>();

        [JsonPropertyName("daily")]
        public List<DailyEntity> Daily { get; set; } = new List<DailyEntity>();

        [JsonPropertyName("monthly")]
        public List<MonthlyEntity> Monthly { get; set; } = new List<MonthlyEntity>();

        [JsonPropertyName("names")]
        public List<NameEntity> Names { get; set; } = new List<NameEntity>();

        [JsonPropertyName("places")]
        public List<PlaceEntity> Places { get; set; } = new List<PlaceEntity>();

        [JsonPropertyName("regions")]
        public List<RegionEntity> Regions { get; set; } = new List<RegionEntity>();

        [JsonPropertyName("services")]
        public List<ServiceEntity> Services { get; set; } = new List<ServiceEntity>();

        [JsonPropertyName("shifts")]
        public List<ShiftEntity> Shifts { get; set; } = new List<ShiftEntity>();

        [JsonPropertyName("trips")]
        public List<TripEntity> Trips { get; set; } = new List<TripEntity>();

        [JsonPropertyName("types")]
        public List<TypeEntity> Types { get; set; } = new List<TypeEntity>();

        [JsonPropertyName("weekdays")]
        public List<WeekdayEntity> Weekdays { get; set; } = new List<WeekdayEntity>();

        [JsonPropertyName("weekly")]
        public List<WeeklyEntity> Weekly { get; set; } = new List<WeeklyEntity>();

        [JsonPropertyName("yearly")]
        public List<YearlyEntity> Yearly { get; set; } = new List<YearlyEntity>();

        [JsonPropertyName("messages")]
        public List<MessageEntity> Messages { get; set; } = new List<MessageEntity>();
    }
}
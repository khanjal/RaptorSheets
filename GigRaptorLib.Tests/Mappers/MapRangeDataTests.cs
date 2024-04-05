using FluentAssertions;
using GigRaptorLib.Mappers;
using GigRaptorLib.Models;
using Newtonsoft.Json;

namespace GigRaptorLib.Tests.Mappers
{
    public class MapRangeDataTests
    {
        public static IEnumerable<object[]> Sheets =>
        [
            ["Address"],
            ["Name"]
        ];

        private static IList<IList<object>>? LoadJson(string sheet)
        {
            using StreamReader reader = new($"./Data/Json/Sheets/{sheet}Sheet.json");
            var json = reader.ReadToEnd();
            var values = JsonConvert.DeserializeObject<GoogleResponse>(json)?.Values;

            return values;
        }

        [Fact]
        public void GivenAddressSheetData_ThenReturnRangeData()
        {
            var values = LoadJson("Address");
            values.Should().NotBeNull();

            var entities = AddressMapper.MapFromRangeData(values!);
            var nonEmptyValues = values!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();

            entities.Should().HaveCount(nonEmptyValues.Count-1);

            foreach (var entity in entities)
            {
                entity.Address.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public void GivenNameSheetData_ThenReturnRangeData()
        {
            var values = LoadJson("Name");
            values.Should().NotBeNull();

            var entities = NameMapper.MapFromRangeData(values!);
            var nonEmptyValues = values!.Where(x => !string.IsNullOrEmpty(x[0].ToString())).ToList();

            entities.Should().HaveCount(nonEmptyValues.Count - 1);

            foreach (var entity in entities)
            {
                entity.Name.Should().NotBeNullOrEmpty();
            }
        }
    }
}

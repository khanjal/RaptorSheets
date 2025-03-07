using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Tests.Data.Helpers;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Tests.Data.Entities;

namespace RaptorSheets.Gig.Tests.Data.Helpers;

internal class TestGigHelpers
{
    internal static SheetEntity? LoadSheetJson()
    {
        var path = JsonHelpers.GetDataJsonPath("ShiftWithTrips");
        var json = JsonHelpers.ReadJson(path);
        var sheetData = JsonHelpers.DeserializeJson<SheetEntity>(json);

        return sheetData;
    }
    internal static SheetEntity GenerateShift(ActionTypeEnum actionType, int shiftStartId = 2, int tripStartId = 2)
    {
        // Get JSON data
        var services = JsonHelpers.LoadJsonData<List<string>>("services");
        var service = services?.GetRandomItem();
        var places = JsonHelpers.LoadJsonData<List<PlaceJsonEntity>>("places")?.Where(x => x.Services.Contains(service!)).ToList() ?? new List<PlaceJsonEntity>();
        var names = JsonHelpers.LoadJsonData<List<NameJsonEntity>>("names");

        // Create shift/trips
        var date = DateTime.Now.ToString("yyyy-MM-dd");
        var random = new Random();
        var randomNumber = random.Next(1, 99);

        var sheetEntity = new SheetEntity();
        sheetEntity.Shifts.Add(new ShiftEntity { RowId = shiftStartId, Action = actionType.GetDescription(), Date = date, Number = randomNumber, Service = service!, Start = DateTime.Now.ToString("T"), Note = actionType.GetDescription() });

        // Add random amount of trips
        for (int i = tripStartId; i < random.Next(tripStartId + 1, tripStartId + 5); i++)
        {
            var place = places.GetRandomItem();
            var name = names!.GetRandomItem();

            var tripEntity = GenerateTrip();
            tripEntity.Action = actionType.GetDescription();
            tripEntity.RowId = i;
            tripEntity.Date = date;
            tripEntity.Number = randomNumber;
            tripEntity.Service = service!;
            tripEntity.Pickup = DateTime.Now.ToString("T");
            tripEntity.Dropoff = DateTime.Now.AddMinutes(10).ToString("T");
            tripEntity.Duration = "00:10:00.000";
            tripEntity.Place = place.Name;
            tripEntity.StartAddress = place.Addresses.GetRandomItem();
            tripEntity.Name = name.Name;
            tripEntity.EndAddress = name.Address;
            tripEntity.Note = actionType.GetDescription();
            sheetEntity.Trips.Add(tripEntity);
        }

        return sheetEntity;
    }

    private static TripEntity GenerateTrip()
    {
        var random = new Random();
        var pay = Math.Round(random.Next(1, 10) + new decimal(random.NextDouble()), 2);
        var distance = Math.Round(random.Next(0, 20) + new decimal(random.NextDouble()), 1);
        var tip = random.Next(1, 5);

        return new TripEntity { Type = "Pickup", Pay = pay, Tip = tip, Distance = distance };
    }
}

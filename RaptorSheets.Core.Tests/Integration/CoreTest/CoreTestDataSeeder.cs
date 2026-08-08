namespace RaptorSheets.Core.Tests.Integration.CoreTest;

/// <summary>
/// Generates realistic, randomized Items/Log data - shared by CoreCleanSlateFixture's own seed step
/// and by the tests that wipe a whole sheet and need to repopulate it afterward (see those tests' own
/// comments for why). Amounts are randomized, not a suspicious linear sequence - a human looking at
/// the live sheet should see something that looks like real data, not generated fixture output.
/// </summary>
internal static class CoreTestDataSeeder
{
    private static readonly string[] Categories = ["Hardware", "Electronics", "Office", "Kitchen", "Outdoor", "Tools"];

    private static readonly string[] LogDescriptions =
    [
        "Routine check", "Restocked", "Inspection", "Maintenance performed",
        "Inventory audit", "Delivery received", "Damage reported", "Returned to vendor"
    ];

    public static List<ItemEntity> GenerateItems(int count, int startRowId, Random random)
    {
        var items = new List<ItemEntity>(count);

        for (var i = 0; i < count; i++)
        {
            items.Add(new ItemEntity
            {
                RowId = startRowId + i,
                Name = $"Item{startRowId + i}",
                Category = Categories[random.Next(Categories.Length)],
                Amount = Math.Round((decimal)(random.NextDouble() * 495) + 5, 2),
                Active = random.Next(2) == 0
            });
        }

        return items;
    }

    public static List<LogEntity> GenerateLogEntries(int count, int startRowId, Random random)
    {
        var logs = new List<LogEntity>(count);
        var baseDate = DateTime.Today.AddDays(-count);

        for (var i = 0; i < count; i++)
        {
            logs.Add(new LogEntity
            {
                RowId = startRowId + i,
                Date = baseDate.AddDays(i).ToString("yyyy-MM-dd"),
                Description = LogDescriptions[random.Next(LogDescriptions.Length)]
            });
        }

        return logs;
    }
}

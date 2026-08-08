namespace RaptorSheets.Core.Tests.Integration.CoreTest;

/// <summary>
/// Generates realistic, randomized Items/Log data - shared by CoreCleanSlateFixture's own seed step
/// and by the tests that wipe a whole sheet and need to repopulate it afterward (see those tests' own
/// comments for why). Amounts are randomized, not a suspicious linear sequence, and Names are drawn
/// from a per-category pool of plausible product names, not "Item10"/"Item11"/... - a human looking
/// at the live sheet should see something that looks like real data, not generated fixture output.
///
/// Name pools deliberately avoid every exact scratch-row name individual tests write to RowId 2
/// (Widget, Gadget, Level, Wrench, Hammer, Bolt, Nut, Screw) - those tests look up rows by exact Name
/// match, and a coincidental seed collision could make a test find the wrong row.
/// </summary>
internal static class CoreTestDataSeeder
{
    private static readonly Dictionary<string, string[]> ItemsByCategory = new()
    {
        ["Hardware"] = ["Bracket", "Anchor", "Chain Link", "Padlock", "Hinge", "Latch", "Washer Pack", "Bolt Cutter"],
        ["Electronics"] = ["USB Cable", "Wireless Mouse", "HDMI Adapter", "Power Bank", "Bluetooth Speaker", "Charging Dock", "Extension Cord"],
        ["Office"] = ["Stapler", "Notebook", "Binder Clip", "Sticky Notes", "Desk Organizer", "Whiteboard Marker", "Envelope Pack"],
        ["Kitchen"] = ["Mixing Bowl", "Cutting Board", "Whisk", "Measuring Cup", "Spatula", "Colander", "Can Opener"],
        ["Outdoor"] = ["Garden Hose", "Rake", "Lawn Chair", "Sprinkler Head", "Trowel", "Watering Can", "Patio Umbrella"],
        ["Tools"] = ["Drill", "Pliers", "Tape Measure", "Utility Knife", "Screwdriver Set", "Level Kit", "Socket Set"],
    };

    private static readonly string[] LogDescriptions =
    [
        "Routine check", "Restocked", "Inspection", "Maintenance performed",
        "Inventory audit", "Delivery received", "Damage reported", "Returned to vendor"
    ];

    public static List<ItemEntity> GenerateItems(int count, int startRowId, Random random)
    {
        var categories = ItemsByCategory.Keys.ToArray();
        var items = new List<ItemEntity>(count);

        for (var i = 0; i < count; i++)
        {
            var category = categories[random.Next(categories.Length)];
            var names = ItemsByCategory[category];

            items.Add(new ItemEntity
            {
                RowId = startRowId + i,
                Name = names[random.Next(names.Length)],
                Category = category,
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

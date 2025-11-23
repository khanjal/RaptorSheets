using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Constants;

[ExcludeFromCodeCoverage]
public static class ColumnNotes
{
    public const string ActiveTime = "Time with a delivery.\n\nOverrides the total active time calculated from the Trips sheet if filled out.";
    public const string DateFormat = "Format: YYYY-MM-DD";
    public const string Duration = "Hours/Minutes the trip (request) took to complete.";
    public const string Exclude = "Exclude this trip from being included in the shift.";
    public const string Pickup = "Time when the trip (request) was picked up.";
    public const string Place = "Location of pickup (delivery).";
    public const string ShiftDistance = "Distance not accounted for on the Trips sheet.";
    public const string ShiftKey = "Used to connect Shifts to the Trips sheet.";
    public const string ShiftNumber = "Shift Number 1-9\n\nLeave blank if there is only one shift for that service for that day.";
    public const string ShiftTrips = "Trips (Requests/Deliveries)\n\nUse this column if you don't track requests or need to increase the number.";
    public const string TimeOmit = "Omit time from non-service-specific totals. Useful for multi-app scenarios to get a more accurate $/hour calculation.\n\nActive time is still counted for the day from omitted shifts.\n\nExample: Omit Uber if it runs concurrently with DoorDash.";
    public const string TotalDistance = "Total Miles/Kilometers from Trips and Shifts.";
    public const string TotalTime = "Total time.";
    public const string TotalTimeActive = "Total active time from the Trips sheet (sum of durations).\n\nIf ActiveTime is entered on the Shifts sheet, it overrides this value.";
    public const string TotalTrips = "Number of trips (requests) during a shift.";
    public const string TripDistance = "How many miles/km the trip (request) took.";
    public const string TripKey = "Used to connect Trips to the Shifts sheet.";
    public const string Types = "Pickup, Shop, Order, Curbside, Canceled.";
    public const string UnitTypes = "Apartment, Unit, Room, Suite.";
}

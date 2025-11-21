using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Constants;

[ExcludeFromCodeCoverage]
public static class ColumnNotes
{
    public const string ActiveTime = "Time with a delivery.\n\nCan be filled out on requests sheet if you have that info.";
    public const string DateFormat = "Format: YYYY-MM-DD";
    public const string Duration = "Hours/Minutes task took to complete.";
    public const string Exclude = "Exclude this trip from being included in shift.";
    public const string Pickup = "Time when request/ride picked up.";
    public const string Place = "Location of pickup (delivery).";
    public const string ShiftDistance = "Distance not accounted for on the Requests/Trips sheet.";
    public const string ShiftKey = "Used to connect Shifts to Requests/Trips sheet.";
    public const string ShiftNumber = "Shift Number 1-9\n\nLeave blank if there is only one shift for that service for that day.";
    public const string ShiftTrips = "Requests/Deliveries/Trips\n\nUse this column if you don't track requests or need to increase the number.";
    public const string TimeOmit = "Omit time from non service specific totals. Mainly useful if you multi app so you can get a more accurate $/hour calculation.\n\nActive time is still counted for the day from omitted shifts.\n\nIE: Omit Uber if you have it also running during DoorDash.";
    public const string TotalDistance = "Total Miles/Kilometers from Requests and Shifts";
    public const string TotalTime = "Total time";
    public const string TotalTimeActive = "Total Active time from Requests and Shifts sheets.";
    public const string TotalTrips = "Number of requests during a shift.";
    public const string TripDistance = "How many miles/km the request/trip took.";
    public const string TripKey = "Used to connect Trips/Requests to Shifts sheet.";
    public const string Types = "Pickup, Shop, Order, Curbside, Canceled";
    public const string UnitTypes = "Apartment, Unit, Room, Suite";
}

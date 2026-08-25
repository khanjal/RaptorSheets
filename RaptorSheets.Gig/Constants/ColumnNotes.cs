using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Constants;

[ExcludeFromCodeCoverage]
public static class ColumnNotes
{
    public const string ActiveTime = "Time with a delivery.\n\nOverrides the total active time calculated from the Trips sheet if filled out.";
    public const string AddressSource = "Street address for this end of the trip.\n\nChoose an existing address, or type a new one here to add it to the Addresses sheet.";
    public const string Bonus = "Platform incentive on top of the base pay - DoorDash Peak Pay, an Uber Quest for completing so many deliveries, and the like.\n\nNot a customer tip. Those go under Tips, or Cash if they were handed to you.";
    public const string Cash = "Tips you were handed in cash rather than through the app.\n\nCash paid as the fare or order total is not a tip - put that under Pay.";
    public const string CategorySelf = "What kind of expense this is - gas, maintenance, supplies, and so on.\n\nChoose an existing category, or type a new one here to add it as a future option.";
    public const string DateFormat = "Format: YYYY-MM-DD";
    public const string Dropoff = "Time when the trip (request) was dropped off.";
    public const string Duration = "Hours/Minutes the trip (request) took to complete.";
    public const string Exclude = "Exclude this trip from being included in the shift.";
    public const string ExpenseDescription = "Any extra detail worth keeping - what it was for, or anything you would want when reviewing later.";
    public const string ExpenseName = "Short label for this specific expense - the station, shop, or item.\n\nCategory covers what kind of expense it is, so this is for telling two of the same kind apart.";
    public const string FreeformNote = "Anything worth remembering about this row that no other column covers.";
    public const string Odometer = "Optional. Fill in the start and end readings and the trip distance can be worked out from them.\n\nNeither is required - record odometer readings or a distance, whichever you actually track.";
    public const string OrderNumber = "Order/confirmation number from the delivery service's app, if it provides one.";
    public const string Pickup = "Time when the trip (request) was picked up.";
    public const string Place = "Location of pickup (delivery).\n\nOn a return this is the destination instead, so set Type to match.";
    public const string RegionSource = "City, area, or zone you are working in.\n\nChoose an existing region, or type a new one here to add it to the Regions sheet.";
    public const string ServiceSource = "Delivery or rideshare app you worked through - DoorDash, Uber Eats, Instacart, and so on.\n\nChoose an existing service, or type a new one here to add it to the Services sheet.";
    public const string ShiftDistance = "Distance not accounted for on the Trips sheet.\n\nOptional if you record odometer readings instead.";
    public const string ShiftKey = "Used to connect Shifts to the Trips sheet.";
    public const string ShiftNumber = "Shift Number 1-9\n\nLeave blank if there is only one shift for that service for that day.";
    public const string ShiftTrips = "Trips (Requests/Deliveries)\n\nUse this column if you don't track requests or need to increase the number.";
    public const string Tags = "Your own labels for this row, separated by commas.\n\nFree text - there is no dropdown, so whatever you type is kept as-is.\n\nExample: rain, surge, airport";
    public const string TimeEnd = "Time you ended the shift.";
    public const string TimeOmit = "Omit time from non-service-specific totals. Useful for multi-app scenarios to get a more accurate $/hour calculation.\n\nActive time is still counted for the day from omitted shifts.\n\nExample: Omit Uber if it runs concurrently with DoorDash.";
    public const string TimeStart = "Time you started the shift.";
    public const string TotalDistance = "Total Miles/Kilometers from Trips and Shifts.";
    public const string TotalTime = "Total time.";
    public const string TotalTimeActive = "Total active time from the Trips sheet (sum of durations).\n\nIf ActiveTime is entered on the Shifts sheet, it overrides this value.";
    public const string TotalTrips = "Number of trips (requests) during a shift.";
    public const string TripDistance = "How many miles/km the trip (request) took.\n\nOptional if you record odometer readings instead.";
    public const string TripKey = "Used to connect Trips to the Shifts sheet.";
    public const string TripNameSource = "The customer - usually first name and last initial, as the app shows it.\n\nOn a return the direction flips and this is who the pickup is from, so set Type to match.\n\nChoose an existing name, or type a new one here to add it to the Names sheet.";
    public const string Types = "Pickup, Shop, Order, Curbside, Return, Canceled.";
    public const string UnitTypes = "Apartment, Unit, Room, Suite.";
}

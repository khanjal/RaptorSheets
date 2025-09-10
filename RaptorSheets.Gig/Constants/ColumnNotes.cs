﻿using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Gig.Constants;

[ExcludeFromCodeCoverage]
public static class ColumnNotes
{
    public static string ActiveTime => $"Time with a delivery.{(char)10}{(char)10}Can be filled out on requests sheet if you have that info.";
    public static string DateFormat => "Format: YYYY-MM-DD";
    public static string Duration => "Hours/Minutes task took to complete.";
    public static string Exclude => "Exclude this trip from being included in shift.";
    public static string Pickup => "Time when request/ride picked up.";
    public static string Place => "Location of pickup (delivery).";
    public static string ShiftDistance => "Distance not accounted for on the Requests/Trips sheet.";
    public static string ShiftKey => "Used to connect Shifts to Requests/Trips sheet.";
    public static string ShiftNumber => $"Shift Number 1-9{(char)10}{(char)10}Leave blank if there is only one shift for that service for that day.";
    public static string ShiftTrips => $"Requests/Deliveries/Trips{(char)10}{(char)10}Use this column if you don't track requests or need to increase the number.";
    public static string TimeOmit => $"Omit time from non service specific totals. Mainly useful if you multi app so you can get a more accurate $/hour calculation.{(char)10}{(char)10}Active time is still counted for the day from omitted shifts.{(char)10}{(char)10}IE: Omit Uber if you have it also running during DoorDash.";
    public static string TotalDistance => "Total Miles/Kilometers from Requests and Shifts";
    public static string TotalTime => "Total time";
    public static string TotalTimeActive => "Total Active time from Requests and Shifts sheets.";
    public static string TotalTrips => "Number of requests during a shift.";
    public static string TripDistance => "How many miles/km the request/trip took.";
    public static string TripKey => "Used to connect Trips/Requests to Shifts sheet.";
    public static string Types => "Pickup, Shop, Order, Curbside, Canceled";
    public static string UnitTypes => "Apartment, Unit, Room, Suite";
}

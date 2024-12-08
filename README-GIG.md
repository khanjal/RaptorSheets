# RaptorSheets.Gig

| Badge Name | Status |
| ---------- | :------------: |
| Latest Build Status | [![build status](https://github.com/khanjal/RaptorSheets/actions/workflows/dotnet.yml/badge.svg)](https://github.com/khanjal/RaptorSheets/actions) |
| RaptorSheets.Gig Nuget Version | [![Nuget](https://img.shields.io/nuget/v/RaptorSheets.Gig)](https://www.nuget.org/packages/RaptorSheets.Gig/) |


# Project Description

This project is a library designed to manage interactions between a custom API service and the Google Sheets API. It offers the following features:

* Appending data to specified sheets
* Creating, formatting, and styling sheets
* Retrieving data from all sheets (individually, in groups, or in batches)
* Retrieving spreadsheet properties, such as the title and sheet tab names

# Sheets

Below is a list of sheets that are automatically generated with ````CreateSheets()```` function.

| # | Sheet Name | Description | Reference Sheets |
| :------------: | ---------- | ------------ | :------------: |
| 1 | Trips | This is a list of the trips you have accepted | None |
| 2 | Shifts | List of shifts you have worked | Trips |
| 3 | Addresses | List of addresses compiled from pickup and delivery locations | Shifts |
| 4 | Names | Names compiled from the trips you have accepted | Trips |
| 5 | Places | Locations compiled from the trips you have accepted | Trips |
| 6 | Regions | Regions compiled from your trips and shifts | Trips, Shifts |
| 7 | Services | Services compiled from your trips and shifts | Trips, Shifts |
| 8 | Types | Types compiled from the trips you have accepted | Trips |
| 9 | Daily | Daily statistics compiled from your shifts | Shifts |
| 10 | Weekdays | Weekday statistics compiled from daily data | Daily |
| 11 | Weekly | Weekly statistics compiled from daily data | Daily |
| 12 | Monthly | Monthly statistics compiled from daily data | Daily |
| 13 | Yearly | Yearly statistics compiled from monthly data | Monthly |

# Using Library

## Auth Modes

To authenticate, you can use one of the following methods:
* [AccessToken](https://cloud.google.com/dotnet/docs/reference/Google.Apis/latest/Google.Apis.Auth.OAuth2.BearerToken)
* [JsonCredentialParameters](https://cloud.google.com/dotnet/docs/reference/Google.Apis/latest/Google.Apis.Auth.OAuth2.JsonCredentialParameters)

## Simple

Using the ````GoogleSheetManager```` allows you to skip referencing ````Google.Apis.Sheets.v4```` package and just call the functions and receive data with common objects.

Create a new instance of the ````GoogleSheetManager```` with auth mode and spreadsheet id

```csharp
var googleSheetManager = new GoogleSheetManager(authMode, spreadsheetId);
```

You can create all sheets, formats, and layouts in a new worksheet by calling ````CreateSheets()````

```csharp
await googleSheetManager.CreateSheets();
```

You can get all sheets and information by calling ````GetSheets()````

```csharp
var data = await googleSheetManager.GetSheets();
```

You can retrieve specific sheets and information by calling ````GetSheets()```` and passing in the sheet enums you want.

```csharp
var sheets = [SheetEnum.Trips, SheetEnum.Shifts]
var data = await googleSheetManager.GetSheets(sheets);
```

## Advanced

Using the ````GoogleSheetService```` allows you to change format, colors, and other options by referencing the ````Google.Apis.Sheets.v4```` package.

# Testing

[Global Testing Setup](README.md#testing)

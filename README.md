# RaptorSheets

| Badge Name | Status | Site |
| ---------- | :------------: | :------------: |
| Latest Build Status | [![build status](https://github.com/khanjal/RaptorSheets/actions/workflows/dotnet.yml/badge.svg)](https://github.com/khanjal/RaptorSheets/actions) | [GitHub Repo](https://github.com/khanjal/RaptorSheets/) |
| RaptorSheets.Gig Nuget Pkg | [![Nuget](https://img.shields.io/nuget/v/RaptorSheets.Gig)](https://www.nuget.org/packages/RaptorSheets.Gig/) | [Raptor Sheets - Gig](https://gig.raptorsheets.com) |


# Project Description

This project is a library designed to manage interactions between a custom API service and the Google Sheets API. It offers the following features:

## Header Management
* Provides extension methods for handling column and index assignments, either individually or in batches, with automatic processing
* Read data in any order by using column names as keys for easy access

## Column Formatting
* Apply data formatting, configure drop-downs, and set protection

## Sheet Styling
* Configure alternating row colors and enable full sheet protection
* Assign names and colors to sheet tabs

## Other Features
* Appending data to specified sheets
* Retrieving data from all sheets (individually, in groups, or in batches)
* Retrieving spreadsheet properties, such as the title and sheet tab names

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
var sheets = [SheetEnum.Sheet1, SheetEnum.Sheet2]
var data = await googleSheetManager.GetSheets(sheets);
```

## Advanced

Using the ````GoogleSheetService```` allows you to change format, colors, and other options by referencing the ````Google.Apis.Sheets.v4```` package.

# Testing

## Create Service Account

You'll need to create your own service account and use the credentials to access your Google Sheet:

* [Google Cloud Console](https://console.cloud.google.com/)
* [Create New Project](https://console.cloud.google.com/projectcreate) or use existing one
* [Visit API Library](https://console.cloud.google.com/apis/library) and enable [Google Sheets API](https://console.cloud.google.com/apis/library/sheets.googleapis.com)
* [APIs & Services](https://console.cloud.google.com/apis/) -> [Create Credentials](https://console.cloud.google.com/apis/api/sheets.googleapis.com/credentials) -> Service Accounts
* Give the service account a ````name```` and ````id````
* Once created select the service account and go to the ````Keys```` tab
* Add/Create a new key and select type ````JSON````
* The key will download to your computer where you will have access to the values needed for the properties below.

## Local Setup

Add ````Google JSON Credentials```` by right clicking on ````RaptorSheets.Test```` and selecting ````Manage User Secrets```` (secrets.json)

Add the following JSON properties to it:

```json
{
  "google_credentials": {
    "type": "service_account",
    "private_key_id": "",
    "private_key": "",
    "client_email": "",
    "client_id": "",
  },
  "spreadsheets": {
    "gig": "",
    "stock": ""
  }
}
```

Create new spreadsheets and add the service account/client email to it.

Update the user secrets with the spreadsheeet ids.

Once that is completed you'll be able to run all tests including integration tests.

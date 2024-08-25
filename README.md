# GigRaptorLib

## Badge Statuses

| Latest Build Status | ![build status](https://github.com/khanjal/GigRaptorLibrary/actions/workflows/dotnet.yml/badge.svg) |
| ---------- | :------------: |

# Project Description

This project is a library used to handle the interactions between a custom API service and Google Sheets API. It offers the following features:

* Appending Data to Trips and Shifts sheets
* Creating, formatting, and styling all sheets in the worksheet.
* Getting data from all the sheets in the worksheet (individually, group, and batch)
* Getting worksheet properties like Title

# Using Library

## Auth Modes

To authenticate you can use either of the following:
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

Add ````Google JSON Credentials```` by right clicking on ````GigRaptorLib.tests```` and selecting ````Manage User Secrets```` (secrets.json)

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
  "spreadsheet_id": ""
}
```

Create a new spreadsheet and add the service account/client email to it.

Update the user secrets with the spreadsheeet id.

Once that is completed you'll be able to run all tests including integration tests.

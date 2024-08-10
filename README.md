# Project Description

This project is a library used to handle the interactions between a custom API service and Google Sheets API. It offers the following features:

* Appending Data to Trips and Shifts sheets
* Creating, formatting, and styling all sheets in the worksheet.
* Getting data from all the sheets in the worksheet (individually, group, and batch)
* Getting worksheet properties like Title

# Using Library

## Auth Modes

To authenticate you can use either an **AccessToken** or **JsonCredentialParameters**

## Simple

Create a new instance of the **GoogleSheetManager**

```csharp
var googleSheetManager = new GoogleSheetManager(authMode);
```

You can create all sheets, formats, and layouts in a new worksheet by calling **CreateSheets()**

```csharp
await googleSheetManager.CreateSheets(googleSheetId);
```

## Advanced

# Testing

Add google credentials by right clicking on **GigRaptorLib.tests** and selecting **Manage User Secrets** (secrets.json)

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

You'll need to create your own service account under **Google Cloud console -> APIs & Services -> Credentials -> Service Accounts**

Add the service account email to your spreadsheet so it can read from it. (May need generated before testing)

Once that is completed you'll be able to run all tests including integration tests.

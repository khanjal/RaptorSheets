using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Job.Constants;
using RaptorSheets.Job.Entities;
using RaptorSheets.Job.Helpers;

namespace RaptorSheets.Job.Managers;

/// <summary>
/// CRUD operations for Google Sheets.
/// Handles Create, Read, Update, and Delete operations.
/// </summary>
public partial class GoogleSheetManager
{
    #region Create Operations

    public async Task<SheetEntity> CreateAllSheets()
    {
        return await CreateSheets(SheetsConfig.SheetUtilities.GetAllSheetNames());
    }

    public async Task<SheetEntity> CreateSheets(List<string> sheets)
    {
        var sheetEntity = new SheetEntity();
        var batchUpdateSpreadsheetRequest = GenerateSheetsHelpers.Generate(sheets);

        try
        {
            var spreadsheetInfo = await _googleSheetService.GetSheetInfo();
            var defaultSheet = spreadsheetInfo?.Sheets?.FirstOrDefault(s =>
                string.Equals(s.Properties.Title, "Sheet1", StringComparison.OrdinalIgnoreCase));

            if (defaultSheet != null && defaultSheet.Properties.SheetId.HasValue)
            {
                var existingCount = spreadsheetInfo!.Sheets!.Count;
                var targetIndex = GoogleRequestHelpers.ComputeEndIndex(existingCount, sheets.Count);
                batchUpdateSpreadsheetRequest.Requests.Add(
                    GoogleRequestHelpers.GenerateUpdateSheetIndex(defaultSheet.Properties.SheetId.Value, targetIndex)
                );
            }
        }
        catch
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage(
                "Could not move default sheet to end; proceeding with creation",
                MessageTypeEnum.CREATE_SHEET));
        }

        // Attempt the batch update with a small retry loop to handle transient API errors
        BatchUpdateSpreadsheetResponse? response = null;
        const int maxAttempts = 3;
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                response = await _googleSheetService.BatchUpdateSpreadsheet(batchUpdateSpreadsheetRequest);
                if (response != null)
                    break;
            }
            catch (Exception ex)
            {
                // Bubble up a warning message but keep retrying
                sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage($"Batch update attempt {attempt} failed: {ex.Message}", MessageTypeEnum.CREATE_SHEET));
                // Also write full exception to console for diagnostics
                Console.WriteLine($"Batch update attempt {attempt} exception: {ex}");
            }

            // Backoff between attempts
            await Task.Delay(1000 * attempt);
        }

        if (response == null)
        {
            // Record explicit errors per-sheet for visibility
            foreach (var sheet in sheets)
            {
                sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage($"{sheet} not created", MessageTypeEnum.CREATE_SHEET));
            }

            // Try to give additional diagnostic information: what sheets actually exist now
            try
            {
                var properties = await _googleSheetService.GetSheetInfo(null);
                var created = properties?.Sheets?.Select(s => s.Properties.Title).ToList() ?? new List<string>();
                sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"Existing sheets after failure: {string.Join(", ", created)}", MessageTypeEnum.CREATE_SHEET));
            }
            catch
            {
                // Ignore diagnostics failure
            }

            // Log intended requests for debugging
            try
            {
                var intended = batchUpdateSpreadsheetRequest.Requests?
                    .Where(r => r.AddSheet != null)
                    .Select(r => r.AddSheet.Properties?.Title)
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList() ?? new List<string>();

                Console.WriteLine($"Intended AddSheet titles ({intended.Count}): {string.Join(", ", intended)}");
                sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"Intended AddSheet titles: {string.Join(", ", intended)}", MessageTypeEnum.CREATE_SHEET));
            }
            catch
            {
                // Best effort only
            }

            return sheetEntity;
        }

        var sheetTitles = response.Replies.Where(x => x.AddSheet != null).Select(x => x.AddSheet.Properties.Title).ToList();

        foreach (var sheetTitle in sheetTitles)
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateWarningMessage($"{sheetTitle.ToUpperInvariant()} created", MessageTypeEnum.CREATE_SHEET));
        }

        return sheetEntity;
    }

    #endregion

    #region Read Operations

    public async Task<SheetEntity> GetSheet(string sheet)
    {
        var sheetExists = GenerateSheetsHelpers.GetSheetNames()
            .Any(name => string.Equals(name, sheet, StringComparison.OrdinalIgnoreCase));

        if (!sheetExists)
        {
            return new SheetEntity { Messages = [MessageHelpers.CreateErrorMessage($"Sheet {sheet.ToUpperInvariant()} does not exist", MessageTypeEnum.GET_SHEETS)] };
        }

        return await GetSheets([sheet]);
    }

    public async Task<SheetEntity> GetAllSheets()
    {
        var sheets = GenerateSheetsHelpers.GetSheetNames();
        var response = await GetSheets(sheets);
        return response ?? new SheetEntity();
    }

    public async Task<SheetEntity> GetSheets(List<string> sheets)
    {
        var data = new SheetEntity();
        var messages = new List<MessageEntity>();
        var stringSheetList = string.Join(", ", sheets);

        var response = await _googleSheetService.GetBatchData(sheets);
        Spreadsheet? spreadsheetInfo;

        if (response == null)
        {
            spreadsheetInfo = await _googleSheetService.GetSheetInfo();
            messages.Add(MessageHelpers.CreateWarningMessage("No data returned from sheets", MessageTypeEnum.GET_SHEETS));
        }
        else
        {
            messages.Add(MessageHelpers.CreateInfoMessage($"Retrieved sheet(s): {stringSheetList}", MessageTypeEnum.GET_SHEETS));

            var ranges = sheets.Select(sheet => $"{sheet}!{GoogleConfig.HeaderRange}").ToList();
            spreadsheetInfo = await _googleSheetService.GetSheetInfo(ranges);

            data = JobSheetHelpers.MapData(response) ?? new SheetEntity();
        }

        if (spreadsheetInfo != null)
        {
            data.Properties.Name = spreadsheetInfo.Properties.Title;
        }

        data.Messages.AddRange(messages);

        return data;
    }

    public async Task<Spreadsheet?> GetSpreadsheetInfo(List<string>? ranges = null)
    {
        return await _googleSheetService.GetSheetInfo(ranges);
    }

    public async Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets)
    {
        return await _googleSheetService.GetBatchData(sheets);
    }

    #endregion

    #region Delete Operations

    public async Task<SheetEntity> DeleteAllSheets()
    {
        return await DeleteSheets(GenerateSheetsHelpers.GetSheetNames());
    }

    public async Task<SheetEntity> DeleteSheets(List<string> sheets)
    {
        var sheetEntity = new SheetEntity();
        var spreadsheetInfo = await _googleSheetService.GetSheetInfo();

        if (spreadsheetInfo == null)
        {
            sheetEntity.Messages.Add(MessageHelpers.CreateErrorMessage("Unable to retrieve spreadsheet info", MessageTypeEnum.DELETE_SHEET));
            return sheetEntity;
        }

        var batchUpdateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest { Requests = new List<Request>() };

        foreach (var sheet in sheets)
        {
            var existingSheet = spreadsheetInfo.Sheets?.FirstOrDefault(s =>
                string.Equals(s.Properties.Title, sheet, StringComparison.OrdinalIgnoreCase));

            if (existingSheet != null && existingSheet.Properties.SheetId.HasValue)
            {
                batchUpdateSpreadsheetRequest.Requests.Add(new Request
                {
                    DeleteSheet = new DeleteSheetRequest { SheetId = existingSheet.Properties.SheetId.Value }
                });
            }
        }

        if (batchUpdateSpreadsheetRequest.Requests.Count > 0)
        {
            await _googleSheetService.BatchUpdateSpreadsheet(batchUpdateSpreadsheetRequest);
            sheetEntity.Messages.Add(MessageHelpers.CreateInfoMessage($"Deleted {batchUpdateSpreadsheetRequest.Requests.Count} sheet(s)", MessageTypeEnum.DELETE_SHEET));
        }

        return sheetEntity;
    }

    #endregion

    #region Update Operations

    public async Task<SheetEntity> ChangeSheetData(List<string> sheets, SheetEntity sheetEntity)
    {
        // Basic implementation - to be enhanced
        return new SheetEntity
        {
            Messages = [MessageHelpers.CreateWarningMessage("ChangeSheetData not yet implemented", MessageTypeEnum.CREATE_SHEET)]
        };
    }

    #endregion
}

using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RaptorSheets.Core.Constants;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models;
using RaptorSheets.Core.Wrappers;
using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Core.Services;

public interface IGoogleSheetService
{
    public Task<AppendValuesResponse?> AppendData(ValueRange valueRange, string range, CancellationToken cancellationToken = default);
    public Task<BatchUpdateValuesResponse?> BatchUpdateData(BatchUpdateValuesRequest batchUpdateValuesRequest, CancellationToken cancellationToken = default);
    public Task<BatchUpdateSpreadsheetResponse?> BatchUpdateSpreadsheet(BatchUpdateSpreadsheetRequest batchUpdateSpreadsheetRequest, CancellationToken cancellationToken = default);
    public Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets, CancellationToken cancellationToken = default);
    public Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets, string? range, CancellationToken cancellationToken = default);
    public Task<ValueRange?> GetSheetData(string sheet, CancellationToken cancellationToken = default);
    public Task<Spreadsheet?> GetSheetInfo(CancellationToken cancellationToken = default);
    public Task<Spreadsheet?> GetSheetInfo(List<string>? ranges, CancellationToken cancellationToken = default);
    public Task<UpdateValuesResponse?> UpdateData(ValueRange valueRange, string range, CancellationToken cancellationToken = default);

    /// <summary>
    /// Same call as <see cref="GetBatchData(List{string}, string?, CancellationToken)"/>, but the
    /// failure reason survives instead of collapsing to <c>null</c>. Use this where the caller needs
    /// to tell a transient quota failure apart from a genuinely empty result - see
    /// <see cref="GoogleApiFailureReason.QuotaExceeded"/>.
    /// </summary>
    public Task<GoogleApiResult<BatchGetValuesByDataFilterResponse>> GetBatchDataResult(List<string> sheets, string? range = null, CancellationToken cancellationToken = default);

    /// <summary>Same call as <see cref="GetSheetInfo(List{string}, CancellationToken)"/>, with the failure reason preserved.</summary>
    public Task<GoogleApiResult<Spreadsheet>> GetSheetInfoResult(List<string>? ranges = null, CancellationToken cancellationToken = default);
}

[ExcludeFromCodeCoverage]
public class GoogleSheetService : IGoogleSheetService
{
    private readonly ISheetServiceWrapper _sheetService;
    private readonly string _range = GoogleConfig.Range;
    private readonly ILogger _logger;

    public GoogleSheetService(string accessToken, string spreadsheetId, ILogger? logger = null, GoogleRetryOptions? retryOptions = null)
    {
        _sheetService = new SheetServiceWrapper(accessToken, spreadsheetId, retryOptions);
        _logger = logger ?? NullLogger.Instance;
    }

    public GoogleSheetService(Dictionary<string, string> parameters, string spreadsheetId, ILogger? logger = null, GoogleRetryOptions? retryOptions = null)
    {
        _sheetService = new SheetServiceWrapper(parameters, spreadsheetId, retryOptions);
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Runs a call against the underlying wrapper, converting any exception into a classified
    /// <see cref="GoogleApiFailure"/> instead of letting it propagate. Every public method below is a
    /// thin wrapper around this, so the try/catch/log shape only exists once.
    /// </summary>
    private async Task<GoogleApiResult<T>> ExecuteAsync<T>(Func<Task<T>> call, string logMessage, params object?[] logArgs)
    {
        try
        {
            var response = await call();
            return GoogleApiResult<T>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, logMessage, logArgs);
            return GoogleApiResult<T>.Failed(GoogleApiFailure.FromException(ex));
        }
    }

    public async Task<AppendValuesResponse?> AppendData(ValueRange valueRange, string range, CancellationToken cancellationToken = default)
    {
        var result = await ExecuteAsync(
            () => _sheetService.AppendValues(range, valueRange, cancellationToken),
            "Error appending data to range '{Range}'", range);

        return result.Value;
    }

    public async Task<BatchUpdateValuesResponse?> BatchUpdateData(BatchUpdateValuesRequest batchUpdateValuesRequest, CancellationToken cancellationToken = default)
    {
        var result = await ExecuteAsync(
            () => _sheetService.BatchUpdateData(batchUpdateValuesRequest, cancellationToken),
            "Error batch updating values");

        return result.Value;
    }

    public async Task<BatchUpdateSpreadsheetResponse?> BatchUpdateSpreadsheet(BatchUpdateSpreadsheetRequest batchUpdateSpreadsheetRequest, CancellationToken cancellationToken = default)
    {
        var result = await ExecuteAsync(
            () => _sheetService.BatchUpdateSpreadsheet(batchUpdateSpreadsheetRequest, cancellationToken),
            "Error batch updating spreadsheet");

        return result.Value;
    }

    public async Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets, CancellationToken cancellationToken = default)
    {
        return await GetBatchData(sheets, null, cancellationToken);
    }

    public async Task<BatchGetValuesByDataFilterResponse?> GetBatchData(List<string> sheets, string? range, CancellationToken cancellationToken = default)
    {
        var result = await GetBatchDataResult(sheets, range, cancellationToken);
        return result.Value;
    }

    public async Task<GoogleApiResult<BatchGetValuesByDataFilterResponse>> GetBatchDataResult(List<string> sheets, string? range = null, CancellationToken cancellationToken = default)
    {
        if (sheets == null || sheets.Count < 1)
        {
            return GoogleApiResult<BatchGetValuesByDataFilterResponse>.Failed(new GoogleApiFailure
            {
                Reason = GoogleApiFailureReason.Unknown,
                Message = "No sheets were requested."
            });
        }

        var request = GoogleRequestHelpers.GenerateBatchGetValuesByDataFilterRequest(sheets, range);

        return await ExecuteAsync(
            () => _sheetService.BatchGetByDataFilter(request, cancellationToken),
            "Error batch getting data for sheets '{Sheets}'", string.Join(", ", sheets));
    }

    public async Task<ValueRange?> GetSheetData(string sheet, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sheet)) return null;

        var result = await ExecuteAsync(
            // NotFound (invalid spreadsheetId/range) or BadRequest (invalid sheet name)
            () => _sheetService.GetValues($"{sheet}!{_range}", cancellationToken),
            "Error getting values for sheet '{Sheet}'", sheet);

        return result.Value;
    }

    public async Task<Spreadsheet?> GetSheetInfo(CancellationToken cancellationToken = default)
    {
        return await GetSheetInfo(null, cancellationToken);
    }

    public async Task<Spreadsheet?> GetSheetInfo(List<string>? ranges, CancellationToken cancellationToken = default)
    {
        var result = await GetSheetInfoResult(ranges, cancellationToken);
        return result.Value;
    }

    public async Task<GoogleApiResult<Spreadsheet>> GetSheetInfoResult(List<string>? ranges = null, CancellationToken cancellationToken = default)
    {
        return await ExecuteAsync(
            () => _sheetService.GetSpreadsheet(ranges, cancellationToken),
            "Error getting sheet info for ranges '{Ranges}'", ranges == null ? "(none)" : string.Join(", ", ranges));
    }

    public async Task<UpdateValuesResponse?> UpdateData(ValueRange valueRange, string range, CancellationToken cancellationToken = default)
    {
        var result = await ExecuteAsync(
            () => _sheetService.UpdateValues(range, valueRange, cancellationToken),
            "Error updating data for range '{Range}'", range);

        return result.Value;
    }
}

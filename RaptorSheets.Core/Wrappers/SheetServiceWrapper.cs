using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Models;
using System.Diagnostics.CodeAnalysis;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource;

namespace RaptorSheets.Core.Wrappers;

public interface ISheetServiceWrapper
{
    Task<AppendValuesResponse> AppendValues(string range, IList<IList<object>> values);
    Task<AppendValuesResponse> AppendValues(string range, ValueRange valueRange);
    Task<BatchUpdateValuesResponse> BatchUpdateData(BatchUpdateValuesRequest batchUpdateValuesRequest);
    Task<BatchUpdateSpreadsheetResponse> BatchUpdateSpreadsheet(BatchUpdateSpreadsheetRequest batchUpdateSpreadsheetRequest);
    Task<BatchGetValuesByDataFilterResponse> BatchGetByDataFilter(BatchGetValuesByDataFilterRequest batchGetValuesByDataFilterRequest);
    Task<Spreadsheet> GetSpreadsheet(List<string>? ranges);
    Task<ValueRange> GetValues(string range);
    Task<UpdateValuesResponse> UpdateValues(string range, IList<IList<object>> values);
    Task<UpdateValuesResponse> UpdateValues(string range, ValueRange valueRange);
}

[ExcludeFromCodeCoverage]
public class SheetServiceWrapper : ISheetServiceWrapper
{
    private readonly SheetsService _sheetsService;
    private readonly string _spreadsheetId;

    public SheetServiceWrapper(string accessToken, string spreadsheetId, GoogleRetryOptions? retryOptions = null)
    {
        _spreadsheetId = spreadsheetId;
        var credential = GoogleCredential.FromAccessToken(accessToken.Trim());

        _sheetsService = CreateService(credential, retryOptions);
    }

    public SheetServiceWrapper(Dictionary<string, string> parameters, string spreadsheetId, GoogleRetryOptions? retryOptions = null)
    {
        _spreadsheetId = spreadsheetId;
        // Resolve credential parameters with tolerant key lookup to accept either
        // camelCase (privateKey, privateKeyId, clientEmail, clientId) or
        // snake_case (private_key, private_key_id, client_email, client_id) names.
        var jsonCredential = new JsonCredentialParameters
        {
            Type = ResolveParameter(parameters, "type"),
            PrivateKeyId = ResolveParameter(parameters, "privateKeyId", "private_key_id"),
            PrivateKey = ResolveParameter(parameters, "privateKey", "private_key"),
            ClientEmail = ResolveParameter(parameters, "clientEmail", "client_email"),
            ClientId = ResolveParameter(parameters, "clientId", "client_id"),
        };

        // Prefer constructing a service account credential explicitly instead of the obsolete FromJsonParameters API.
        // Build a ServiceAccountCredential from the provided parameters and convert to a GoogleCredential.
        var initializer = new ServiceAccountCredential.Initializer(jsonCredential.ClientEmail)
        {
            Scopes = new[] { SheetsService.Scope.Spreadsheets }
        };

        // The private key in parameters typically contains escaped newlines; ensure proper formatting
        var privateKey = jsonCredential.PrivateKey?.Replace("\\n", "\n");

        GoogleCredential credential;
        try
        {
            var serviceAccountCredential = new ServiceAccountCredential(initializer.FromPrivateKey(privateKey));
            // Convert to GoogleCredential for compatibility with APIs that expect it (and to be used as HttpClientInitializer)
            credential = serviceAccountCredential.ToGoogleCredential();
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            // Fail here rather than later. Unusable key material means every subsequent API call will
            // fail authentication, and diagnosing that from a 401 on the first request is far harder
            // than being told at construction which credential parameter was bad.
            throw new ArgumentException(
                "The private key credential parameter is not a valid PEM-encoded RSA private key.",
                nameof(parameters), ex);
        }

        _sheetsService = CreateService(credential, retryOptions);
    }

    private static string ResolveParameter(Dictionary<string, string> parameters, params string[] candidates)
    {
        if (parameters == null) throw new ArgumentNullException(nameof(parameters));

        // Direct lookup (exact key)
        foreach (var c in candidates)
        {
            if (parameters.TryGetValue(c, out var val) && !string.IsNullOrWhiteSpace(val))
            {
                return val.Trim();
            }
        }

        // Fallback: try normalized keys (remove underscores, case-insensitive)
        foreach (var kv in parameters)
        {
            var normalizedKey = kv.Key?.Replace("_", "").ToLowerInvariant() ?? string.Empty;
            foreach (var c in candidates)
            {
                var candidateNorm = c.Replace("_", "").ToLowerInvariant();
                if (normalizedKey == candidateNorm && !string.IsNullOrWhiteSpace(kv.Value))
                {
                    return kv.Value.Trim();
                }
            }
        }

        throw new ArgumentException($"Missing required parameter. Expected one of: {string.Join(", ", candidates)}", nameof(parameters));
    }

    private static SheetsService CreateService(
        Google.Apis.Http.IConfigurableHttpClientInitializer httpInitializer,
        GoogleRetryOptions? retryOptions)
    {
        var service = new SheetsService(
            GoogleServiceInitializerHelper.CreateInitializer(httpInitializer, retryOptions));

        GoogleServiceInitializerHelper.ApplyRateLimitBackOff(service, retryOptions);

        return service;
    }

    public async Task<AppendValuesResponse> AppendValues(string range, IList<IList<object>> values)
    {
        var valueRange = new ValueRange { Values = values };
        return await AppendValues(range, valueRange);
    }

    public async Task<AppendValuesResponse> AppendValues(string range, ValueRange valueRange)
    {
        var request = _sheetsService.Spreadsheets.Values.Append(valueRange, _spreadsheetId, range);
        request.ValueInputOption = AppendRequest.ValueInputOptionEnum.USERENTERED;
        return await request.ExecuteAsync();
    }

    public async Task<BatchGetValuesByDataFilterResponse> BatchGetByDataFilter(BatchGetValuesByDataFilterRequest batchGetValuesByDataFilterRequest)
    {
        var request = _sheetsService.Spreadsheets.Values.BatchGetByDataFilter(batchGetValuesByDataFilterRequest, _spreadsheetId);
        return await request.ExecuteAsync();
    }
    
    public async Task<BatchUpdateValuesResponse> BatchUpdateData(BatchUpdateValuesRequest batchUpdateValuesRequest)
    {
        var request = _sheetsService.Spreadsheets.Values.BatchUpdate(batchUpdateValuesRequest, _spreadsheetId);
        return await request.ExecuteAsync();
    }

    public async Task<BatchUpdateSpreadsheetResponse> BatchUpdateSpreadsheet(BatchUpdateSpreadsheetRequest batchUpdateSpreadsheetRequest)
    {
        var request = _sheetsService.Spreadsheets.BatchUpdate(batchUpdateSpreadsheetRequest, _spreadsheetId);
        return await request.ExecuteAsync();
    }

    public async Task<ValueRange> GetValues(string range)
    {
        var request = _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);
        var response = await request.ExecuteAsync();
        return response;
    }

    public async Task<Spreadsheet> GetSpreadsheet(List<string>? ranges)
    {
        var request = _sheetsService.Spreadsheets.Get(_spreadsheetId);
        if (ranges?.Count > 0)
        {
            request.IncludeGridData = true;
            request.Ranges = ranges;
        }
        return await request.ExecuteAsync();
    }

    public async Task<UpdateValuesResponse> UpdateValues(string range, IList<IList<object>> values)
    {
        var valueRange = new ValueRange { Values = values };
        return await UpdateValues(range, valueRange);
    }

    public async Task<UpdateValuesResponse> UpdateValues(string range, ValueRange valueRange)
    {
        var request = _sheetsService.Spreadsheets.Values.Update(valueRange, _spreadsheetId, range);
        request.ValueInputOption = UpdateRequest.ValueInputOptionEnum.USERENTERED;
        return await request.ExecuteAsync();
    }
}
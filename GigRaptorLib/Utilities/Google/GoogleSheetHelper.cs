using GigRaptorLib.Enums;
using GigRaptorLib.Utilities.Extensions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Configuration;

namespace GigRaptorLib.Utilities.Google;

public class GoogleSheetHelper
{
    private SheetsService _sheetsService;
    private readonly IConfiguration _configuration;
    private readonly string _range = "A1:Z1000";

    public GoogleSheetHelper()
    {
        _configuration = ConfigurationHelper.GetConfiguration();

        var jsonCredential = new JsonCredentialParameters
        {
            Type = _configuration.GetSection("google_credentials:type").Value,
            ProjectId = _configuration.GetSection("google_credentials:project_id").Value,
            PrivateKeyId = _configuration.GetSection("google_credentials:private_key_id").Value,
            PrivateKey = _configuration.GetSection("google_credentials:private_key").Value,
            ClientEmail = _configuration.GetSection("google_credentials:client_email").Value,
            ClientId = _configuration.GetSection("google_credentials:client_id").Value,
            TokenUrl = _configuration.GetSection("google_credentials:token_url").Value
        };

        var credential = GoogleCredential.FromJsonParameters(jsonCredential);

        _sheetsService = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "GigLogger"
        });
    }
    public async Task<IList<MatchedValueRange>> GetAllData(string spreadsheetId)
    {
        var body = new BatchGetValuesByDataFilterRequest
        {
            DataFilters = []
        };

        var sheets = Enum.GetValues(typeof(SheetEnum)).Cast<SheetEnum>();

        foreach (var sheet in sheets)
        {
            var filter = new DataFilter();
            filter.A1Range = sheet.DisplayName();
            body.DataFilters.Add(filter);
        }

        var batchGetRequest = _sheetsService.Spreadsheets.Values.BatchGetByDataFilter(body, spreadsheetId);
        var batchResponse = await batchGetRequest.ExecuteAsync();
        var values = batchResponse.ValueRanges;

        return values;
    }

    public async Task<IList<IList<object>>> GetSheetData(string spreadsheetId, SheetEnum sheetEnum)
    {
        var getRequest = _sheetsService.Spreadsheets.Values.Get(spreadsheetId, $"{sheetEnum.DisplayName()}!{_range}");

        var getResponse = await getRequest.ExecuteAsync();
        IList<IList<Object>> values = getResponse.Values;

        return values;

        //switch (sheetEnum)
        //{
        //    case SheetEnum.ADDRESSES:
        //        sheetEntity.Addresses = AddressMapper.MapFromRangeData(values);
        //        break;
        //    case SheetEnum.DAILY:
        //        sheetEntity.Daily = DailyMapper.MapFromRangeData(values);
        //        break;
        //    case SheetEnum.MONTHLY:
        //        sheetEntity.Monthly = MonthlyMapper.MapFromRangeData(values);
        //        break;
        //    case SheetEnum.NAMES:
        //        sheetEntity.Names = NameMapper.MapFromRangeData(values);
        //        break;
        //    case SheetEnum.PLACES:
        //        sheetEntity.Places = PlaceMapper.MapFromRangeData(values);
        //        break;
        //    case SheetEnum.REGIONS:
        //        sheetEntity.Regions = RegionMapper.MapFromRangeData(values);
        //        break;
        //    case SheetEnum.SERVICES:
        //        sheetEntity.Services = ServiceMapper.MapFromRangeData(values);
        //        break;
        //    case SheetEnum.SHIFTS:
        //        sheetEntity.Shifts = ShiftMapper.MapFromRangeData(values);
        //        break;
        //    case SheetEnum.TRIPS:
        //        sheetEntity.Trips = TripMapper.MapFromRangeData(values);
        //        break;
        //    case SheetEnum.TYPES:
        //        sheetEntity.Types = TypeMapper.MapFromRangeData(values);
        //        break;
        //    case SheetEnum.WEEKDAYS:
        //        sheetEntity.Weekdays = WeekdayMapper.MapFromRangeData(values);
        //        break;
        //    case SheetEnum.WEEKLY:
        //        sheetEntity.Weekly = WeeklyMapper.MapFromRangeData(values);
        //        break;
        //    case SheetEnum.YEARLY:
        //        sheetEntity.Yearly = YearlyMapper.MapFromRangeData(values);
        //        break;
        //    default:
        //        break;
        //}
    }

    public async Task<SpreadsheetProperties> GetSheetProperties(string spreadsheetId)
    {
        var getRequest = _sheetsService.Spreadsheets.Get(spreadsheetId);

        var getResponse = await getRequest.ExecuteAsync();
        var properties = getResponse.Properties;

        return properties;
    }
}

﻿using Google.Apis.Sheets.v4.Data;
using RLE.Core.Extensions;
using RLE.Core.Services;
using RLE.Stock.Enums;
using RLE.Test.Helpers;
using Xunit;

namespace RLE.Stock.Tests.Data;

public class GoogleDataFixture : IAsyncLifetime // https://xunit.net/docs/shared-context
{
    public async Task InitializeAsync()
    {
        var spreadsheetId = TestConfigurationHelpers.GetStockSpreadsheet();
        var credential = TestConfigurationHelpers.GetJsonCredential();

        var googleSheetService = new GoogleSheetService(credential, spreadsheetId);
        var sheets = Enum.GetValues(typeof(SheetEnum)).Cast<SheetEnum>().ToList();
        var result = await googleSheetService.GetBatchData(sheets.Select(x => x.GetDescription()).ToList());

        valueRanges = result?.ValueRanges;
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public IList<MatchedValueRange>? valueRanges { get; private set; }
}
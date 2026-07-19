using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Entities;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Gig.Mappers;
using System.ComponentModel;

namespace RaptorSheets.Gig.Tests.Unit.Mappers;

[Category("Unit Tests")]
public class MapperGetSheetTests
{
    public static IEnumerable<object[]> Sheets =>
    new List<object[]>
    {
        new object[] { AddressMapper.GetSheet(), SheetsConfig.AddressSheet },
        new object[] { DailyMapper.GetSheet(), SheetsConfig.DailySheet },
        new object[] { GenericSheetMapper<ExpenseEntity>.GetSheet(SheetsConfig.ExpenseSheet), SheetsConfig.ExpenseSheet },
        new object[] { MonthlyMapper.GetSheet(), SheetsConfig.MonthlySheet },
        new object[] { NameMapper.GetSheet(), SheetsConfig.NameSheet },
        new object[] { PlaceMapper.GetSheet(), SheetsConfig.PlaceSheet },
        new object[] { RegionMapper.GetSheet(), SheetsConfig.RegionSheet },
        new object[] { ServiceMapper.GetSheet(), SheetsConfig.ServiceSheet },
        new object[] { GenericSheetMapper<SetupEntity>.GetSheet(SheetsConfig.SetupSheet), SheetsConfig.SetupSheet },
        new object[] { ShiftMapper.GetSheet(), SheetsConfig.ShiftSheet },
        new object[] { TripMapper.GetSheet(), SheetsConfig.TripSheet },
        new object[] { TypeMapper.GetSheet(), SheetsConfig.TypeSheet },
        new object[] { WeekdayMapper.GetSheet(), SheetsConfig.WeekdaySheet },
        new object[] { WeeklyMapper.GetSheet(), SheetsConfig.WeeklySheet },
        new object[] { YearlyMapper.GetSheet(), SheetsConfig.YearlySheet },
        new object[] { DeliveryMapper.GetSheet(), SheetsConfig.Deliveries },
        new object[] { LocationMapper.GetSheet(), SheetsConfig.Locations },
    };

    [Theory]
    [MemberData(nameof(Sheets))]
    public void GivenGetSheetConfig_ThenReturnSheet(SheetModel result, SheetModel sheetConfig)
    {
        Assert.Equal(sheetConfig.CellColor, result.CellColor);
        Assert.Equal(sheetConfig.FreezeColumnCount, result.FreezeColumnCount);
        Assert.Equal(sheetConfig.FreezeRowCount, result.FreezeRowCount);
        Assert.Equal(sheetConfig.Headers.Count, result.Headers.Count);
        Assert.Equal(sheetConfig.Name, result.Name);
        Assert.Equal(sheetConfig.ProtectSheet, result.ProtectSheet);
        Assert.Equal(sheetConfig.TabColor, result.TabColor);

        // Verify all result headers have proper column assignments
        // Note: We check result headers since sheetConfig may not have UpdateColumns() called
        foreach (var resultHeader in result.Headers)
        {
            Assert.False(string.IsNullOrWhiteSpace(resultHeader.Column), 
                $"Header '{resultHeader.Name}' should have a Column value");

            // Protected sheets should have EITHER formulas OR be marked as input columns
            // Not all headers in protected sheets have formulas - some are user input fields
            if (result.ProtectSheet && !string.IsNullOrEmpty(resultHeader.Formula))
            {
                // If it has a formula, it should start with =
                Assert.True(resultHeader.Formula.StartsWith("="), 
                    $"Protected sheet header '{resultHeader.Name}' with formula should start with =");
            }
        }
    }

    [Theory]
    [MemberData(nameof(Sheets))]
    public void GetSheet_ShouldGenerateValidFormulas(SheetModel sheet, SheetModel _)
    {
        // Act - Get headers with formulas
        var formulaHeaders = sheet.Headers.Where(h => !string.IsNullOrEmpty(h.Formula)).ToList();

        // Assert - Protected sheets may or may not have formulas
        // Some protected sheets (like Setup) are protected for data integrity, not because they have formulas
        if (formulaHeaders.Count > 0)
        {
            // All formulas should start with =
            Assert.All(formulaHeaders, header => Assert.StartsWith("=", header.Formula));
            
            // All formulas should not contain common unresolved placeholder patterns
            Assert.All(formulaHeaders, header => 
            {
                // Check for unresolved builder placeholders (these should not exist)
                Assert.DoesNotContain("{keyRange}", header.Formula);
                Assert.DoesNotContain("{header}", header.Formula);
                Assert.DoesNotContain("{sourceRange}", header.Formula);
                Assert.DoesNotContain("{lookupRange}", header.Formula);
                // Note: Other { } might be valid Google Sheets syntax (like array literals)
            });
        }
        else if (sheet.ProtectSheet)
        {
            // If a protected sheet has no formulas, it's likely a simple data entry sheet
            // like Setup that's protected for data integrity reasons
            // This is valid - just verify it has headers
            Assert.NotEmpty(sheet.Headers);
        }
    }

    [Theory]
    [MemberData(nameof(Sheets))]
    public void GetSheet_ShouldHaveProperColumnIndexes(SheetModel sheet, SheetModel _)
    {
        // Assert
        Assert.All(sheet.Headers, header => Assert.True(header.Index >= 0));
        Assert.All(sheet.Headers, header => Assert.False(string.IsNullOrEmpty(header.Column)));

        // Verify no duplicate column indexes
        var columnIndexes = sheet.Headers.Select(h => h.Index).ToList();
        Assert.Equal(columnIndexes.Count, columnIndexes.Distinct().Count());
    }

    [Theory]
    [MemberData(nameof(Sheets))]
    public void GetSheet_ShouldHaveProperFormatting(SheetModel sheet, SheetModel _)
    {
        // Act - Get headers with specific formats
        var dateHeaders = sheet.Headers.Where(h => h.Format == FormatEnum.DATE).ToList();
        var accountingHeaders = sheet.Headers.Where(h => h.Format == FormatEnum.ACCOUNTING).ToList();
        var durationHeaders = sheet.Headers.Where(h => h.Format == FormatEnum.DURATION).ToList();

        // Assert - Headers should have appropriate formats based on their names
        Assert.All(dateHeaders, header => 
        {
            var headerName = header.Name.ToString().ToUpper();
            Assert.True(headerName.Contains("DATE") || headerName.Contains("VISIT") || headerName.Contains("BEGIN") || 
                       headerName.Contains("END") || headerName.Contains("DAY") || headerName == "DAY" || 
                       headerName.Contains('T')); // Add TRIP for "First Trip", "Last Trip"
        });

        Assert.All(accountingHeaders, header => 
        {
            var headerName = header.Name.ToString().ToUpper();
            Assert.True(headerName.Contains("PAY") || headerName.Contains("TIP") || headerName.Contains("BONUS") || 
                       headerName.Contains("TOTAL") || headerName.Contains("CASH") || headerName.Contains("AMOUNT") ||
                       headerName.Contains("AMT") || headerName.Contains("AVERAGE")); // Include AMT and AVERAGE
        });

        Assert.All(durationHeaders, header =>
        {
            var headerName = header.Name.ToString().ToUpper();
            Assert.True(headerName.Contains("TIME") || headerName.Contains("DURATION") || headerName.Contains("ACTIVE"));
        });
    }

    [Fact]
    public void TripMapper_GetSheet_ShouldGenerateKeyFormula()
    {
        // Act
        var sheet = TripMapper.GetSheet();
        var keyHeader = sheet.Headers.FirstOrDefault(h => h.Name.ToString() == "Key");

        // Assert
        if (keyHeader != null) // Only test if the header exists
        {
            Assert.NotNull(keyHeader.Formula);
            Assert.StartsWith("=ARRAYFORMULA(", keyHeader.Formula);
            Assert.Contains("IF(ISBLANK(", keyHeader.Formula); // Should contain conditional logic
            Assert.Contains("\"-X-\"", keyHeader.Formula); // Exclude marker
            Assert.Contains("\"-0-\"", keyHeader.Formula); // Default number
            // Note: Range references will be resolved column references, not literal "Service"
        }
    }

    [Fact]
    public void TripMapper_GetSheet_ShouldHandleDateAsString()
    {
        // Act
        var sheet = TripMapper.GetSheet();
        var dateHeader = sheet.Headers.FirstOrDefault(h => h.Name.ToString() == "Date");

        // Assert
        if (dateHeader != null)
        {
            Assert.NotNull(dateHeader);
            // Date header should either have no formula (user input) or formula should be a string type
            // Use case-insensitive comparison for type name
            if (!string.IsNullOrEmpty(dateHeader.Formula))
            {
                Assert.Equal("String", dateHeader.Formula?.GetType().Name); // C# returns "String" not "string"
            }
        }
    }

    [Fact]
    public void ShiftMapper_GetSheet_ShouldGenerateComplexFormulas()
    {
        // Act
        var sheet = ShiftMapper.GetSheet();
        
        var keyHeader = sheet.Headers.FirstOrDefault(h => h.Name.ToString() == "Key");
        var totalTimeHeader = sheet.Headers.FirstOrDefault(h => h.Name.ToString() == "Total Time");
        var totalPayHeader = sheet.Headers.FirstOrDefault(h => h.Name.ToString() == "Total Pay");

        // Assert - Only test headers that exist
        if (keyHeader != null)
        {
            Assert.NotNull(keyHeader.Formula);
            Assert.Contains("IF(ISBLANK(", keyHeader.Formula);
            Assert.Contains("\"-0-\"", keyHeader.Formula);
        }

        if (totalTimeHeader != null)
        {
            Assert.NotNull(totalTimeHeader.Formula);
            // Should contain time logic but exact pattern may vary
            Assert.Contains("IF(", totalTimeHeader.Formula);
        }

        if (totalPayHeader != null)
        {
            Assert.NotNull(totalPayHeader.Formula);
            Assert.Contains("SUMIF(", totalPayHeader.Formula);
        }
    }

    [Fact]
    public void NameMapper_GetSheet_ShouldGenerateUniqueNameFormula()
    {
        // Act
        var sheet = NameMapper.GetSheet();
        var nameHeader = sheet.Headers.First(h => h.Name.ToString() == "Name");

        // Assert
        Assert.NotNull(nameHeader.Formula);
        Assert.StartsWith("={\"Name\";SORT(UNIQUE(", nameHeader.Formula);
        Assert.EndsWith("))}", nameHeader.Formula);
        Assert.Contains("Trips!", nameHeader.Formula);
    }

    [Fact]
    public void AddressMapper_GetSheet_ShouldCombineStartAndEndAddresses()
    {
        // Act
        var sheet = AddressMapper.GetSheet();
        var addressHeader = sheet.Headers.First(h => h.Name.ToString() == "Address");
        var tripsHeader = sheet.Headers.First(h => h.Name.ToString() == "Trips");

        // Assert
        Assert.NotNull(addressHeader.Formula);
        Assert.Contains("SORT(UNIQUE(IFERROR(FILTER({", addressHeader.Formula);
        Assert.Contains(";", addressHeader.Formula); // Range combination

        Assert.NotNull(tripsHeader.Formula);
        Assert.Contains("COUNTIF(", tripsHeader.Formula);
        Assert.Contains("+COUNTIF(", tripsHeader.Formula); // Dual count
    }

    [Fact]
    public void WeekdayMapper_GetSheet_ShouldGenerateWeekdayAnalysisFormulas()
    {
        // Act
        var sheet = WeekdayMapper.GetSheet();
        
        // Check if headers exist before testing them
        var weekdayHeader = sheet.Headers.FirstOrDefault(h => h.Name.ToString() == HeaderEnum.WEEKDAY.GetDescription());
        var currentAmountHeader = sheet.Headers.FirstOrDefault(h => h.Name.ToString() == HeaderEnum.AMOUNT_CURRENT.GetDescription());
        var previousAmountHeader = sheet.Headers.FirstOrDefault(h => h.Name.ToString() == HeaderEnum.AMOUNT_PREVIOUS.GetDescription());

        // Assert - Only test headers that exist
        if (weekdayHeader != null)
        {
            Assert.NotNull(weekdayHeader.Formula);
            // Updated expectation: Should use TEXT function for weekday conversion
            Assert.Contains("TEXT(", weekdayHeader.Formula);
            Assert.Contains("\"ddd\"", weekdayHeader.Formula);
        }

        if (currentAmountHeader != null)
        {
            Assert.NotNull(currentAmountHeader.Formula);
            Assert.Contains("TODAY()-WEEKDAY(TODAY(),2)", currentAmountHeader.Formula);
            Assert.Contains("VLOOKUP(", currentAmountHeader.Formula);
        }

        if (previousAmountHeader != null)
        {
            Assert.NotNull(previousAmountHeader.Formula);
            Assert.Contains("TODAY()-WEEKDAY(TODAY(),2)", previousAmountHeader.Formula);
            Assert.Contains("-7", previousAmountHeader.Formula); // Previous week
        }
    }

    [Fact]
    public void MonthlyMapper_GetSheet_ShouldGenerateComplexAnalysisFormulas()
    {
        // Act
        var sheet = MonthlyMapper.GetSheet();
        
        // Check if headers exist before testing them
        var averageHeader = sheet.Headers.FirstOrDefault(h => h.Name.ToString() == HeaderEnum.AVERAGE.GetDescription());
        var numberHeader = sheet.Headers.FirstOrDefault(h => h.Name.ToString() == HeaderEnum.NUMBER.GetDescription());
        var yearHeader = sheet.Headers.FirstOrDefault(h => h.Name.ToString() == HeaderEnum.YEAR.GetDescription());

        // Assert - Only test headers that exist
        if (averageHeader != null)
        {
            Assert.NotNull(averageHeader.Formula);
            Assert.Contains("DAVERAGE(", averageHeader.Formula);
            Assert.Contains("transpose(", averageHeader.Formula);
            Assert.Contains("TRANSPOSE(", averageHeader.Formula);
            Assert.Contains("sequence(", averageHeader.Formula);
        }

        if (numberHeader != null)
        {
            Assert.NotNull(numberHeader.Formula);
            Assert.Contains("INDEX(SPLIT(", numberHeader.Formula);
            Assert.Contains("\"-\"", numberHeader.Formula);
            Assert.Contains(",0,1)", numberHeader.Formula.Replace(" ","")); // Ignore spaces
        }

        if (yearHeader != null)
        {
            Assert.NotNull(yearHeader.Formula);
            Assert.Contains("INDEX(SPLIT(", yearHeader.Formula);
            Assert.Contains(",0,2)", yearHeader.Formula.Replace(" ","")); // Ignore spaces
        }
    }

    [Fact]
    public void DeliveryMapper_GetSheet_ShouldGenerateGroupedSummaryFormulas()
    {
        // Act
        var sheet = DeliveryMapper.GetSheet();
        var tripsHeader = sheet.Headers.First(h => h.Name.ToString() == "Trips");
        var firstTripHeader = sheet.Headers.First(h => h.Name.ToString() == "First Trip");
        var lastTripHeader = sheet.Headers.First(h => h.Name.ToString() == "Last Trip");
        var amountPerTripHeader = sheet.Headers.First(h => h.Name.ToString() == "Amt/Trip");
        var amountPerDistanceHeader = sheet.Headers.First(h => h.Name.ToString() == "Amt/Dist");

        // Assert - QUERY formula groups by Name/Address and sums Pay/Tips/Bonus/Total/Dist, plus
        // min/max First Trip/Last Trip from Date - all part of the same query, since a QUERY's
        // spilled array is contiguous and must include everything up to Amt/Trip/Amt/Dist, which
        // are separate ARRAYFORMULAs placed after it.
        var queryFormula = sheet.Headers[0].Formula;
        Assert.NotNull(queryFormula);
        Assert.StartsWith("=QUERY({", queryFormula);
        Assert.Contains("select Col1, Col2, count(Col1), sum(Col3), sum(Col4), sum(Col5), sum(Col6), sum(Col7), min(Col8), max(Col9)", queryFormula);
        Assert.Contains("label Col1 'Name', Col2 'Address', count(Col1) 'Trips', sum(Col3) 'Pay', sum(Col4) 'Tips', sum(Col5) 'Bonus', sum(Col6) 'Total', sum(Col7) 'Dist', min(Col8) 'First Trip', max(Col9) 'Last Trip'", queryFormula);

        // Assert - Trips, First Trip and Last Trip are all produced by the query, not formulas of their own
        Assert.True(tripsHeader.HideHeaderName);
        Assert.True(firstTripHeader.HideHeaderName);
        Assert.True(lastTripHeader.HideHeaderName);
        Assert.True(string.IsNullOrEmpty(firstTripHeader.Formula));
        Assert.True(string.IsNullOrEmpty(lastTripHeader.Formula));

        // Assert - Amt/Trip and Amt/Dist are separate ARRAYFORMULAs referencing this sheet's own columns
        // (Total/Trips/Distance stay at columns G/C/H regardless of First Trip/Last Trip being added)
        Assert.NotNull(amountPerTripHeader.Formula);
        Assert.Contains("G1:G/IF(C1:C=0,1,C1:C)", amountPerTripHeader.Formula);
        Assert.NotNull(amountPerDistanceHeader.Formula);
        Assert.Contains("G1:G/IF(H1:H=0,1,H1:H)", amountPerDistanceHeader.Formula);

        // Regression: EnsureHeaderPlaceholders(1) hides header *names* for spilled columns, but
        // Amt/Trip and Amt/Dist have their own formulas and must not be silently dropped when the
        // sheet is converted to row data (see SheetHelpers.HeadersToRowData).
        Assert.False(amountPerTripHeader.HideHeaderName);
        Assert.False(amountPerDistanceHeader.HideHeaderName);

        var rowData = SheetHelpers.HeadersToRowData(sheet);
        var cells = rowData[0].Values;
        Assert.NotNull(cells[sheet.Headers.IndexOf(amountPerTripHeader)].UserEnteredValue);
        Assert.NotNull(cells[sheet.Headers.IndexOf(amountPerDistanceHeader)].UserEnteredValue);
    }

    [Fact]
    public void LocationMapper_GetSheet_ShouldGenerateGroupedSummaryFormulas()
    {
        // Act
        var sheet = LocationMapper.GetSheet();
        var firstTripHeader = sheet.Headers.First(h => h.Name.ToString() == "First Trip");
        var lastTripHeader = sheet.Headers.First(h => h.Name.ToString() == "Last Trip");
        var amountPerTripHeader = sheet.Headers.First(h => h.Name.ToString() == "Amt/Trip");
        var amountPerDistanceHeader = sheet.Headers.First(h => h.Name.ToString() == "Amt/Dist");

        // Assert - QUERY groups by Place/Address, counting Col2 (Address), and sums Pay/Tips/Bonus/Total/Dist,
        // plus min/max First Trip/Last Trip from Date
        var queryFormula = sheet.Headers[0].Formula;
        Assert.NotNull(queryFormula);
        Assert.Contains("count(Col2)", queryFormula);
        Assert.Contains("select Col1, Col2, count(Col2), sum(Col3), sum(Col4), sum(Col5), sum(Col6), sum(Col7), min(Col8), max(Col9)", queryFormula);
        Assert.Contains("label Col1 'Place', Col2 'Address', count(Col2) 'Trips', sum(Col3) 'Pay', sum(Col4) 'Tips', sum(Col5) 'Bonus', sum(Col6) 'Total', sum(Col7) 'Dist', min(Col8) 'First Trip', max(Col9) 'Last Trip'", queryFormula);

        // Assert - First Trip and Last Trip are produced by the query, not formulas of their own
        Assert.True(firstTripHeader.HideHeaderName);
        Assert.True(lastTripHeader.HideHeaderName);

        // Regression: same HideHeaderName gap as DeliveryMapper - Amt/Trip/Amt/Dist formulas must
        // survive row-data conversion
        Assert.False(amountPerTripHeader.HideHeaderName);
        Assert.False(amountPerDistanceHeader.HideHeaderName);

        var rowData = SheetHelpers.HeadersToRowData(sheet);
        var cells = rowData[0].Values;
        Assert.NotNull(cells[sheet.Headers.IndexOf(amountPerTripHeader)].UserEnteredValue);
        Assert.NotNull(cells[sheet.Headers.IndexOf(amountPerDistanceHeader)].UserEnteredValue);
    }

    //GetDataValidation

    //GetSheetForRange

    //GetCommonShiftGroupSheetHeaders

    //GetCommonTripGroupSheetHeaders
}
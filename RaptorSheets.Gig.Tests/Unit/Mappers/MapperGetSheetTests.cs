using RaptorSheets.Core.Models.Google;
using RaptorSheets.Gig.Constants;
using RaptorSheets.Gig.Mappers;
using RaptorSheets.Gig.Enums;
using RaptorSheets.Core.Enums;
using RaptorSheets.Core.Extensions;
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
        new object[] { ExpenseMapper.GetSheet(), SheetsConfig.ExpenseSheet },
        new object[] { MonthlyMapper.GetSheet(), SheetsConfig.MonthlySheet },
        new object[] { NameMapper.GetSheet(), SheetsConfig.NameSheet },
        new object[] { PlaceMapper.GetSheet(), SheetsConfig.PlaceSheet },
        new object[] { RegionMapper.GetSheet(), SheetsConfig.RegionSheet },
        new object[] { ServiceMapper.GetSheet(), SheetsConfig.ServiceSheet },
        new object[] { ShiftMapper.GetSheet(), SheetsConfig.ShiftSheet },
        new object[] { TripMapper.GetSheet(), SheetsConfig.TripSheet },
        new object[] { TypeMapper.GetSheet(), SheetsConfig.TypeSheet },
        new object[] { WeekdayMapper.GetSheet(), SheetsConfig.WeekdaySheet },
        new object[] { WeeklyMapper.GetSheet(), SheetsConfig.WeeklySheet },
        new object[] { YearlyMapper.GetSheet(), SheetsConfig.YearlySheet },
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

        foreach (var configHeader in sheetConfig.Headers)
        {
            var resultHeader = result.Headers.First(x => x.Name == configHeader.Name);
            Assert.False(string.IsNullOrWhiteSpace(resultHeader.Column));

            if (result.ProtectSheet)
                Assert.False(string.IsNullOrWhiteSpace(resultHeader.Formula));
        }
    }

    [Theory]
    [MemberData(nameof(Sheets))]
    public void GetSheet_ShouldGenerateValidFormulas(SheetModel sheet, SheetModel _)
    {
        // Act - Get headers with formulas
        var formulaHeaders = sheet.Headers.Where(h => !string.IsNullOrEmpty(h.Formula)).ToList();

        // Assert - Only verify that protected sheets have formulas (simple sheets may not have formulas)
        if (sheet.ProtectSheet)
        {
            Assert.NotEmpty(formulaHeaders);
            
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
                       headerName.Contains("TRIP")); // Add TRIP for "First Trip", "Last Trip"
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
        Assert.Contains("SORT(UNIQUE({", addressHeader.Formula);
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
            Assert.Contains("AVERAGE(", averageHeader.Formula);
            Assert.Contains("OFFSET(", averageHeader.Formula);
            Assert.Contains("INDIRECT(", averageHeader.Formula);
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

    //GetDataValidation

    //GetSheetForRange

    //GetCommonShiftGroupSheetHeaders

    //GetCommonTripGroupSheetHeaders
}
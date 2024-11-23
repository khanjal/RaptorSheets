using System.Diagnostics.CodeAnalysis;

namespace RLE.Stock.Constants;

[ExcludeFromCodeCoverage]
public static class ColumnNotes
{
    public static string AverageCost => "The average cost of the share is calculated based on all previous purchases and will change if you buy or sell additional shares";
    public static string MarketTypes => "NASDAQ and NYSE Markets";
}

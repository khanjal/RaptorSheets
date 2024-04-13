using GigRaptorLib.Enums;
using GigRaptorLib.Tests.Data.Helpers;
using GigRaptorLib.Utilities;
using Newtonsoft.Json.Linq;

namespace GigRaptorLib.Tests.Utilities;

public class HeaderParserTests
{
    private static IList<IList<object>>? _values;
    private static Dictionary<int, string>? _headers;

    public HeaderParserTests()
    {
        _values = JsonHelpers.LoadJsonData("Headers");
        _headers = HeaderParser.ParserHeader(_values[0]);
    }

    [Theory]
    [InlineData("String", "text")]
    [InlineData("StringEmpty", "")]
    [InlineData("StringInvalid", "")]
    public void GivenStringHeaders_ThenReturnParsedValue(string header, string value)
    {
        Assert.NotNull(HeaderParser.GetStringValue(header, _values[1], _headers));
        Assert.Equivalent(value, HeaderParser.GetStringValue(header, _values[1], _headers));
    }

    [Theory]
    [InlineData("Int", 1)]
    [InlineData("IntEmpty", 0)]
    [InlineData("IntInvalid", 0)]
    public void GivenIntHeaders_ThenReturnParsedValue(string header, int value)
    {
        Assert.Equivalent(value, HeaderParser.GetIntValue(header, _values[1], _headers));
    }

    [Theory]
    [InlineData("Decimal", "2.75")]
    [InlineData("DecimalEmpty", "0")]
    [InlineData("DecimalInvalid", "0")]
    public void GivenDecimalHeaders_ThenReturnParsedValue(string header, string value)
    {
        var decimalValue = decimal.Parse(value);
        Assert.Equivalent(decimalValue, HeaderParser.GetDecimalValue(header, _values[1], _headers));
    }

    [Theory]
    [InlineData("BoolTrue", true)]
    [InlineData("BoolFalse", false)]
    [InlineData("BoolEmpty", false)]
    [InlineData("BoolInvalid", false)]
    public void GivenBoolHeaders_ThenReturnParsedValue(string header, bool value)
    {
        Assert.Equivalent(value, HeaderParser.GetBoolValue(header, _values[1], _headers));
    }

    [Theory]
    [InlineData("Date", "2024-03-22")]
    [InlineData("DateEmpty", "")]
    [InlineData("DateInvalid", "")]
    public void GivenDateHeaders_ThenReturnParsedValue(string header, string value)
    {
        Assert.Equivalent(value, HeaderParser.GetDateValue(header, _values[1], _headers));
    }
}

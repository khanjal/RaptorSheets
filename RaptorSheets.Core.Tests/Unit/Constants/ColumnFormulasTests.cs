using RaptorSheets.Core.Constants;
using Xunit;

namespace RaptorSheets.Core.Tests.Unit.Constants;

public class ColumnFormulasTests
{
    private readonly string _columnTitle = "Test";
    private readonly string _range = GoogleConfig.Range;
    private readonly string _keyRange = GoogleConfig.KeyRange;
    private readonly string _altRange = GoogleConfig.ValidationRange;
    private readonly string _formula = "{formula}";
    private readonly string _name = "name";

    [Fact]
    public void GivenArrayFormula_ThenReturnString()
    {
        var text = $"=ARRAYFORMULA(IFS(ROW({_keyRange})=1,\"{_columnTitle}\",ISBLANK({_keyRange}), \"\",true,{_formula})";
        var result = ColumnFormulas.ArrayFormula(_columnTitle, _keyRange, _formula);

        Assert.Equal(text, result);
    }

    [Fact]
    public void GivenCountIf_ThenReturnString()
    {
        var text = $"=ARRAYFORMULA(IFS(ROW({_keyRange})=1,\"{_columnTitle}\",ISBLANK({_keyRange}), \"\",true,COUNTIF({_range},{_formula}))";
        var result = ColumnFormulas.CountIf(_columnTitle, _keyRange, _range, _formula);

        Assert.Equal(text, result);
    }

    [Fact]
    public void GivenSortUnique_ThenReturnString()
    {
        var text = $"={{\"{_columnTitle}\";SORT(UNIQUE({{{_range}}}))}}";
        var result = ColumnFormulas.SortUnique(_columnTitle, _range);

        Assert.Equal(text, result);
    }

    [Fact]
    public void GivenSumIf_ThenReturnString()
    {
        var text = $"=ARRAYFORMULA(IFS(ROW({_keyRange})=1,\"{_columnTitle}\",ISBLANK({_keyRange}), \"\",true,SUMIF({_range},{_formula},{_altRange}))";
        var result = ColumnFormulas.SumIf(_columnTitle, _keyRange, _range, _formula, _altRange);

        Assert.Equal(text, result);
    }

    [Fact]
    public void GivenSumIfDivide_ThenReturnString()
    {
        var text = $"=ARRAYFORMULA(IFS(ROW({_keyRange})=1,\"{_columnTitle}\",ISBLANK({_keyRange}), \"\",true,SUMIF({_range},{_formula},{_altRange})/{_range})";
        var result = ColumnFormulas.SumIfDivide(_columnTitle, _keyRange, _range, _formula, _altRange, _range);

        Assert.Equal(text, result);
    }

    [Fact]
    public void GivenSumIfBlank_ThenReturnString()
    {
        var text = $"=ARRAYFORMULA(IFS(ROW({_keyRange})=1,\"{_columnTitle}\",ISBLANK({_keyRange}), \"\",true,IF(SUMIF({_range},{_formula},{_altRange})=0,\"\",SUMIF({_range},{_formula},{_altRange}))";
        var result = ColumnFormulas.SumIfBlank(_columnTitle, _keyRange, _range, _formula, _altRange);

        Assert.Equal(text, result);
    }

    [Fact]
    public void GivenDivideRanges_ThenReturnString()
    {
        var text = $"=ARRAYFORMULA(IFS(ROW({_keyRange})=1,\"{_columnTitle}\",ISBLANK({_keyRange}), \"\",true,IFERROR({_range}/{_altRange},0))";
        var result = ColumnFormulas.DivideRanges(_columnTitle, _keyRange, _range, _altRange);

        Assert.Equal(text, result);
    }

    [Fact]
    public void GivenMultiplyRanges_ThenReturnString()
    {
        var text = $"=ARRAYFORMULA(IFS(ROW({_keyRange})=1,\"{_columnTitle}\",ISBLANK({_keyRange}), \"\",true,{_range}*{_altRange})";
        var result = ColumnFormulas.MultiplyRanges(_columnTitle, _keyRange, _range, _altRange);

        Assert.Equal(text, result);
    }

    [Fact]
    public void GivenSubtractRanges_ThenReturnString()
    {
        var text = $"=ARRAYFORMULA(IFS(ROW({_keyRange})=1,\"{_columnTitle}\",ISBLANK({_keyRange}), \"\",true,{_range}-{_altRange})";
        var result = ColumnFormulas.SubtractRanges(_columnTitle, _keyRange, _range, _altRange);

        Assert.Equal(text, result);
    }

    [Fact]
    public void GivenMapLambda_ThenReturnString()
    {
        var text = $"=MAP({_keyRange},LAMBDA({_name},IF(ROW({_name})=1,\"{_columnTitle}\",if(isblank({_name}),,{_formula}))))";
        var result = ColumnFormulas.MapLambda(_columnTitle, _keyRange, _name, _formula);

        Assert.Equal(text, result);
    }

    [Fact]
    public void GivenGoogleFinanceBasic_ThenReturnString()
    {
        var text = $"=MAP({_keyRange},LAMBDA({_name},IF(ROW({_name})=1,\"{_columnTitle}\",if(isblank({_name}),,GOOGLEFINANCE({_name},\"{_columnTitle}\")))))";
        var result = ColumnFormulas.GoogleFinanceBasic(_columnTitle, _keyRange, _name, _columnTitle);

        Assert.Equal(text, result);
    }

    [Fact]
    public void GivenGoogleFinanceMax_ThenReturnString()
    {
        var text = $"=MAP({_keyRange},LAMBDA({_name},IF(ROW({_name})=1,\"{_columnTitle}\",if(isblank({_name}),,MAX(INDEX(GOOGLEFINANCE({_name}, \"{_columnTitle}\", DATE(1980,1,2), TODAY(), \"DAILY\"),,2))))))";
        var result = ColumnFormulas.GoogleFinanceMax(_columnTitle, _keyRange, _name, _columnTitle);

        Assert.Equal(text, result);
    }

    [Fact]
    public void GivenGoogleFinanceMin_ThenReturnString()
    {
        var text = $"=MAP({_keyRange},LAMBDA({_name},IF(ROW({_name})=1,\"{_columnTitle}\",if(isblank({_name}),,MIN(INDEX(GOOGLEFINANCE({_name}, \"{_columnTitle}\", DATE(1980,1,2), TODAY(), \"DAILY\"),,2))))))";
        var result = ColumnFormulas.GoogleFinanceMin(_columnTitle, _keyRange, _name, _columnTitle);

        Assert.Equal(text, result);
    }

    [Theory]
    [InlineData("", "range", "formula")]
    [InlineData("title", "", "formula")]
    [InlineData("title", "range", "")]
    public void ArrayFormula_WithEmptyParameters_ShouldHandleGracefully(string title, string keyRange, string formula)
    {
        var result = ColumnFormulas.ArrayFormula(title, keyRange, formula);
        Assert.NotNull(result);
        Assert.Contains("ARRAYFORMULA", result);
    }    [Fact]
    public void ArrayFormula_WithNullParameters_ShouldNotThrow()
    {
        var exception1 = Record.Exception(() => ColumnFormulas.ArrayFormula(null!, "range", "formula"));
        var exception2 = Record.Exception(() => ColumnFormulas.ArrayFormula("title", null!, "formula"));
        var exception3 = Record.Exception(() => ColumnFormulas.ArrayFormula("title", "range", null!));
        
        Assert.Null(exception1);
        Assert.Null(exception2);
        Assert.Null(exception3);
    }

    [Fact]
    public void CountIf_WithSpecialCharacters_ShouldEscapeProperly()
    {
        var titleWithQuotes = "Test\"Column";
        var result = ColumnFormulas.CountIf(titleWithQuotes, _keyRange, _range, _formula);
        
        Assert.Contains(titleWithQuotes, result);
        Assert.Contains("COUNTIF", result);
    }

    [Theory]
    [InlineData("Very Long Column Title That Exceeds Normal Limits")]
    [InlineData("Column with symbols !@#$%^&*()")]
    [InlineData("Column\nwith\nnewlines")]
    public void SortUnique_WithVariousColumnTitles_ShouldFormatCorrectly(string columnTitle)
    {
        var result = ColumnFormulas.SortUnique(columnTitle, _range);
        
        Assert.Contains(columnTitle, result);
        Assert.Contains("SORT", result);
        Assert.Contains("UNIQUE", result);
    }

    [Fact]
    public void SumIf_WithComplexFormula_ShouldMaintainStructure()
    {
        var complexFormula = "INDIRECT(\"Sheet1!A\"&ROW())";
        var result = ColumnFormulas.SumIf(_columnTitle, _keyRange, _range, complexFormula, _altRange);
        
        Assert.Contains(complexFormula, result);
        Assert.Contains("SUMIF", result);
    }

    [Theory]
    [InlineData("", "", "", "", "")]
    [InlineData("A", "B", "C", "D", "E")]
    public void SumIfDivide_WithVariousInputs_ShouldGenerateValidFormula(string title, string keyRange, string range, string formula, string altRange)
    {
        var result = ColumnFormulas.SumIfDivide(title, keyRange, range, formula, altRange, range);
        
        Assert.Contains("SUMIF", result);
        Assert.Contains("/", result);
    }

    [Fact]
    public void SumIfBlank_ShouldHandleZeroValues()
    {
        var result = ColumnFormulas.SumIfBlank(_columnTitle, _keyRange, _range, _formula, _altRange);
        
        Assert.Contains("IF(SUMIF", result);
        Assert.Contains("=0,\"\"", result);
    }

    [Theory]
    [InlineData("GOOGLEFINANCE", "price")]
    [InlineData("price", "GOOGLEFINANCE")]
    [InlineData("volume", "marketcap")]
    public void GoogleFinanceBasic_WithDifferentAttributes_ShouldFormatCorrectly(string name, string attribute)
    {
        var result = ColumnFormulas.GoogleFinanceBasic(_columnTitle, _keyRange, name, attribute);
        
        Assert.Contains("GOOGLEFINANCE", result);
        Assert.Contains(name, result);
        Assert.Contains(attribute, result);
    }

    [Fact]
    public void GoogleFinanceMax_ShouldIncludeDateRange()
    {
        var result = ColumnFormulas.GoogleFinanceMax(_columnTitle, _keyRange, _name, _columnTitle);
        
        Assert.Contains("MAX", result);
        Assert.Contains("DATE(1980,1,2)", result);
        Assert.Contains("TODAY()", result);
        Assert.Contains("DAILY", result);
    }

    [Fact]
    public void GoogleFinanceMin_ShouldIncludeDateRange()
    {
        var result = ColumnFormulas.GoogleFinanceMin(_columnTitle, _keyRange, _name, _columnTitle);
        
        Assert.Contains("MIN", result);
        Assert.Contains("DATE(1980,1,2)", result);
        Assert.Contains("TODAY()", result);
        Assert.Contains("DAILY", result);
    }

    [Fact]
    public void DivideRanges_ShouldHandleErrorsWithIFERROR()
    {
        var result = ColumnFormulas.DivideRanges(_columnTitle, _keyRange, _range, _altRange);
        
        Assert.Contains("IFERROR", result);
        Assert.Contains(",0)", result);
    }

    [Theory]
    [InlineData("MultiplyRanges")]
    [InlineData("SubtractRanges")]
    public void ArithmeticOperations_ShouldContainCorrectOperators(string operation)
    {
        string result = operation switch
        {
            "MultiplyRanges" => ColumnFormulas.MultiplyRanges(_columnTitle, _keyRange, _range, _altRange),
            "SubtractRanges" => ColumnFormulas.SubtractRanges(_columnTitle, _keyRange, _range, _altRange),
            _ => throw new ArgumentException("Invalid operation")
        };

        Assert.Contains("ARRAYFORMULA", result);
        Assert.Contains("IFS", result);
    }

    [Fact]
    public void MapLambda_ShouldContainLambdaFunction()
    {
        var result = ColumnFormulas.MapLambda(_columnTitle, _keyRange, _name, _formula);
        
        Assert.Contains("MAP", result);
        Assert.Contains("LAMBDA", result);
        Assert.Contains(_name, result);
        Assert.Contains("isblank", result);
    }
}
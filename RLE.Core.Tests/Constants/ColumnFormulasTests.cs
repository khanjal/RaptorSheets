using FluentAssertions;
using RLE.Core.Constants;
using Xunit;

namespace RLE.Core.Tests.Constants;

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

        result.Should().Be(text);
    }

    [Fact]
    public void GivenCountIf_ThenReturnString()
    {
        var text = $"=ARRAYFORMULA(IFS(ROW({_keyRange})=1,\"{_columnTitle}\",ISBLANK({_keyRange}), \"\",true,COUNTIF({_range},{_formula}))";
        var result = ColumnFormulas.CountIf(_columnTitle, _keyRange, _range, _formula);

        result.Should().Be(text);
    }

    [Fact]
    public void GivenSortUnique_ThenReturnString()
    {
        var text = $"={{\"{_columnTitle}\";SORT(UNIQUE({{{_range}}}))}}";
        var result = ColumnFormulas.SortUnique(_columnTitle, _range);

        result.Should().Be(text);
    }

    [Fact]
    public void GivenSumIf_ThenReturnString()
    {
        var text = $"=ARRAYFORMULA(IFS(ROW({_keyRange})=1,\"{_columnTitle}\",ISBLANK({_keyRange}), \"\",true,SUMIF({_range},{_formula},{_altRange}))";
        var result = ColumnFormulas.SumIf(_columnTitle, _keyRange, _range, _formula, _altRange);

        result.Should().Be(text);
    }

    [Fact]
    public void GivenSumIfDivide_ThenReturnString()
    {
        var text = $"=ARRAYFORMULA(IFS(ROW({_keyRange})=1,\"{_columnTitle}\",ISBLANK({_keyRange}), \"\",true,SUMIF({_range},{_formula},{_altRange})/{_range})";
        var result = ColumnFormulas.SumIfDivide(_columnTitle, _keyRange, _range, _formula, _altRange, _range);

        result.Should().Be(text);
    }

    [Fact]
    public void GivenSumIfBlank_ThenReturnString()
    {
        var text = $"=ARRAYFORMULA(IFS(ROW({_keyRange})=1,\"{_columnTitle}\",ISBLANK({_keyRange}), \"\",true,IF(SUMIF({_range},{_formula},{_altRange})=0,\"\",SUMIF({_range},{_formula},{_altRange}))";
        var result = ColumnFormulas.SumIfBlank(_columnTitle, _keyRange, _range, _formula, _altRange);

        result.Should().Be(text);
    }

    [Fact]
    public void GivenDivideRanges_ThenReturnString()
    {
        var text = $"=ARRAYFORMULA(IFS(ROW({_keyRange})=1,\"{_columnTitle}\",ISBLANK({_keyRange}), \"\",true,IFERROR({_range}/{_altRange},0))";
        var result = ColumnFormulas.DivideRanges(_columnTitle, _keyRange, _range, _altRange);

        result.Should().Be(text);
    }

    [Fact]
    public void GivenMultiplyRanges_ThenReturnString()
    {
        var text = $"=ARRAYFORMULA(IFS(ROW({_keyRange})=1,\"{_columnTitle}\",ISBLANK({_keyRange}), \"\",true,{_range}*{_altRange})";
        var result = ColumnFormulas.MultiplyRanges(_columnTitle, _keyRange, _range, _altRange);

        result.Should().Be(text);
    }

    [Fact]
    public void GivenSubtractRanges_ThenReturnString()
    {
        var text = $"=ARRAYFORMULA(IFS(ROW({_keyRange})=1,\"{_columnTitle}\",ISBLANK({_keyRange}), \"\",true,{_range}-{_altRange})";
        var result = ColumnFormulas.SubtractRanges(_columnTitle, _keyRange, _range, _altRange);

        result.Should().Be(text);
    }

    [Fact]
    public void GivenMapLambda_ThenReturnString()
    {
        var text = $"=MAP({_keyRange},LAMBDA({_name},IF(ROW({_name})=1,\"{_columnTitle}\",if(isblank({_name}),,{_formula}))))";
        var result = ColumnFormulas.MapLambda(_columnTitle, _keyRange, _name, _formula);

        result.Should().Be(text);
    }

    [Fact]
    public void GivenGoogleFinanceBasic_ThenReturnString()
    {
        var text = $"=MAP({_keyRange},LAMBDA({_name},IF(ROW({_name})=1,\"{_columnTitle}\",if(isblank({_name}),,GOOGLEFINANCE({_name},\"{_columnTitle}\")))))";
        var result = ColumnFormulas.GoogleFinanceBasic(_columnTitle, _keyRange, _name, _columnTitle);

        result.Should().Be(text);
    }

    [Fact]
    public void GivenGoogleFinanceMax_ThenReturnString()
    {
        var text = $"=MAP({_keyRange},LAMBDA({_name},IF(ROW({_name})=1,\"{_columnTitle}\",if(isblank({_name}),,MAX(INDEX(GOOGLEFINANCE({_name}, \"{_columnTitle}\", DATE(1980,1,2), TODAY(), \"DAILY\"),,2))))))";
        var result = ColumnFormulas.GoogleFinanceMax(_columnTitle, _keyRange, _name, _columnTitle);

        result.Should().Be(text);
    }

    [Fact]
    public void GivenGoogleFinanceMin_ThenReturnString()
    {
        var text = $"=MAP({_keyRange},LAMBDA({_name},IF(ROW({_name})=1,\"{_columnTitle}\",if(isblank({_name}),,MIN(INDEX(GOOGLEFINANCE({_name}, \"{_columnTitle}\", DATE(1980,1,2), TODAY(), \"DAILY\"),,2))))))";
        var result = ColumnFormulas.GoogleFinanceMin(_columnTitle, _keyRange, _name, _columnTitle);

        result.Should().Be(text);
    }
}

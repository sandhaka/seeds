using Monads.Option;
using Tests.Option.Support;
using Xunit.Abstractions;

// ReSharper disable EqualExpressionComparison

namespace Tests.Option;

public class OptionTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void ShouldConvertAnObjectToAnOption()
    {
        var sampleTask = Task.Run(() => 1);
        var some = Option<Task>.Some(sampleTask);
        var none = Option<Task>.None();
        
        Assert.Equal(sampleTask, some.Reduce(Task.CompletedTask));
        Assert.NotEqual(sampleTask, none.Reduce(Task.CompletedTask));
    }

    [Fact]
    public void ShouldMapReduce()
    {
        var some = ValueOption<int>.Some(5);
        var result = some.MapValue(x => x * 2).Reduce(0);
        Assert.Equal(10, result);

        var someLiteral = Option<string>.Some("hello");
        var upperLiteral = someLiteral.Map(c => c.ToUpper());
        Assert.Equal("HELLO", upperLiteral.Reduce("hello"));
    }
    
    [Fact]
    public void ShouldWhereReturnSomeWhenPredicateIsTrue()
    {
        var some = ValueOption<int>.Some(5);
        var result = some.Where(x => x > 0);
        Assert.Equal(some, result);
    }
    
    [Fact]
    public void ShouldWhereReturnNoneWhenPredicateIsFalse()
    {
        var none = ValueOption<int>.None();
        var result = none.Where(x => x > 0);
        Assert.Equal(none, result);
    }

    [Fact]
    public void ShouldManageOptionsLikeEquitable()
    {
        var some = Option<Task>.Some(Task.CompletedTask);
        Assert.Equal(some, some);
        Assert.NotEqual(some, Option<Task>.None());
        Assert.NotEqual(some, Option<Task>.Some(Task.Delay(100)));
    }

    [Fact]
    public void ShouldUseEqualityOperator()
    {
        var some = Option<Task>.Some(Task.CompletedTask);
        Assert.True(some == some);
        Assert.False(some == Option<Task>.None());
        Assert.False(some == Option<Task>.Some(Task.Delay(100)));
    }

    [Fact]
    public void ShouldUseInSelectorExtendedTest()
    {
        var bankDeposit = new BankDeposit();

        var supportedCurrencies = bankDeposit.MultiCurrencies
            .GroupBy(x => x.Map(v => v.Currency.Reduce(string.Empty)))
            .OrderBy(x => x.Key.Reduce(string.Empty))
            .Select(x => x.Key.Reduce(string.Empty))
            .ToList();
        
        testOutputHelper.WriteLine($"Supported currencies: {supportedCurrencies.Count}");
        foreach (var currency in supportedCurrencies)
            testOutputHelper.WriteLine(currency);
        
        Assert.Equal("EUR", supportedCurrencies.First());
        Assert.Equal("JPY", supportedCurrencies[1]);
        Assert.Equal("USD", supportedCurrencies[2]);
    }
}
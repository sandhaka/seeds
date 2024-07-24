using Monads.Option;
// ReSharper disable EqualExpressionComparison

namespace Tests.Option;

internal record Money(decimal Amount, string? Currency = null)
{
    public static Money Zero => new(0);
}

public class OptionTests
{
    [Fact]
    public void ShouldConvertAnObjectToAnOption()
    {
        var sampleTask = Task.Run(() => 1);
        var some = Option<Task>.Some(sampleTask);
        var none = Option<Task>.None();
        
        Assert.Equal(sampleTask, some.Reduce(Task.CompletedTask));
        Assert.NotEqual(sampleTask, none.Reduce(Task.CompletedTask));
        
        var dollars = new Money(100, "USD");
        var someDollars = Option<Money>.Some(dollars);
        
        Assert.NotEqual(Money.Zero, someDollars.Reduce(Money.Zero));
    }

    [Fact]
    public void ShouldMapReduce()
    {
        var dollars = new Money(100, "USD");
        var someDollars = Option<Money>.Some(dollars);
        var dollarCurrency = someDollars.Reduce(Money.Zero).Currency;
        
        Assert.Equal("USD", dollarCurrency);
        
        var noneDollars = Option<Money>.None();
        var noneCurrency = noneDollars.Reduce(Money.Zero).Currency;

        Assert.Null(noneCurrency);

        var innerCurrency = someDollars.Map(d => d.Currency);
        
        Assert.Equal(Option<string?>.Some("USD"), innerCurrency);
        Assert.Equal("USD", innerCurrency.Reduce(string.Empty));
        
        var noneDollarsCurrency = noneDollars.Map(d => d.Currency);
        
        Assert.NotNull(noneDollarsCurrency.Reduce(string.Empty));
        Assert.Equal(string.Empty, noneDollarsCurrency.Reduce(string.Empty));
        Assert.NotNull(noneDollarsCurrency.Reduce(() => string.Empty));
        Assert.Equal(string.Empty, noneDollarsCurrency.Reduce(() => string.Empty));
    }
    
    [Fact]
    public void ShouldWhereReturnSomeWhenPredicateIsTrue()
    {
        var dollars = new Money(100, "USD");
        var someDollars = Option<Money>.Some(dollars);
        var filteredDollars = someDollars.Where(d => d.Amount > 50);

        Assert.Equal(someDollars, filteredDollars);
    }
    
    [Fact]
    public void ShouldWhereReturnNoneWhenPredicateIsFalse()
    {
        var dollars = new Money(100, "USD");
        var someDollars = Option<Money>.Some(dollars);
        var filteredDollars = someDollars.Where(d => d.Amount > 150);

        Assert.Equal(Option<Money>.None(), filteredDollars);
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
}
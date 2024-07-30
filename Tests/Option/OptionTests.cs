using System.Globalization;
using Monads.Optional;
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

    [Fact]
    public void ShouldUseInLogic()
    {
        Assert.Equal(Option<string>.Some("United state"), GetCurrencyCountry(new Money(100, "USD")));
        Assert.Equal(Option<string>.Some("Japan"), GetCurrencyCountry(new Money(100, "JPY")));
        Assert.Equal(Option<string>.None(), GetCurrencyCountry(Money.NoValue));
        Assert.Equal(Option<string>.Some("United state"), GetCurrencyCountry(Money.Zero("USD")));

        Assert.Equal(Option<string>.Some("ML"), GetInitialsFromFullName("Marco Lincoln"));
        Assert.Equal(Option<string>.Some("MA"), GetInitialsFromFullName("MarcoLincoln"));
        Assert.Equal(Option<string>.None(), GetInitialsFromFullName("Q"));
        Assert.Equal(Option<string>.None(), GetInitialsFromFullName("        "));
        
        Assert.Equal("100.00 USD", DescribeMoney(ValueOption<Money>.Some(new Money(100, "USD"))));
        Assert.Equal("0.00 ", DescribeMoney(ValueOption<Money>.Some(Money.NoValue)));
        Assert.Equal("0.00 USD", DescribeMoney(ValueOption<Money>.Some(Money.Zero("USD"))));
        return;

        Option<string> GetCurrencyCountry(Money money) =>
            money.Currency.Reduce(string.Empty) switch
            {
                "USD" => Option<string>.Some("United state"),
                "JPY" => Option<string>.Some("Japan"),
                "EUR" => Option<string>.Some("Europe"),
                _ => Option<string>.None(),
            };

        string ToUpperFirst(string s) => s.First().ToString().ToUpper();

        Option<string> GetInitialsFromFullName(string fullName) =>
            fullName switch
            {
                { Length: 1 } => Option<string>.None(),
                _ => fullName.Split(" ") switch
                {
                    [{ } first, { } second] => Option<string>.Some($"{ToUpperFirst(first)}{ToUpperFirst(second)}"),
                    [{ } whole] => Option<string>.Some(whole[0..2].ToUpper()),
                    [] => Option<string>.None(),
                    _ => Option<string>.None()
                }
            };

        string DescribeMoney(ValueOption<Money> money) =>
            money.Map(m => $"{m.Amount.ToString("N2", new CultureInfo("US-us"))} {m.Currency}").Reduce(string.Empty);
    }
}
// ReSharper disable PossibleMultipleEnumeration

using CollectionsExtensions;
using Tests.Support;
using Xunit.Abstractions;

namespace Tests.CollectionsExtensions;

public class EnumerableExtensionsTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public EnumerableExtensionsTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void EnumerableOnceShouldEnumerateOnlyOnce()
    {
        var max = int.MinValue;

        IEnumerable<int> sequence = Enumerable.Range(1, 1024)
            .Once();

        foreach (var i in sequence)
            max = Math.Max(max, i);

        Assert.Throws<InvalidOperationException>(() =>
        {
            max = int.MinValue;
            
            foreach (var i in sequence)
                max = Math.Max(max, i);
        });
    }

    [Fact]
    public void DescribableShouldFormatAnObjectsSequence()
    {
        var accounts = new BankDeposit().MultiCurrencies;

        var describable = accounts
            .Select(m => m.Reduce(Money.NoValue))
            .ToDescribable();
        
        Assert.Equal(["AMOUNT", "CURRENCY"], describable.Header);

        var formatted = describable.Format();
        
        Assert.Equal("              AMOUNT|            CURRENCY", formatted.ElementAt(0));
        Assert.Equal("                   0|                 USD", formatted.ElementAt(1));
    }

    [Fact]
    public void DescribableShouldBeEnumerable()
    {
        var accounts = new BankDeposit().MultiCurrencies;

        var describable = accounts
            .Select(m => m.Reduce(Money.NoValue))
            .ToDescribable();

        var descriptions = describable.ToList();
        
        Assert.NotEmpty(descriptions);
        Assert.Contains(descriptions, desc => desc.Currency.Equals("USD"));
    }
}
// ReSharper disable PossibleMultipleEnumeration

using CollectionsExtensions;
using Tests.Monads.Optional.Support;
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
    public void ShouldEnumerateOnlyOnce()
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
    public void ShouldFormatAnObjectsSequence()
    {
        var accounts = new BankDeposit().MultiCurrencies.Select(m => m.Reduce(Money.NoValue));
        
        var formatted = accounts.Format();
        
        foreach (var row in formatted)
        {
            _testOutputHelper.WriteLine(row);
        }
    }
}
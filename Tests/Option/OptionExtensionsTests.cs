using Monads.Option;
using Monads.Option.Extensions;
// ReSharper disable PossibleMultipleEnumeration

namespace Tests.Option;

public class OptionExtensionsTests
{
    [Fact]
    public void ShouldReturnSomeAsFirstElementOfSequences()
    {
        IEnumerable<Money> sequence = [ new Money(1, "USD"),  new Money(1000, "JPY")];
        
        var first = sequence.FirstOrNone();
        
        Assert.Equal(Option<Money>.Some(new Money(1, "USD")), first);

        var anotherSequence = sequence.Concat([new Money(1, "CHF"), new Money(2, "USD")]);
        
        var firstChf = anotherSequence.FirstOrNone(m => m.Currency == "CHF");
        Assert.Equal(Option<Money>.Some(new Money(1, "CHF")), firstChf);

        var firstEur = anotherSequence.FirstOrNone(m => m.Currency == "EUR");
        Assert.Equal(Option<Money>.None(), firstEur);
    }

    [Fact]
    public void ShouldReturnNoneAsFirstElementOfDictionary()
    {
        var dictionary = new Dictionary<string, Option<Money>>
        {
            { "USD", Option<Money>.None() },
            { "JPY", Option<Money>.Some(new Money(1000, "JPY")) }
        };

        dictionary.TryGetValue("USD", out var dollar);
        
        Assert.Equal(Option<Money>.None(), dollar);
        
        dictionary.TryGetValue("EUR", out var euro);
        Assert.Equal(Option<Money>.None(), euro);
        
        dictionary.TryGetValue("JPY", out var yen);
        Assert.Equal(Option<Money>.Some(new Money(1000, "JPY")), yen);
    }
}
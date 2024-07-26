using Monads.Option;
using Monads.Option.Extensions;
using Tests.Option.Support;

// ReSharper disable PossibleMultipleEnumeration

namespace Tests.Option;

public class OptionExtensionsTests
{
    [Fact]
    public void ShouldReturnOptionAsElementOfSequences()
    {
        IEnumerable<Money> sequence = [ new Money(1, "USD"),  new Money(1000, "JPY") ];
        
        var first = sequence.FirstValueOrNone();
        Assert.Equal(ValueOption<Money>.Some(new Money(1, "USD")), first);

        var anotherSequence = sequence.Concat([new Money(1, "CHF"), new Money(2, "USD")]);
        
        var firstChf = anotherSequence.FirstValueOrNone(m => m.Currency == "CHF");
        Assert.Equal(ValueOption<Money>.Some(new Money(1, "CHF")), firstChf);

        var firstEur = anotherSequence.FirstValueOrNone(m => m.Currency == "EUR");
        Assert.Equal(ValueOption<Money>.None(), firstEur);
    }

    [Fact]
    public void ShouldReturnOptionAsElementOfDictionary()
    {
        var dictionary = new Dictionary<string, Money>
        {
            { "USD", Money.Zero("USD") },
            { "JPY", new Money(1000, "JPY") }
        };

        var dollar = dictionary.TryGetOptionValue("USD");
        Assert.Equal(ValueOption<Money>.Some(Money.Zero("USD")), dollar);
        
        var euro = dictionary.TryGetOptionValue("EUR");
        Assert.Equal(ValueOption<Money>.None(), euro);
        
        var yen = dictionary.TryGetOptionValue("JPY");
        Assert.Equal(ValueOption<Money>.Some(new Money(1000, "JPY")), yen);
    }
}
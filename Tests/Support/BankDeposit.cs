using Monads.Optional;

namespace Tests.Support;

internal sealed class BankDeposit
{
    public ValueOption<Money> Dollars { get; set; } = ValueOption<Money>.Some(new Money(decimal.Zero, "USD"));
    public ValueOption<Money> Euros { get; set; } = ValueOption<Money>.Some(new Money(decimal.Zero, "EUR"));
    public ValueOption<Money> Yens { get; set; } = ValueOption<Money>.Some(new Money(decimal.Zero, "JPY"));
    
    public IReadOnlyCollection<ValueOption<Money>> MultiCurrencies => [ Dollars, Euros, Yens ];
}
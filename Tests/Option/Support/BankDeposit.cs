namespace Tests.Option.Support;

internal sealed class BankDeposit
{
    public Money Dollars { get; set; } = new(decimal.Zero, "USD");
    public Money Euros { get; set; } = new(decimal.Zero, "EUR");
    public Money Yens { get; set; } = new(decimal.Zero, "JPY");
    
    public IReadOnlyCollection<Money> MultiCurrencies => new[] { Dollars, Euros, Yens };
}
namespace Tests.Support;

public record struct Money()
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount) : this()
    {
        Amount = amount;
        Currency = string.Empty;
    }
    
    public Money(decimal amount, string currency) : this()
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money NoValue => new(0m);
    
    public static Money Zero(string currency) => new(decimal.Zero, currency);
    
    public static Money operator +(Money a, Money b)
    {
        if (a.Currency != b.Currency) throw new InvalidOperationException();
        return new Money(a.Amount + b.Amount, a.Currency);
    }
    
    public static Money operator -(Money a, Money b)
    {
        if (a.Currency != b.Currency) throw new InvalidOperationException();
        return new Money(a.Amount - b.Amount, a.Currency);
    }
}
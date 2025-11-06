namespace PaymentService.Domain.Aggregates.PaymentAggregate.ValueObjects;

public record Currency
{
    public string Code { get; private set; }

    public static readonly Currency USD = new("USD");
    public static readonly Currency VND = new("VND");

    private static readonly HashSet<string> ValidCodes = new() { "USD", "VND" };

    private Currency(string code)
    {
        Code = code;
    }

    public static Currency Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Currency code cannot be empty", nameof(code));

        var upperCode = code.ToUpperInvariant();
        if (!ValidCodes.Contains(upperCode))
            throw new ArgumentException($"Invalid currency: {code}", nameof(code));

        return new Currency(upperCode);
    }

    public static implicit operator string(Currency currency) => currency.Code;

}
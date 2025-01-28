namespace BookingService.Application.Models.ObjectValues;

public readonly record struct BookingId
{
    public long Value { get; private init; }

    public BookingId(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);

        Value = value;
    }

    public static readonly BookingId Default = new(0);
}
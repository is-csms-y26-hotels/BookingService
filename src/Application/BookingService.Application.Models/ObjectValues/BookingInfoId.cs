namespace BookingService.Application.Models.ObjectValues;

public readonly record struct BookingInfoId
{
    public long Value { get; private init; }

    public BookingInfoId(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);

        Value = value;
    }

    public static readonly BookingInfoId Default = new(0);
}
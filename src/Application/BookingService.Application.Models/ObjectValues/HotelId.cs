namespace BookingService.Application.Models.ObjectValues;

public readonly record struct HotelId
{
    public long Value { get; private init; }

    public HotelId(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);

        Value = value;
    }

    public static readonly HotelId Default = new(0);
}
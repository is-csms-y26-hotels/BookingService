namespace BookingService.Application.Models.ObjectValues;

public readonly record struct RoomId
{
    public long Value { get; private init; }

    public RoomId(long value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);

        Value = value;
    }

    public static readonly RoomId Default = new(0);
}
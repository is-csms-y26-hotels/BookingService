using BookingService.Application.Models.ObjectValues;

namespace BookingService.Application.Contracts.Bookings.Operations;

public static class GetRoomAvailableDateRanges
{
    public readonly record struct Request(RoomId RoomId, DateTimeOffset StartDate, DateTimeOffset EndDate);

    public abstract record Result
    {
        private Result() { }

        public sealed record Success(IReadOnlyCollection<(DateTimeOffset Start, DateTimeOffset End)> Ranges) : Result;

        public sealed record RoomNotFound : Result;
    }
}
using BookingService.Application.Models.Enums;
using BookingService.Application.Models.ObjectValues;

namespace BookingService.Application.Contracts.Bookings.Operations;

public static class PostponeBooking
{
    public readonly record struct Request(
        BookingId BookingId,
        DateTimeOffset NewCheckInDate,
        DateTimeOffset NewCheckOutDate);

    public abstract record Result
    {
        private Result() { }

        public sealed record Updated(
            BookingId BookingId,
            DateTimeOffset BookingCheckInDate,
            DateTimeOffset BookingCheckOutDate) : Result;

        public sealed record RoomNotFound : Result;

        public sealed record RoomNotAvailable : Result;

        public sealed record InvalidBookingState(BookingState BookingState) : Result;
    }
}
using BookingService.Application.Models.Enums;
using BookingService.Application.Models.ObjectValues;

namespace BookingService.Application.Contracts.Bookings.Operations;

public static class SubmitBooking
{
    public readonly record struct Request(BookingId BookingId);

    public abstract record Result
    {
        private Result() { }

        public sealed record Submitted(BookingId BookingId) : Result;

        public sealed record RoomNotFound : Result;

        public sealed record InvalidBookingState(BookingState BookingState) : Result;
    }
}
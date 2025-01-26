using BookingService.Application.Models.Enums;
using BookingService.Application.Models.ObjectValues;

namespace BookingService.Application.Contracts.Bookings.Operations;

public static class CreateBooking
{
    public readonly record struct Request(
        UserEmail UserEmail,
        HotelId HotelId,
        RoomId RoomId,
        DateTimeOffset CheckInDate,
        DateTimeOffset CheckOutDate);

    public abstract record Result
    {
        private Result() { }

        public sealed record Created(
            BookingId BookingId,
            BookingState BookingState,
            HotelId BookingHotelId,
            RoomId BookingRoomId,
            UserEmail BookingUserEmail,
            DateTimeOffset BookingCheckInDate,
            DateTimeOffset BookingCheckOutDate,
            DateTimeOffset BokingCreatedAt) : Result;

        public sealed record RoomNotFound : Result;

        public sealed record RoomNotAvailable : Result;
    }
}
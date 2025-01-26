using BookingService.Application.Models.Enums;
using BookingService.Application.Models.ObjectValues;
using System;

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
            BookingState State,
            HotelId HotelId,
            RoomId RoomId,
            UserEmail UserEmail,
            DateTimeOffset CheckInDate,
            DateTimeOffset CheckOutDate,
            DateTimeOffset CreatedAt) : Result;

        public sealed record RoomNotFound : Result;

        public sealed record RoomNotAvailable : Result;
    }
}
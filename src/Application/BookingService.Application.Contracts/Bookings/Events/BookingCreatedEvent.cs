using BookingService.Application.Models.ObjectValues;
using Itmo.Dev.Platform.Events;

namespace BookingService.Application.Contracts.Bookings.Events;

public record BookingCreatedEvent(
    BookingId BookingId,
    RoomId BookingRoomId,
    UserEmail BookingUserEmail,
    DateTimeOffset BookingCheckInDate,
    DateTimeOffset BookingCheckOutDate,
    DateTimeOffset BookingCreatedAt) : IEvent;
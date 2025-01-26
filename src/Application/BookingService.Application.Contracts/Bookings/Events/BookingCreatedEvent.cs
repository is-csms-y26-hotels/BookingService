using BookingService.Application.Models.Enums;
using BookingService.Application.Models.ObjectValues;
using Itmo.Dev.Platform.Events;

namespace BookingService.Application.Contracts.Bookings.Events;

public record BookingCreatedEvent(
    BookingId BookingId,
    BookingState BookingState,
    RoomId BookingRoomId,
    UserEmail BookingUserEmail,
    DateTimeOffset BookingCheckInDate,
    DateTimeOffset BookingCheckOutDate) : IEvent;
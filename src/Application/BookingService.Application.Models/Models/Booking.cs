using BookingService.Application.Models.Enums;
using BookingService.Application.Models.ObjectValues;

namespace BookingService.Application.Models.Models;

public record Booking(
    BookingId BookingId,
    BookingState BookingState,
    BookingInfo BookingInfo,
    DateTimeOffset BookingCreatedAt);
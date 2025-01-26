using BookingService.Application.Models.Enums;
using BookingService.Application.Models.ObjectValues;

namespace BookingService.Application.Models.Models;

public record Booking(
    BookingId Id,
    BookingState State,
    BookingInfoId InfoId,
    DateTimeOffset CreatedAt);
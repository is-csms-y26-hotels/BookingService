using BookingService.Application.Models.Enums;
using BookingService.Application.Models.ObjectValues;

namespace BookingService.Application.Models.Models;

public record Booking(
    BookingId Id,
    BookingStatus Status,
    BookingInfo Info,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ModifiedAt,
    DateTimeOffset? DeletedAt,
    bool IsDeleted);
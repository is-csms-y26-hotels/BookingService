using BookingService.Application.Models.ObjectValues;

namespace BookingService.Application.Models.Models;

public record BookingInfo(
    BookingInfoId Id,
    HotelId HotelId,
    RoomId RoomId,
    UserEmail UserEmail,
    DateTimeOffset CheckInDate,
    DateTimeOffset CheckOutDate);
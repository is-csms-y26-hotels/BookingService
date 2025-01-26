using BookingService.Application.Models.ObjectValues;

namespace BookingService.Application.Models.Models;

public record BookingInfo(
    BookingInfoId BookingInfoId,
    HotelId BookingInfoHotelId,
    RoomId BookingInfoRoomId,
    UserEmail BookingInfoUserEmail,
    DateTimeOffset BookingInfoCheckInDate,
    DateTimeOffset BookingInfoCheckOutDate);
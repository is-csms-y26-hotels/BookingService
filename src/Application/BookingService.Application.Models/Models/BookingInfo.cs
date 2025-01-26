using BookingService.Application.Models.ObjectValues;

namespace BookingService.Application.Models.Models;

public record BookingInfo(
    BookingInfoId BookingInfoId,
    RoomId BookingInfoRoomId,
    UserEmail BookingInfoUserEmail,
    DateTimeOffset BookingInfoCheckInDate,
    DateTimeOffset BookingInfoCheckOutDate);
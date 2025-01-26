using BookingService.Application.Models.Enums;

namespace BookingService.Application.Exceptions;

public class InvalidBookingStateException(BookingState bookingState) : Exception()
{
    public BookingState BookingState { get; } = bookingState;
}
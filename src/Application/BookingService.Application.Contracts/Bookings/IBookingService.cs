using BookingService.Application.Contracts.Bookings.Operations;

namespace BookingService.Application.Contracts.Bookings;

public interface IBookingService
{
    public Task<CreateBooking.Result> CreateBookingAsync(
        CreateBooking.Request request,
        CancellationToken cancellationToken);
}
using BookingService.Application.Abstractions.Persistence.Queries;
using BookingService.Application.Models.Enums;
using BookingService.Application.Models.Models;
using BookingService.Application.Models.ObjectValues;

namespace BookingService.Application.Abstractions.Persistence.Repositories;

public interface IBookingRepository
{
    public Task<Booking> AddAsync(Booking booking, CancellationToken cancellationToken);

    public Task<Booking> UpdateStateAsync(
        BookingId bookingId,
        BookingState bookingState,
        CancellationToken cancellationToken);

    public IAsyncEnumerable<Booking> QueryAsync(BookingQuery query, CancellationToken cancellationToken);
}
using BookingService.Application.Abstractions.Persistence.Queries;
using BookingService.Application.Models.Models;

namespace BookingService.Application.Abstractions.Persistence.Repositories;

public interface IBookingRepository
{
    public Task<Booking> AddAsync(Booking booking, CancellationToken cancellationToken);

    public Task<Booking> UpdateAsync(Booking booking, CancellationToken cancellationToken);

    public IAsyncEnumerable<Booking> QueryAsync(BookingQuery query, CancellationToken cancellationToken);
}
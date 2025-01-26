using BookingService.Application.Abstractions.Persistence.Queries;
using BookingService.Application.Models.Models;

namespace BookingService.Application.Abstractions.Persistence.Repositories;

public interface IBookingInfoRepository
{
    public Task<BookingInfo> AddAsync(BookingInfo bookingInfo, CancellationToken cancellationToken);

    public IAsyncEnumerable<BookingInfo> QueryAsync(BookingInfoQuery query, CancellationToken cancellationToken);
}
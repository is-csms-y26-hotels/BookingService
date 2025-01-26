using BookingService.Application.Abstractions.Persistence.Repositories;

namespace BookingService.Application.Abstractions.Persistence;

public interface IPersistenceContext
{
    IBookingRepository Bookings { get; }

    IBookingInfoRepository BookingInfos { get; }
}
using BookingService.Application.Abstractions.Persistence;
using BookingService.Application.Abstractions.Persistence.Repositories;

namespace BookingService.Infrastructure.Persistence;

public class PersistenceContext(
    IBookingRepository bookingRepository,
    IBookingInfoRepository bookingInfoRepository) : IPersistenceContext
{
    public IBookingRepository Bookings => bookingRepository;

    public IBookingInfoRepository BookingInfos => bookingInfoRepository;
}
using BookingService.Application.Abstractions.Persistence;
using BookingService.Application.Abstractions.Persistence.Repositories;

namespace BookingService.Infrastructure.Persistence;

public class PersistenceContext(IBookingRepository bookingRepository) : IPersistenceContext
{
    public IBookingRepository Bookings => bookingRepository;
}
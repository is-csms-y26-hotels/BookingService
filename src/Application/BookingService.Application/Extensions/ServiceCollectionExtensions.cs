using BookingService.Application.Bookings;
using BookingService.Application.Contracts.Bookings;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection collection)
    {
        collection.AddScoped<IReservationService, ReservationService>();

        return collection;
    }
}
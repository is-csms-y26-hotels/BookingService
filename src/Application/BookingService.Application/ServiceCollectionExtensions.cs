using BookingService.Application.Contracts.Bookings;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection collection)
    {
        collection.AddScoped<IBookingService, Bookings.BookingService>();

        return collection;
    }
}
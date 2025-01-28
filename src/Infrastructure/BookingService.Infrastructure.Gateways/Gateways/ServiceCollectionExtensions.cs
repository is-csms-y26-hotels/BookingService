using BookingService.Application.Abstractions.Gateways;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Infrastructure.Gateways.Gateways;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccommodationGateway(this IServiceCollection collection)
    {
        collection.AddScoped<IAccommodationGateway, AccommodationGateway>();

        return collection;
    }
}
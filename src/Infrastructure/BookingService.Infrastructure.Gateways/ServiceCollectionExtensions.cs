using BookingService.Application.Abstractions.Gateways;
using BookingService.Infrastructure.Gateways.Gateways;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Infrastructure.Gateways;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureGateways(this IServiceCollection collection)
    {
        // TODO add grpc client
        collection.AddScoped<IAccommodationGateway, AccommodationGateway>();

        return collection;
    }
}
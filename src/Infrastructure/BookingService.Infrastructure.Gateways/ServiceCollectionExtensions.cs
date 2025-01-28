using BookingService.Infrastructure.Gateways.Gateways;
using Itmo.Dev.Platform.Grpc.Clients;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Infrastructure.Gateways;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureGateways(this IServiceCollection collection)
    {
        collection.AddPlatformGrpcClients(clients => clients
            .AddAccommodationGatewayClient());

        collection.AddAccommodationGateway();

        return collection;
    }
}
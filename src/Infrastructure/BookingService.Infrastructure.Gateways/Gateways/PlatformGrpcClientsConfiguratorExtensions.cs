using Accommodation.Service.Presentation.Grpc;
using Itmo.Dev.Platform.Grpc.Clients;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Infrastructure.Gateways.Gateways;

public static class PlatformGrpcClientsConfiguratorExtensions
{
    public static IPlatformGrpcClientsConfigurator AddAccommodationGatewayClient(this IPlatformGrpcClientsConfigurator configurator)
    {
        configurator
            .AddService(service => service
                .Called("accommodation")
                .WithConfiguration(x => x.BindConfiguration("Infrastructure:External:AccommodationService"))
                .WithClient<RoomService.RoomServiceClient>());

        return configurator;
    }
}
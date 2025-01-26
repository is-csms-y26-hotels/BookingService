using Itmo.Dev.Platform.Grpc.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Presentation.Grpc.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationGrpc(this IServiceCollection collection)
    {
        collection.AddGrpc();
        collection.AddGrpcReflection();

        collection.AddPlatformGrpcServices(builder => builder);

        return collection;
    }
}
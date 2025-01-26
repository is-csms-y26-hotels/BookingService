using BookingService.Presentation.Grpc.Controllers;
using Microsoft.AspNetCore.Builder;

namespace BookingService.Presentation.Grpc.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UsePresentationGrpc(this IApplicationBuilder builder)
    {
        builder.UseEndpoints(routeBuilder =>
        {
            routeBuilder.MapGrpcService<BookingController>();
            routeBuilder.MapGrpcReflectionService();
        });

        return builder;
    }
}
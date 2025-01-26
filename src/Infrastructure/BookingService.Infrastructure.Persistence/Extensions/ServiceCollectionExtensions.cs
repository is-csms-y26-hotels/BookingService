using BookingService.Application.Abstractions.Persistence;
using BookingService.Application.Abstractions.Persistence.Repositories;
using BookingService.Infrastructure.Persistence.Plugins;
using BookingService.Infrastructure.Persistence.Repositories;
using Itmo.Dev.Platform.Persistence.Abstractions.Extensions;
using Itmo.Dev.Platform.Persistence.Postgres.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Infrastructure.Persistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructurePersistence(this IServiceCollection collection)
    {
        collection.AddPlatformPersistence(persistence => persistence
            .UsePostgres(postgres => postgres
                .WithConnectionOptions(b => b.BindConfiguration("Infrastructure:Persistence:Postgres"))
                .WithMigrationsFrom(typeof(IAssemblyMarker).Assembly)
                .WithDataSourcePlugin<MappingPlugin>()));

        collection.AddScoped<IBookingRepository, BookingRepository>();
        collection.AddScoped<IBookingInfoRepository, BookingInfoRepository>();

        collection.AddScoped<IPersistenceContext, PersistenceContext>();

        return collection;
    }
}
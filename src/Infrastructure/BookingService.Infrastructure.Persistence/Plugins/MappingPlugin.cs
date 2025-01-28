using BookingService.Application.Models.Enums;
using Itmo.Dev.Platform.Persistence.Postgres.Plugins;
using Npgsql;

namespace BookingService.Infrastructure.Persistence.Plugins;

public class MappingPlugin : IPostgresDataSourcePlugin
{
    public void Configure(NpgsqlDataSourceBuilder dataSource)
    {
        dataSource.MapEnum<BookingState>("booking_state");
    }
}
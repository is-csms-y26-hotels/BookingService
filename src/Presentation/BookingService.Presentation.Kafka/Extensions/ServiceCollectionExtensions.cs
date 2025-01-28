using Bookings.Kafka.Contracts;
using Itmo.Dev.Platform.Kafka.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookingService.Presentation.Kafka.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationKafka(
        this IServiceCollection collection,
        IConfiguration configuration)
    {
        const string producerKey = "Presentation:Kafka:Producers";

        collection.AddPlatformKafka(builder => builder
                .ConfigureOptions(configuration.GetSection("Presentation:Kafka"))
                .AddProducer(b => b
                    .WithKey<BookingKey>()
                    .WithValue<BookingValue>()
                    .WithConfiguration(configuration.GetSection($"{producerKey}:Bookings"))
                    .SerializeKeyWithProto()
                    .SerializeValueWithProto()
                    .WithOutbox()));

        return collection;
    }
}
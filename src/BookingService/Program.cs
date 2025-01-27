#pragma warning disable CA1506

using BookingService.Application.Extensions;
using BookingService.Infrastructure.Gateways;
using BookingService.Infrastructure.Persistence.Extensions;
using BookingService.Presentation.Grpc.Extensions;
using BookingService.Presentation.Kafka.Extensions;
using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Events;
using Itmo.Dev.Platform.MessagePersistence.Extensions;
using Itmo.Dev.Platform.MessagePersistence.Postgres.Extensions;
using Itmo.Dev.Platform.Observability;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddPlatform();
builder.AddPlatformObservability();
builder.Services.AddUtcDateTimeProvider();

// Null value ignore is needed to correctly deserialize oneof messages in inbox/outbox
builder.Services.AddOptions<JsonSerializerSettings>()
    .Configure(options => options.NullValueHandling = NullValueHandling.Ignore);

builder.Services.AddSingleton<JsonSerializerSettings>(
    provider => provider.GetRequiredService<IOptions<JsonSerializerSettings>>().Value);

// Used as inbox and outbox infrastructure
builder.Services.AddPlatformMessagePersistence(selector => selector
    .UsePostgresPersistence(postgres => postgres
        .ConfigureOptions(optionsBuilder => optionsBuilder
            .BindConfiguration("Infrastructure:MessagePersistence:Persistence"))));

builder.Services.AddApplication();
builder.Services.AddInfrastructurePersistence();
builder.Services.AddInfrastructureGateways();
builder.Services.AddPresentationGrpc();
builder.Services.AddPresentationKafka(builder.Configuration);

builder.Services.AddPlatformEvents(b => b.AddPresentationKafkaHandlers());

builder.Services.AddGrpcSwagger();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc(
        "v1",
        new OpenApiInfo { Title = "Hotels BookingService API", Version = "v1" }));

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint(
    "/swagger/v1/swagger.json",
    "Hotels BookingService API v1"));

app.UseRouting();

app.UsePlatformObservability();

app.UsePresentationGrpc();

await app.RunAsync();
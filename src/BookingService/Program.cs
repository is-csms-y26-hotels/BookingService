#pragma warning disable CA1506

using BookingService.Application.Extensions;
using BookingService.Infrastructure.Gateways;
using BookingService.Infrastructure.Persistence.Extensions;
using BookingService.Presentation.Grpc.Extensions;
using BookingService.Presentation.Kafka.Extensions;
using Itmo.Dev.Platform.Common.Extensions;
using Itmo.Dev.Platform.Events;
using Itmo.Dev.Platform.Observability;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddOptions<JsonSerializerSettings>();
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<JsonSerializerSettings>>().Value);

builder.Services.AddPlatform();
builder.AddPlatformObservability();

builder.Services.AddApplication();
builder.Services.AddInfrastructurePersistence();
builder.Services.AddInfrastructureGateways();
builder.Services.AddPresentationGrpc();
builder.Services.AddPresentationKafka(builder.Configuration);

/*builder.Services.AddPlatformBackgroundTasks(configurator => configurator
    .UsePostgresPersistence(postgres => postgres.BindConfiguration("Infrastructure:BackgroundTasks:Persistence"))
    .ConfigureScheduling(scheduling => scheduling.BindConfiguration("Infrastructure:BackgroundTasks:Scheduling"))
    .UseHangfireScheduling(hangfire => hangfire
        .ConfigureOptions(o => o.BindConfiguration("Infrastructure:BackgroundTasks:Scheduling:Hangfire"))
        .UsePostgresJobStorage())
    .ConfigureExecution(builder.Configuration.GetSection("Infrastructure:BackgroundTasks:Execution"))
    .AddApplicationBackgroundTasks());*/

builder.Services.AddPlatformEvents(b => b.AddPresentationKafkaHandlers());

builder.Services.AddUtcDateTimeProvider();

WebApplication app = builder.Build();

app.UseRouting();

app.UsePlatformObservability();

app.UsePresentationGrpc();

await app.RunAsync();
using System.Text.Json;
using JasperFx;
using JasperFx.Core;
using JasperFx.Events;
using JasperFx.Events.Daemon;
using JasperFx.Resources;
using Marten;
using Microsoft.AspNetCore.Mvc;
using Monolith.Host.Example.Collaboration;
using Qowaiv.Text.Json.Serialization;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.Kafka;
using Wolverine.Marten;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.SerializerOptions.Converters.Add(new QowaivJsonConverter());
    });

builder.Services.AddWolverine(wolverine =>
{
    builder.Services.CritterStackDefaults(_ => { });
    wolverine.Services.AddMarten(marten =>
        {
            marten.Connection("User ID=username;Password=password;Host=localhost;Port=5432;Database=monolith;");
            marten.Events.StreamIdentity = StreamIdentity.AsString;
            
            marten.UseSystemTextJsonForSerialization();
        })
        .AddAsyncDaemon(DaemonMode.HotCold)
        .UseLightweightSessions()
        .IntegrateWithWolverine()
        .PublishEventsToWolverine("all");

    wolverine.UseSystemTextJsonForSerialization();
    
    wolverine.Policies.AutoApplyTransactions();

    wolverine.Durability.Mode = builder.Environment.IsDevelopment()
        ? DurabilityMode.Solo
        : DurabilityMode.Balanced;
    
    wolverine.Policies.OnAnyException()
        .RetryWithCooldown(50.Milliseconds(), 100.Milliseconds(), 250.Milliseconds());

    wolverine.Policies.UseDurableInboxOnAllListeners();
    wolverine.Policies.UseDurableOutboxOnAllSendingEndpoints();

    wolverine.Policies.AutoApplyTransactions();

    wolverine.UseKafka("localhost:9092")
        .AutoProvision(provision =>
        {
            provision.AllowAutoCreateTopics = true;
        })
        .ConfigureProducers(producers =>
        {
            producers.BatchSize = 100;
            producers.EnableGaplessGuarantee = true;
            producers.EnableIdempotence = true;
        });

    wolverine.Services.AddResourceSetupOnStartup();
    wolverine.Services.AddWolverineExtension<CollaborationExtension>();
});

builder.Host.ApplyJasperFxExtensions();

var app = builder.Build();

app.MapPost("room/open", ([FromBody] Commands.OpenRoom command, [FromServices] IMessageBus bus) => bus.InvokeAsync(command));
app.MapPost("room/close", ([FromBody] Commands.CloseRoom command, [FromServices] IMessageBus bus) => bus.InvokeAsync(command));

await app.RunJasperFxCommands(args);

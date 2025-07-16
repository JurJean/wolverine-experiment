using System.Text.Json;
using Example.Collaboration;
using JasperFx.CodeGeneration;
using JasperFx.Core;
using Marten;
using Marten.Events;
using Microsoft.AspNetCore.Mvc;
using Oakton;
using Oakton.Resources;
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
    wolverine.Services.AddMarten(marten =>
        {
            marten.Connection("User ID=username;Password=password;Host=localhost;Port=5432;Database=monolith;");
            marten.SourceCodeWritingEnabled = false;
            marten.GeneratedCodeMode = builder.Environment.IsDevelopment()
                ? TypeLoadMode.Auto
                : TypeLoadMode.Static;
            marten.Events.StreamIdentity = StreamIdentity.AsString;
            
            marten.UseSystemTextJsonForSerialization();
        })
        .UseLightweightSessions()
        .IntegrateWithWolverine(integrate =>
        {
            integrate.UseFastEventForwarding = true;
            integrate.UseWolverineManagedEventSubscriptionDistribution = true;
        });
    
    wolverine.UseSystemTextJsonForSerialization();
    
    wolverine.Policies.AutoApplyTransactions();

    wolverine.CodeGeneration.SourceCodeWritingEnabled = false;
    wolverine.CodeGeneration.TypeLoadMode = builder.Environment.IsDevelopment()
        ? TypeLoadMode.Dynamic
        : TypeLoadMode.Static;

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
        .ConfigureClient(client =>
        {
            client.AllowAutoCreateTopics = true;
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

builder.Host.ApplyOaktonExtensions();

var app = builder.Build();

app.MapPost("room/test", ([FromServices] IMessageBus bus) => Console.WriteLine("FOO"));
app.MapPost("room/open", ([FromBody] Commands.OpenRoom command, [FromServices] IMessageBus bus) => bus.InvokeAsync(command));
app.MapPost("room/close", ([FromBody] Commands.CloseRoom command, [FromServices] IMessageBus bus) => bus.InvokeAsync(command));

await app.RunOaktonCommands(args);

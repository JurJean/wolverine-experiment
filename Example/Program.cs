using System;
using System.Text.Json;
using Example;
using JasperFx;
using JasperFx.Core;
using Marten;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using Weasel.Core;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.Marten;
using Wolverine.Runtime;
using Events = Example.Events;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ApplyJasperFxExtensions();

builder.Services.AddOpenApi();

builder.Services
    .ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddMarten(marten =>
    {
        marten.Connection(builder.Configuration.GetConnectionString("postgres")
                          ?? throw new InvalidOperationException("ConnectionString 'postgres' is not configured"));

        marten.ApplyChangesLockId = 4004;

        marten.UseSystemTextJsonForSerialization(EnumStorage.AsString, Casing.CamelCase);
    })
    .UseLightweightSessions()
    .ApplyAllDatabaseChangesOnStartup()
    .IntegrateWithWolverine(integrate => { integrate.UseFastEventForwarding = true; });
builder.Services.AddWolverine(wolverine =>
{
    builder.Services.CritterStackDefaults(_ => { });

    wolverine.UseRuntimeCompilation();
    wolverine.UseSystemTextJsonForSerialization();

    wolverine.Durability.Mode = builder.Environment.IsDevelopment()
        ? DurabilityMode.Solo
        : DurabilityMode.Balanced;

    wolverine.Policies.OnAnyException()
        .RetryWithCooldown(50.Milliseconds(), 100.Milliseconds(), 250.Milliseconds());

    wolverine.Policies.UseDurableLocalQueues();
    wolverine.Policies.UseDurableInboxOnAllListeners();
    wolverine.Policies.UseDurableOutboxOnAllSendingEndpoints();

    wolverine.Policies.AutoApplyTransactions();
});

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapPost("schedule", ([FromBody] Commands.ScheduleSomething command, [FromServices] IMessageContext context) =>
context.ScheduleAsync(
    command,
    TimeSpan.FromSeconds(1)));

app.MapPost("route", ([FromServices] IWolverineRuntime runtime) =>
{
    var router = runtime.RoutingFor(typeof(Events.SomethingWasScheduled));
    
    foreach (var messageRoute in router.Routes)
    {
        Console.WriteLine(messageRoute);
    }
});

await app.RunJasperFxCommands(args);
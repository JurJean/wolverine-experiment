using Example;
using JasperFx;
using JasperFx.CodeGeneration;
using JasperFx.Core;
using Marten;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;
using System;
using Weasel.Core;
using Wolverine;
using Wolverine.ErrorHandling;
using Wolverine.Marten;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ApplyJasperFxExtensions();

builder.Services.AddOpenApi();

builder.Services.AddMarten(marten =>
    {
        marten.Connection(builder.Configuration.GetConnectionString("postgres")
                          ?? throw new InvalidOperationException("ConnectionString 'postgres' is not configured"));

        marten.UseSystemTextJsonForSerialization(EnumStorage.AsString, Casing.CamelCase);
    })
    .ApplyAllDatabaseChangesOnStartup()
    .IntegrateWithWolverine(integrate => { integrate.UseFastEventForwarding = true; });

builder.Services.AddWolverine(wolverine =>
{
    wolverine.UseStaticRegistries();
    wolverine.UseSystemTextJsonForSerialization();

    wolverine.CodeGeneration.TypeLoadMode = TypeLoadMode.Static;

    wolverine.Policies.OnAnyException()
        .RetryWithCooldown(50.Milliseconds(), 100.Milliseconds(), 250.Milliseconds());

    wolverine.Policies.UseDurableInboxOnAllListeners();
    wolverine.Policies.UseDurableOutboxOnAllSendingEndpoints();

    wolverine.Policies.UseDurableLocalQueues();
    wolverine.Policies.AllLocalQueues(queue => { queue.UseDurableInbox(); });
    wolverine.Policies.AlwaysMakeScheduledMessagesDurable();

    wolverine.Policies.AutoApplyTransactions();
});

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapPost("schedule", ([FromBody] Commands.ScheduleSomething command, [FromServices] IMessageBus context) =>
context.PublishAsync(command));

return await app.RunJasperFxCommands(args);
using JasperFx;
using JasperFx.CodeGeneration;
using Marten;
using Oakton;
using Wolverine;
using Wolverine.Marten;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWolverine(wolverine =>
{
    wolverine.Services.AddMarten(marten =>
        {
            marten.Connection("User ID=username;Password=password;Host=localhost;Port=5432;Database=monolith;");
            marten.SourceCodeWritingEnabled = false;
            marten.GeneratedCodeMode = builder.Environment.IsDevelopment()
                ? TypeLoadMode.Dynamic
                : TypeLoadMode.Static;
        })

        .IntegrateWithWolverine(integrate =>
        {
            integrate.UseWolverineManagedEventSubscriptionDistribution = true;
        });

    wolverine.Policies.AutoApplyTransactions();
    
    wolverine.CodeGeneration.TypeLoadMode = builder.Environment.IsDevelopment()
        ? TypeLoadMode.Dynamic
        : TypeLoadMode.Static;

    wolverine.Durability.Mode = builder.Environment.IsDevelopment()
        ? DurabilityMode.Solo
        : DurabilityMode.Balanced;
});

builder.Host.ApplyOaktonExtensions();

var app = builder.Build();

await app.RunOaktonCommands(args);

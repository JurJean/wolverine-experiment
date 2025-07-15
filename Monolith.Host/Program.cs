using JasperFx;
using Marten;
using Wolverine;
using Wolverine.Marten;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWolverine(wolverine =>
{
    wolverine.Services.AddMarten(marten =>
        {
            marten.Connection("User ID=username;Password=password;Host=localhost;Port=5432;Database=monolith;");
        })

        .IntegrateWithWolverine(integrate =>
        {
            integrate.UseWolverineManagedEventSubscriptionDistribution = true;
        });

    wolverine.Policies.AutoApplyTransactions();

    if (builder.Environment.IsDevelopment())
    {
        wolverine.Durability.Mode = DurabilityMode.Solo;
    }
});

builder.Host.ApplyJasperFxExtensions();

var app = builder.Build();

await app.RunJasperFxCommands(args);
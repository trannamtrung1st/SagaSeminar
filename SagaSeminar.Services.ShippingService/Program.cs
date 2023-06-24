using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SagaSeminar.Services.ShippingService.Entities;
using SagaSeminar.Services.ShippingService.Services;
using SagaSeminar.Services.ShippingService.Services.Interfaces;
using SagaSeminar.Shared;
using SagaSeminar.Shared.Extensions;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Service.Extensions;
using SagaSeminar.Shared.Service.Kafka.Extensions;
using SagaSeminar.Shared.Service.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen()
    .AddSwaggerGenNewtonsoftSupport();
builder.Services.AddCorsDefaults();

builder.Services
    .AddDbContext<ShippingDbContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString(nameof(ShippingDbContext))))
    .AddUnitOfWork<ShippingDbContext>()
    .AddRepositories()
    .AddRedis()
    .AddKafka(builder.Configuration)
    .AddGlobalConfigReader()
    .AddLogClient("purple", nameof(SagaSeminar.Services.ShippingService))
    .AddServiceClients()
    .AddSingleton<IShippingPublisher, ShippingPublisher>()
    .AddScoped<IShippingService, ShippingService>();

builder.Services.ConfigureApiInfo(builder.Configuration);

builder.Services.AddSingleton<IShippingChoreographySaga, ShippingChoreographySaga>()
    .AddSingleton<IShippingOrchestratorSaga, ShippingOrchestratorSaga>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapGet("/api/deliveries", async (
    [FromQuery] int skip,
    [FromQuery] int take,
    [FromServices] IShippingService shippingService) =>
{
    ListResponseModel<DeliveryModel> response = await shippingService.GetDeliveries(skip, take);

    return Results.Ok(response);
})
.WithName("Get deliveries");

app.MapGetConfigurations();

using (IServiceScope scope = app.Services.CreateScope())
{
    IGlobalConfigReader globalClient = scope.ServiceProvider.GetRequiredService<IGlobalConfigReader>();

    GlobalConfig globalConfig = await globalClient.Read();

    await Initialize(scope.ServiceProvider, globalConfig);
}

await StartSaga(app.Services);

app.Run();

static async Task Initialize(IServiceProvider provider, GlobalConfig globalConfig)
{
    ShippingDbContext dbContext = provider.GetService<ShippingDbContext>();

    if (globalConfig.ShouldResetData)
    {
        await dbContext.Database.EnsureDeletedAsync();
    }

    await dbContext.Database.MigrateAsync();
}

static async Task StartSaga(IServiceProvider provider, CancellationToken cancellationToken = default)
{
    await Task.Delay(Constants.SagaStartDelay);

    IShippingOrchestratorSaga orchestrator = provider.GetRequiredService<IShippingOrchestratorSaga>();

    await orchestrator.HandleProcessDelivery(cancellationToken);

    IShippingChoreographySaga choreo = provider.GetRequiredService<IShippingChoreographySaga>();

    await choreo.HandleCreateDeliveryWhenInventoryGoodsDelivered(cancellationToken);
}
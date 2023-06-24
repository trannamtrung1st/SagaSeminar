using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SagaSeminar.Services.InventoryService.Entities;
using SagaSeminar.Services.InventoryService.Services;
using SagaSeminar.Services.InventoryService.Services.Interfaces;
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
    .AddDbContext<InventoryDbContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString(nameof(InventoryDbContext))))
    .AddUnitOfWork<InventoryDbContext>()
    .AddRepositories()
    .AddRedis()
    .AddKafka(builder.Configuration)
    .AddGlobalConfigReader()
    .AddLogClient("red", nameof(SagaSeminar.Services.InventoryService))
    .AddServiceClients()
    .AddSingleton<IInventoryPublisher, InventoryPublisher>()
    .AddScoped<IInventoryService, InventoryService>();

builder.Services.ConfigureApiInfo(builder.Configuration);

builder.Services.AddSingleton<IInventoryChoreographySaga, InventoryChoreographySaga>()
    .AddSingleton<IInventoryOrchestratorSaga, InventoryOrchestratorSaga>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapGet("/api/inventory-notes", async (
    [FromQuery] int skip,
    [FromQuery] int take,
    [FromServices] IInventoryService inventoryService) =>
{
    ListResponseModel<InventoryNoteModel> response = await inventoryService.GetInventoryNotes(skip, take);

    return Results.Ok(response);
})
.WithName("Get inventory notes");

app.MapGet("/api/inventory-notes/available-quantity", async (
    [FromServices] IInventoryService inventoryService) =>
{
    int response = await inventoryService.GetInventoryAvailableQuantity();

    return Results.Ok(response);
})
.WithName("Get inventory available quantity");

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
    InventoryDbContext dbContext = provider.GetService<InventoryDbContext>();

    if (globalConfig.ShouldResetData)
    {
        await dbContext.Database.EnsureDeletedAsync();
    }

    await dbContext.Database.MigrateAsync();
}

static async Task StartSaga(IServiceProvider provider, CancellationToken cancellationToken = default)
{
    await Task.Delay(Constants.SagaStartDelay);

    IInventoryOrchestratorSaga orchestrator = provider.GetRequiredService<IInventoryOrchestratorSaga>();

    await orchestrator.HandleInventoryDelivery(cancellationToken);

    await orchestrator.HandleReverseInventoryDelivery(cancellationToken);

    IInventoryChoreographySaga choreo = provider.GetRequiredService<IInventoryChoreographySaga>();

    await choreo.HandleInventoryDeliveryWhenOrderPaymentCreated(cancellationToken);

    await choreo.HandleReverseInventoryDeliveryWhenDeliveryFailed(cancellationToken);
}
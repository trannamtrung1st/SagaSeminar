using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SagaSeminar.Services.OrderService.Entities;
using SagaSeminar.Services.OrderService.Services;
using SagaSeminar.Services.OrderService.Services.Interfaces;
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
    .AddDbContext<OrderDbContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString(nameof(OrderDbContext))))
    .AddUnitOfWork<OrderDbContext>()
    .AddRepositories()
    .AddRedis()
    .AddKafka(builder.Configuration)
    .AddGlobalConfigReader()
    .AddLogClient("green", nameof(SagaSeminar.Services.OrderService))
    .AddSingleton<IOrderPublisher, OrderPublisher>()
    .AddScoped<IOrderService, OrderService>();

builder.Services.ConfigureApiInfo(builder.Configuration);

builder.Services.AddSingleton<IOrderChoreographySaga, OrderChoreographySaga>()
    .AddSingleton<IOrderOrchestratorSaga, OrderOrchestratorSaga>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapPost("/api/orders", async (
    [FromBody] CreateOrderModel model,
    [FromServices] IOrderService orderService) =>
{
    await orderService.CreateOrder(model);

    return Results.NoContent();
})
.WithName("Create order");

app.MapGet("/api/orders", async (
    [FromQuery] int skip,
    [FromQuery] int take,
    [FromServices] IOrderService orderService) =>
{
    ListResponseModel<OrderModel> response = await orderService.GetOrders(skip, take);

    return Results.Ok(response);
})
.WithName("Get orders");

app.MapGet("/api/orders/details", async (
    [FromQuery] Guid? id,
    [FromQuery] Guid? transactionId,
    [FromServices] IOrderService orderService) =>
{
    OrderModel model;

    if (id.HasValue)
    {
        model = await orderService.GetOrderById(id.Value);
    }
    else if (transactionId.HasValue)
    {
        model = await orderService.GetOrderByTransactionId(transactionId.Value);
    }
    else
    {
        return Results.BadRequest();
    }


    return Results.Ok(model);
})
.WithName("Get order details");

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
    OrderDbContext dbContext = provider.GetService<OrderDbContext>();

    if (globalConfig.ShouldResetData)
    {
        await dbContext.Database.EnsureDeletedAsync();
    }

    await dbContext.Database.MigrateAsync();
}

static async Task StartSaga(IServiceProvider provider, CancellationToken cancellationToken = default)
{
    await Task.Delay(Constants.SagaStartDelay);

    IOrderOrchestratorSaga orchestrator = provider.GetRequiredService<IOrderOrchestratorSaga>();

    await orchestrator.HandleCancelOrder(cancellationToken);

    await orchestrator.HandleCompleteOrder(cancellationToken);

    IOrderChoreographySaga choreo = provider.GetRequiredService<IOrderChoreographySaga>();

    await choreo.HandleCancelOrderWhenPaymentFailed(cancellationToken);

    await choreo.HandleCancelOrderWhenInventoryDeliveryFailed(cancellationToken);

    await choreo.HandleCancelOrderWhenDeliveryFailed(cancellationToken);

    await choreo.HandleCompleteOrderWhenDeliveryCreated(cancellationToken);
}
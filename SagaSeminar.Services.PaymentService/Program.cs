using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SagaSeminar.Services.PaymentService.Entities;
using SagaSeminar.Services.PaymentService.Services;
using SagaSeminar.Services.PaymentService.Services.Interfaces;
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
    .AddDbContext<PaymentDbContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString(nameof(PaymentDbContext))))
    .AddUnitOfWork<PaymentDbContext>()
    .AddRepositories()
    .AddRedis()
    .AddKafka(builder.Configuration)
    .AddGlobalConfigReader()
    .AddLogClient("blue", nameof(SagaSeminar.Services.PaymentService))
    .AddSingleton<IPaymentPublisher, PaymentPublisher>()
    .AddScoped<IPaymentService, PaymentService>();

builder.Services.ConfigureApiInfo(builder.Configuration);

builder.Services.AddSingleton<IPaymentChoreographySaga, PaymentChoreographySaga>()
    .AddSingleton<IPaymentOrchestratorSaga, PaymentOrchestratorSaga>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapGet("/api/payments", async (
    [FromQuery] int skip,
    [FromQuery] int take,
    [FromServices] IPaymentService paymentService) =>
{
    ListResponseModel<PaymentModel> response = await paymentService.GetPayments(skip, take);

    return Results.Ok(response);
})
.WithName("Get payments");

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
    PaymentDbContext dbContext = provider.GetService<PaymentDbContext>();

    if (globalConfig.ShouldResetData)
    {
        await dbContext.Database.EnsureDeletedAsync();
    }

    await dbContext.Database.MigrateAsync();
}

static async Task StartSaga(IServiceProvider provider, CancellationToken cancellationToken = default)
{
    await Task.Delay(Constants.SagaStartDelay);

    IPaymentOrchestratorSaga orchestrator = provider.GetRequiredService<IPaymentOrchestratorSaga>();

    await orchestrator.HandleCancelPayment(cancellationToken);

    await orchestrator.HandleProcessPayment(cancellationToken);

    IPaymentChoreographySaga choreo = provider.GetRequiredService<IPaymentChoreographySaga>();

    await choreo.HandleCreatePaymentWhenOrderCreated(cancellationToken);

    await choreo.HandleCancelPaymentWhenInventoryDeliveryFailed(cancellationToken);

    await choreo.HandleCancelPaymentWhenDeliveryFailed(cancellationToken);
}
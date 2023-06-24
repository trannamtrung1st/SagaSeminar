using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using SagaSeminar.Entities;
using SagaSeminar.Hubs;
using SagaSeminar.Services;
using SagaSeminar.Services.Interfaces;
using SagaSeminar.Shared;
using SagaSeminar.Shared.Commands;
using SagaSeminar.Shared.Events;
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
builder.Services.AddSignalR();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

builder.Services
    .AddDbContext<SagaDbContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString(nameof(SagaDbContext))))
    .AddUnitOfWork<SagaDbContext>()
    .AddRepositories()
    .AddRedis()
    .AddKafka(builder.Configuration, adminEnabled: true)
    .AddGlobalConfigServices()
    .AddLogClient("black", "Global")
    .AddServiceClients()
    .AddSingleton<IOrderProcessingOrchestrator, OrderProcessingOrchestrator>()
    .AddSingleton<IOrderProcessingOrchestrator, OrderProcessingReplayOrchestrator>()
    .AddScoped<IOrderProcessingPublisher, OrderProcessingPublisher>()
    .AddScoped<ITransactionService, TransactionService>();

builder.Services.ConfigureApiInfo(builder.Configuration);

var app = builder.Build();

app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.MapPut("/api/global-config", async (
    [FromBody] GlobalConfig config,
    [FromServices] IGlobalConfigService configService) =>
{
    await configService.Update(config);

    return Results.NoContent();
})
.WithName("Update global config");

app.MapGet("/api/global-config", async (
    [FromServices] IGlobalConfigReader reader) =>
{
    GlobalConfig config = await reader.Read();

    return Results.Ok(config);
})
.WithName("Get global config");

app.MapGet("/api/transactions", async (
    [FromQuery] int skip,
    [FromQuery] int take,
    [FromServices] ITransactionService transactionService) =>
{
    ListResponseModel<TransactionListingModel> response = await transactionService.GetTransactions(skip, take);

    return Results.Ok(response);
})
.WithName("Get transactions");

app.MapGet("/api/transactions/{id}", async (
    [FromRoute] Guid id,
    [FromServices] ITransactionService transactionService) =>
{
    TransactionDetailsModel model = await transactionService.GetTransactionDetails(id);

    return Results.Ok(model);
})
.WithName("Get transaction details");

app.MapPost("/api/orders/transactions/{id}/retry/{sagaTransactionId}", async (
    [FromRoute] Guid id,
    [FromRoute] Guid sagaTransactionId,
    [FromServices] IOrderProcessingPublisher publisher) =>
{
    await publisher.Retry(id, sagaTransactionId);

    return Results.NoContent();
})
.WithName("Retry order transaction");

app.MapGetConfigurations();

app.MapHub<LogHub>("/hub/logs");

using (IServiceScope scope = app.Services.CreateScope())
{
    IGlobalConfigReader globalClient = scope.ServiceProvider.GetRequiredService<IGlobalConfigReader>();

    GlobalConfig globalConfig = await globalClient.Read();

    await Initialize(scope.ServiceProvider, globalConfig);
}

await StartOrchestrator(app.Services);

app.Run();

static async Task Initialize(IServiceProvider provider, GlobalConfig globalConfig)
{
    SagaDbContext dbContext = provider.GetRequiredService<SagaDbContext>();
    IAdminClient kafkaAdmin = provider.GetRequiredService<IAdminClient>();

    string[] allTopics = kafkaAdmin
        .GetMetadata(TimeSpan.FromSeconds(7)).Topics
        .Where(t => !t.Topic.StartsWith("__"))
        .Select(t => t.Topic)
        .ToArray();
    bool shouldCreateTopics = allTopics.Length == 0;

    if (globalConfig.ShouldResetData)
    {
        if (allTopics.Length > 0)
        {
            await kafkaAdmin.DeleteTopicsAsync(allTopics);

            await Task.Delay(Constants.SagaStartDelay);

            shouldCreateTopics = true;
        }

        await dbContext.Database.EnsureDeletedAsync();
    }

    if (shouldCreateTopics)
    {
        List<TopicSpecification> topicSpecs = new List<TopicSpecification>
        {
            new TopicSpecification { Name = nameof(CancelOrderCommand) },
            new TopicSpecification { Name = nameof(CancelPaymentCommand) },
            new TopicSpecification { Name = nameof(CompleteOrderCommand) },
            new TopicSpecification { Name = nameof(InventoryDeliveryCommand) },
            new TopicSpecification { Name = nameof(ProcessDeliveryCommand) },
            new TopicSpecification { Name = nameof(ProcessPaymentCommand) },
            new TopicSpecification { Name = nameof(ReverseInventoryDeliveryCommand) },
            new TopicSpecification { Name = nameof(CompleteOrderFailedEvent) },
            new TopicSpecification { Name = nameof(DeliveryCreatedEvent) },
            new TopicSpecification { Name = nameof(DeliveryFailedEvent) },
            new TopicSpecification { Name = nameof(InventoryDeliveryFailedEvent) },
            new TopicSpecification { Name = nameof(InventoryDeliveryNoteCreatedEvent) },
            new TopicSpecification { Name = nameof(OrderCompletedEvent) },
            new TopicSpecification { Name = nameof(OrderCreatedEvent) },
            new TopicSpecification { Name = nameof(PaymentCreatedEvent) },
            new TopicSpecification { Name = nameof(PaymentFailedEvent) },
            new TopicSpecification { Name = nameof(TransactionUpdatedEvent) }
        };

        await kafkaAdmin.CreateTopicsAsync(topicSpecs);
    }

    await dbContext.Database.MigrateAsync();
}

static async Task StartOrchestrator(IServiceProvider provider, CancellationToken cancellationToken = default)
{
    await Task.Delay(Constants.SagaStartDelay);

    IEnumerable<IOrderProcessingOrchestrator> orchestrators = provider.GetRequiredService<IEnumerable<IOrderProcessingOrchestrator>>();

    foreach (IOrderProcessingOrchestrator orchestrator in orchestrators)
    {
        await orchestrator.Start(cancellationToken);
    }
}
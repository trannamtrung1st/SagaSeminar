using Microsoft.EntityFrameworkCore;
using SagaSeminar.Services.OrderService.Entities;
using SagaSeminar.Services.OrderService.Services.Interfaces;
using SagaSeminar.Shared;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Service.Interfaces;
using SagaSeminar.Shared.Service.Repositories.Interfaces;
using SagaSeminar.Shared.Service.Services.Interfaces;

namespace SagaSeminar.Services.OrderService.Services
{
    public class OrderService : IOrderService
    {
        const int MaxTryCount = 5;

        private readonly IRepository<OrderDbContext, OrderEntity> _orderRepository;
        private readonly IUnitOfWork<OrderDbContext> _unitOfWork;
        private readonly IOrderPublisher _orderPublisher;
        private readonly IGlobalConfigReader _globalConfigReader;
        private readonly ILogClient _logClient;

        public OrderService(IRepository<OrderDbContext, OrderEntity> orderRepository,
            IUnitOfWork<OrderDbContext> unitOfWork,
            IOrderPublisher orderPublisher,
            IGlobalConfigReader globalConfigReader,
            ILogClient logClient)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
            _orderPublisher = orderPublisher;
            _globalConfigReader = globalConfigReader;
            _logClient = logClient;
        }

        public async Task CompleteOrder(Guid transactionId)
        {
            GlobalConfig config = await _globalConfigReader.Read();
            int retryCount = 0;
            bool successful = false;

            while (!successful && retryCount++ < MaxTryCount)
            {
                try
                {
                    await _logClient.Log($"Start completing order of transaction {transactionId} (try {retryCount})");

                    await _globalConfigReader.Delay();

                    if (retryCount <= config.CompleteOrderRetryCount)
                    {
                        throw new Exception($"Failed to complete order (try {retryCount})!");
                    }

                    OrderEntity entity = await _orderRepository.Query()
                        .Where(e => e.TransactionId == transactionId)
                        .FirstOrDefaultAsync();

                    if (entity == null) throw new Exception("Order not found");

                    entity.Status = Constants.OrderStatus.Completed;

                    await _unitOfWork.CommitChanges();

                    successful = true;

                    await _orderPublisher.PublishOrderCompleted(transactionId);

                    await _logClient.Log($"Order {entity.Id} was completed");
                }
                catch (Exception ex)
                {
                    await _logClient.Log(ex.Message);

                    if (retryCount >= MaxTryCount)
                    {
                        await _logClient.Log($"Failed to complete order after {retryCount} tries");

                        await _orderPublisher.PublishCompleteOrderFailed(transactionId, ex.Message);

                        throw;
                    }
                }
            }
        }

        public async Task CancelOrder(Guid transactionId, string note)
        {
            try
            {
                await _logClient.Log($"Start cancelling order of transaction {transactionId}");

                await _globalConfigReader.Delay();

                OrderEntity entity = await _orderRepository.Query()
                    .Where(e => e.TransactionId == transactionId)
                    .FirstOrDefaultAsync();

                if (entity == null) throw new Exception("Order not found");

                entity.Status = Constants.OrderStatus.Cancelled;
                entity.Note = note;

                await _unitOfWork.CommitChanges();

                await _logClient.Log($"Order {entity.Id} was cancelled");
            }
            catch (Exception ex)
            {
                await _logClient.Log(ex.Message);

                throw;
            }
        }

        public async Task CreateOrder(CreateOrderModel model)
        {
            try
            {
                await _logClient.Log($"Start creating order amount {model.Amount} of {model.Customer}");

                await _globalConfigReader.Delay();

                await _globalConfigReader.ThrowIfShould(
                    e => e.ShouldCreateOrderFail,
                    new Exception("Failed to create order"));

                Guid id = Guid.NewGuid();

                OrderEntity entity = new OrderEntity
                {
                    Id = id,
                    Amount = model.Amount,
                    Customer = model.Customer,
                    CreatedTime = DateTime.Now,
                    Status = Constants.OrderStatus.Processing,
                    TransactionId = id
                };

                await _orderRepository.Create(entity);

                await _unitOfWork.CommitChanges();

                await _logClient.Log($"Order amount {model.Amount} of {model.Customer} was created");

                OrderModel entityModel = entity.ToOrderModel();

                await _orderPublisher.PublishOrderCreated(entityModel);

                await _logClient.Log($"Published order created event of order {entity.Id}");
            }
            catch (Exception ex)
            {
                await _logClient.Log(ex.Message);

                throw;
            }
        }

        public async Task<OrderModel> GetOrderById(Guid id)
        {
            OrderModel model = await _orderRepository.Query()
                .Where(e => e.Id == id)
                .ToOrderModel()
                .FirstOrDefaultAsync();

            if (model == null) throw new Exception("Entity not found!");

            return model;
        }

        public async Task<OrderModel> GetOrderByTransactionId(Guid transactionId)
        {
            OrderModel model = await _orderRepository.Query()
                .Where(e => e.TransactionId == transactionId)
                .ToOrderModel()
                .FirstOrDefaultAsync();

            if (model == null) throw new Exception("Entity not found!");

            return model;
        }

        public async Task<ListResponseModel<OrderModel>> GetOrders(int skip, int take)
        {
            IQueryable<OrderEntity> query = _orderRepository.Query();

            int count = await query.CountAsync();

            query = query
                .OrderByDescending(e => e.CreatedTime)
                .Skip(skip).Take(take);

            OrderModel[] list = await query
                .ToOrderModel()
                .ToArrayAsync();

            return new ListResponseModel<OrderModel>
            {
                Total = count,
                List = list
            };
        }
    }
}

using Microsoft.EntityFrameworkCore;
using SagaSeminar.Services.InventoryService.Entities;
using SagaSeminar.Services.InventoryService.Services.Interfaces;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Service.Interfaces;
using SagaSeminar.Shared.Service.Repositories.Interfaces;
using SagaSeminar.Shared.Service.Services.Interfaces;
using SagaSeminar.Shared.Services.Interfaces;

namespace SagaSeminar.Services.InventoryService.Services
{
    public class InventoryService : IInventoryService
    {
        const int DefaultUnitPrice = 1;

        private readonly IRepository<InventoryDbContext, InventoryNoteEntity> _inventoryNoteRepository;
        private readonly IUnitOfWork<InventoryDbContext> _unitOfWork;
        private readonly IInventoryPublisher _inventoryPublisher;
        private readonly IGlobalConfigReader _globalConfigReader;
        private readonly ILogClient _logClient;
        private readonly IOrderClient _orderClient;

        public InventoryService(IRepository<InventoryDbContext, InventoryNoteEntity> inventoryNoteRepository,
            IUnitOfWork<InventoryDbContext> unitOfWork,
            IInventoryPublisher inventoryPublisher,
            IGlobalConfigReader globalConfigReader,
            ILogClient logClient,
            IOrderClient orderClient)
        {
            _inventoryNoteRepository = inventoryNoteRepository;
            _unitOfWork = unitOfWork;
            _inventoryPublisher = inventoryPublisher;
            _globalConfigReader = globalConfigReader;
            _logClient = logClient;
            _orderClient = orderClient;
        }

        public async Task CreateInventoryDeliveryNote(PaymentModel fromPayment)
        {
            try
            {
                await _logClient.Log($"Start creating inventory delivery for payment {fromPayment.Amount} of order {fromPayment.OrderId}");

                await _globalConfigReader.Delay();

                await _globalConfigReader.ThrowIfShould(
                    e => e.ShouldInventoryDeliveryFail,
                    new Exception("Failed to do inventory delivery"));

                OrderModel order = await _orderClient.GetOrderDetails(id: fromPayment.OrderId, transactionId: null);
                int quantity = -(int)order.Amount / DefaultUnitPrice;
                int availableQuantity = await GetAvailableQuantity();

                if (availableQuantity + quantity < 0) throw new Exception("Failed to delivery due to out of stock!");

                Guid id = Guid.NewGuid();

                InventoryNoteEntity entity = new InventoryNoteEntity
                {
                    Id = id,
                    Quantity = quantity,
                    CreatedTime = DateTime.Now,
                    Reason = $"Delivery goods for order {order.Amount} of customer {order.Customer}",
                    TransactionId = fromPayment.TransactionId,
                    Note = ""
                };

                await _inventoryNoteRepository.Create(entity);

                await _unitOfWork.CommitChanges();

                await _logClient.Log($"Inventory delivery note {entity.Id} with quantity of {entity.Quantity} was created");

                InventoryNoteModel entityModel = entity.ToInventoryNoteModel();

                await _inventoryPublisher.PublishInventoryDeliveryNoteCreated(entityModel);

                await _logClient.Log($"Published inventory delivery note created event of inventory {entity.Id}");
            }
            catch (Exception ex)
            {
                await _logClient.Log(ex.Message);

                await _inventoryPublisher.PublishInventoryDeliveryFailed(fromPayment.TransactionId, ex.Message);

                await _logClient.Log($"Published inventory delivery failed event of transaction {fromPayment.TransactionId}");

                throw;
            }
        }

        public Task<int> GetInventoryAvailableQuantity() => GetAvailableQuantity();

        public async Task<ListResponseModel<InventoryNoteModel>> GetInventoryNotes(int skip, int take)
        {
            IQueryable<InventoryNoteEntity> query = _inventoryNoteRepository.Query();

            int count = await query.CountAsync();

            query = query
                .OrderByDescending(e => e.CreatedTime)
                .Skip(skip).Take(take);

            InventoryNoteModel[] list = await query
                .Select(e => new InventoryNoteModel
                {
                    Id = e.Id,
                    Quantity = e.Quantity,
                    CreatedTime = e.CreatedTime,
                    Reason = e.Reason,
                    TransactionId = e.TransactionId,
                    Note = e.Note,
                }).ToArrayAsync();

            return new ListResponseModel<InventoryNoteModel>
            {
                Total = count,
                List = list
            };
        }

        public async Task ReverseDelivery(Guid transactionId, string note)
        {
            try
            {
                await _logClient.Log($"Start reversing inventory delivery for transaction {transactionId}");

                await _globalConfigReader.Delay();

                await _globalConfigReader.ThrowIfShould(
                    e => e.ShouldInventoryDeliveryFail,
                    new Exception("Failed to do inventory delivery"));

                InventoryNoteEntity previousNote = await _inventoryNoteRepository.Query()
                    .Where(e => e.TransactionId == transactionId)
                    .Where(e => e.Quantity < 0)
                    .FirstOrDefaultAsync();

                if (previousNote == null) throw new Exception("Entity not found!");

                Guid id = Guid.NewGuid();

                InventoryNoteEntity entity = new InventoryNoteEntity
                {
                    Id = id,
                    Quantity = -previousNote.Quantity,
                    CreatedTime = DateTime.Now,
                    Reason = $"Transaction {previousNote.TransactionId}: {note}",
                    TransactionId = previousNote.TransactionId
                };

                await _inventoryNoteRepository.Create(entity);

                await _unitOfWork.CommitChanges();

                await _logClient.Log($"Reversed inventory delivery, receipt note {entity.Id} with quantity of {entity.Quantity} was created");
            }
            catch (Exception ex)
            {
                await _logClient.Log(ex.Message);

                throw;
            }
        }

        private async Task<int> GetAvailableQuantity()
        {
            int availableQuantity = await _inventoryNoteRepository.Query()
                .SumAsync(e => e.Quantity);

            return availableQuantity;
        }
    }
}

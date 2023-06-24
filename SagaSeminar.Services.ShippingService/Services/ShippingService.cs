using Microsoft.EntityFrameworkCore;
using SagaSeminar.Services.ShippingService.Entities;
using SagaSeminar.Services.ShippingService.Services.Interfaces;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Service.Interfaces;
using SagaSeminar.Shared.Service.Repositories.Interfaces;
using SagaSeminar.Shared.Service.Services.Interfaces;
using SagaSeminar.Shared.Services.Interfaces;

namespace SagaSeminar.Services.ShippingService.Services
{
    public class ShippingService : IShippingService
    {
        private readonly IRepository<ShippingDbContext, DeliveryEntity> _deliveryRepository;
        private readonly IUnitOfWork<ShippingDbContext> _unitOfWork;
        private readonly IShippingPublisher _shippingPublisher;
        private readonly IGlobalConfigReader _globalConfigReader;
        private readonly ILogClient _logClient;
        private readonly IOrderClient _orderClient;

        public ShippingService(IRepository<ShippingDbContext, DeliveryEntity> deliveryRepository,
            IUnitOfWork<ShippingDbContext> unitOfWork,
            IShippingPublisher shippingPublisher,
            IGlobalConfigReader globalConfigReader,
            ILogClient logClient,
            IOrderClient orderClient)
        {
            _deliveryRepository = deliveryRepository;
            _unitOfWork = unitOfWork;
            _shippingPublisher = shippingPublisher;
            _globalConfigReader = globalConfigReader;
            _logClient = logClient;
            _orderClient = orderClient;
        }

        public async Task CreateDelivery(InventoryNoteModel fromNote)
        {
            try
            {
                await _logClient.Log($"Start processing delivery with quantity of {-fromNote.Quantity}");

                await _globalConfigReader.Delay();

                await _globalConfigReader.ThrowIfShould(
                    e => e.ShouldDeliveryFail,
                    new Exception("Failed to process delivery"));

                OrderModel order = await _orderClient.GetOrderDetails(id: null, transactionId: fromNote.TransactionId);

                Guid id = Guid.NewGuid();

                DeliveryEntity entity = new DeliveryEntity
                {
                    Id = id,
                    Quantity = -fromNote.Quantity,
                    CreatedTime = DateTime.Now,
                    TransactionId = fromNote.TransactionId,
                    Customer = order.Customer,
                    OrderId = order.Id
                };

                await _deliveryRepository.Create(entity);

                await _unitOfWork.CommitChanges();

                await _logClient.Log($"Delivery for order {entity.OrderId} to customer {entity.Customer} was created");

                DeliveryModel entityModel = entity.ToDeliveryModel();

                await _shippingPublisher.PublishDeliveryCreated(entityModel);

                await _logClient.Log($"Published delivery created event of delivery {entity.Id}");
            }
            catch (Exception ex)
            {
                await _logClient.Log(ex.Message);

                await _shippingPublisher.PublishDeliveryFailed(fromNote.TransactionId, ex.Message);

                await _logClient.Log($"Published delivery failed event of transaction {fromNote.TransactionId}");

                throw;
            }
        }

        public async Task<ListResponseModel<DeliveryModel>> GetDeliveries(int skip, int take)
        {
            IQueryable<DeliveryEntity> query = _deliveryRepository.Query();

            int count = await query.CountAsync();

            query = query
                .OrderByDescending(e => e.CreatedTime)
                .Skip(skip).Take(take);

            DeliveryModel[] list = await query
                .Select(e => new DeliveryModel
                {
                    Id = e.Id,
                    CreatedTime = e.CreatedTime,
                    OrderId = e.OrderId,
                    Quantity = e.Quantity,
                    Customer = e.Customer,
                    TransactionId = e.TransactionId,
                }).ToArrayAsync();

            return new ListResponseModel<DeliveryModel>
            {
                Total = count,
                List = list
            };
        }
    }
}

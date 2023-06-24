using Microsoft.EntityFrameworkCore;
using SagaSeminar.Services.PaymentService.Entities;
using SagaSeminar.Services.PaymentService.Services.Interfaces;
using SagaSeminar.Shared;
using SagaSeminar.Shared.Models;
using SagaSeminar.Shared.Service.Interfaces;
using SagaSeminar.Shared.Service.Repositories.Interfaces;
using SagaSeminar.Shared.Service.Services.Interfaces;

namespace SagaSeminar.Services.PaymentService.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IRepository<PaymentDbContext, PaymentEntity> _paymentRepository;
        private readonly IUnitOfWork<PaymentDbContext> _unitOfWork;
        private readonly IPaymentPublisher _paymentPublisher;
        private readonly IGlobalConfigReader _globalConfigReader;
        private readonly ILogClient _logClient;

        public PaymentService(IRepository<PaymentDbContext, PaymentEntity> paymentRepository,
            IUnitOfWork<PaymentDbContext> unitOfWork,
            IPaymentPublisher paymentPublisher,
            IGlobalConfigReader globalConfigReader,
            ILogClient logClient)
        {
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
            _paymentPublisher = paymentPublisher;
            _globalConfigReader = globalConfigReader;
            _logClient = logClient;
        }

        public async Task CancelPayment(Guid transactionId, string note)
        {
            try
            {
                await _logClient.Log($"Start cancelling payment of transaction {transactionId}");

                await _globalConfigReader.Delay();

                PaymentEntity entity = await _paymentRepository.Query()
                    .Where(e => e.TransactionId == transactionId)
                    .FirstOrDefaultAsync();

                if (entity == null) throw new Exception("Payment not found");

                entity.Status = Constants.PaymentStatus.Cancelled;
                entity.Note = note;

                await _unitOfWork.CommitChanges();

                await _logClient.Log($"Payment {entity.Id} was cancelled");
            }
            catch (Exception ex)
            {
                await _logClient.Log(ex.Message);

                throw;
            }
        }

        public async Task CreatePaymentFromOrder(OrderModel orderModel)
        {
            try
            {
                await _logClient.Log($"Start processing payment from order {orderModel.Id}");

                await _globalConfigReader.Delay();

                await _globalConfigReader.ThrowIfShould(
                    e => e.ShouldProcessPaymentFail,
                    new Exception("Failed to process payment"));

                Guid id = Guid.NewGuid();

                PaymentEntity entity = new PaymentEntity
                {
                    Id = id,
                    Amount = orderModel.Amount,
                    OrderId = orderModel.Id,
                    Customer = orderModel.Customer,
                    CreatedTime = DateTime.Now,
                    Status = Constants.PaymentStatus.Successful,
                    TransactionId = orderModel.TransactionId
                };

                await _paymentRepository.Create(entity);

                await _unitOfWork.CommitChanges();

                await _logClient.Log($"Payment amount {entity.Amount} of {entity.Customer} was created");

                PaymentModel entityModel = entity.ToPaymentModel();

                await _paymentPublisher.PublishPaymentCreated(entityModel);

                await _logClient.Log($"Published payment created event of payment {entity.Id}");
            }
            catch (Exception ex)
            {
                await _logClient.Log(ex.Message);

                await _paymentPublisher.PublishPaymentFailed(orderModel.TransactionId, ex.Message);

                await _logClient.Log($"Published payment failed event of order {orderModel.Id}");

                throw;
            }
        }

        public async Task<ListResponseModel<PaymentModel>> GetPayments(int skip, int take)
        {
            IQueryable<PaymentEntity> query = _paymentRepository.Query();

            int count = await query.CountAsync();

            query = query
                .OrderByDescending(e => e.CreatedTime)
                .Skip(skip).Take(take);

            PaymentModel[] list = await query
                .Select(e => new PaymentModel
                {
                    Id = e.Id,
                    Amount = e.Amount,
                    CreatedTime = e.CreatedTime,
                    OrderId = e.OrderId,
                    Status = e.Status,
                    Customer = e.Customer,
                    TransactionId = e.TransactionId,
                    Note = e.Note,
                }).ToArrayAsync();

            return new ListResponseModel<PaymentModel>
            {
                Total = count,
                List = list
            };
        }
    }
}

using BuildingBlocks.Domain.Repositories;
using PaymentService.Domain.Aggregates.PaymentAggregate.Entities;

namespace PaymentService.Domain.Aggregates.PaymentAggregate.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
}
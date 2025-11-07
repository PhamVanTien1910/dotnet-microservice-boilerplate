using BuildingBlocks.EfCore;
using PaymentService.Infrastructure.Data;
using PaymentService.Domain.Aggregates.PaymentAggregate.Entities;
using PaymentService.Domain.Aggregates.PaymentAggregate.Repositories;

namespace PaymentService.Infrastructure.Data.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(PaymentDbContext context) : base(context)
    {
    }

}
using System.Linq.Expressions;
using BuildingBlocks.Domain.Specifications;
using PaymentService.Domain.Aggregates.PaymentAggregate.Entities;

namespace PaymentService.Domain.Aggregates.PaymentAggregate.Specifications;

public class PaymentByUserIdSpecification : Specification<Payment>
{
    private readonly Guid _userId;
    public PaymentByUserIdSpecification(Guid userId)
    {
        _userId = userId;
    }
    public override Expression<Func<Payment, bool>> ToExpression()
    {
        return payment => payment.UserId == _userId;
    }
}
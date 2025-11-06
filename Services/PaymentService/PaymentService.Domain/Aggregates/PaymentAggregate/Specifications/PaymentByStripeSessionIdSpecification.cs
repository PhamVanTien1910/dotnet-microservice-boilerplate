using System.Linq.Expressions;
using BuildingBlocks.Domain.Specifications;
using PaymentService.Domain.Aggregates.PaymentAggregate.Entities;

namespace PaymentService.Domain.Aggregates.PaymentAggregate.Specifications;

public sealed class PaymentByStripeSessionIdSpecification : Specification<Payment>
{
    private readonly string _stripeSessionId;

    public PaymentByStripeSessionIdSpecification(string stripeSessionId)
    {
        _stripeSessionId = stripeSessionId;
    }

    public override Expression<Func<Payment, bool>> ToExpression()
        => payment => payment.StripeSessionId == _stripeSessionId;
}

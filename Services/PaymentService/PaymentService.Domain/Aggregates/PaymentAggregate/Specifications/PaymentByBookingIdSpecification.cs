using System.Linq.Expressions;
using BuildingBlocks.Domain.Specifications;
using PaymentService.Domain.Aggregates.PaymentAggregate.Entities;

namespace PaymentService.Domain.Aggregates.PaymentAggregate.Specifications;

public class PaymentByBookingIdSpecification : Specification<Payment>
{
    private readonly Guid _bookingId;

    public PaymentByBookingIdSpecification(Guid bookingId)
    {
        _bookingId = bookingId;
    }

    public override Expression<Func<Payment, bool>> ToExpression()
    {
        return payment => payment.BookingId == _bookingId;
    }
}
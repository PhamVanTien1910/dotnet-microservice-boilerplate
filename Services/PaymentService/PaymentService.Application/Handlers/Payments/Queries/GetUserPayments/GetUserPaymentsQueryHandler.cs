using BuildingBlocks.MediatR.Abstractions.Query;
using PaymentService.Application.DTOs;
using PaymentService.Domain.Aggregates.PaymentAggregate.Repositories;
using PaymentService.Domain.Aggregates.PaymentAggregate.Specifications;

namespace PaymentService.Application.Handlers.Payments.Queries.GetUserPayments;

public class GetUserPaymentsQueryHandler : IQueryHandler<GetUserPaymentsQuery, List<PaymentDto>>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetUserPaymentsQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<List<PaymentDto>> Handle(GetUserPaymentsQuery request, CancellationToken cancellationToken)
    {
        var payments =
            await _paymentRepository.ListAsync(new PaymentByUserIdSpecification(request.UserId), cancellationToken);

        return payments.Select(payment => new PaymentDto
        {
            Id = payment.Id,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Status = payment.Status,
            CreatedAt = payment.CreatedAt,
            ProcessedAt = payment.ProcessedAt,
            BookingId = payment.BookingId
        }).ToList();
    }
}
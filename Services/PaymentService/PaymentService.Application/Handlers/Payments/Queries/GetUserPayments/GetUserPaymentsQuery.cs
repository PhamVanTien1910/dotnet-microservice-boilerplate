using BuildingBlocks.MediatR.Abstractions.Query;
using PaymentService.Application.DTOs;

namespace PaymentService.Application.Handlers.Payments.Queries.GetUserPayments;

public record GetUserPaymentsQuery(
    Guid UserId) : IQuery<List<PaymentDto>>;
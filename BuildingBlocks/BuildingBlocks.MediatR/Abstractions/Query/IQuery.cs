using MediatR;

namespace BuildingBlocks.MediatR.Abstractions.Query;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}

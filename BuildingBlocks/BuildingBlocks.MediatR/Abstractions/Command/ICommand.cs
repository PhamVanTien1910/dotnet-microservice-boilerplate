using MediatR;

namespace BuildingBlocks.MediatR.Abstractions.Command;

public interface ICommand : IRequest
{
}

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

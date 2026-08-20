using MediatR;

namespace LeavePlanner.Application.Common;

public interface ICommand<TResponse> : IRequest<TResponse>;

public interface IQuery<TResponse> : IRequest<TResponse>;

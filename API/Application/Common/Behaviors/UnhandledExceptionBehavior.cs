using MediatR;

namespace LeavePlanner.Application.Common.Behaviors;

public class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull
{
	private readonly ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> _logger;

	public UnhandledExceptionBehavior(ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger) => _logger = logger;

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		try
		{
			return await next();
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "{Request} threw an unhandled exception", typeof(TRequest).Name);
			throw;
		}
	}
}

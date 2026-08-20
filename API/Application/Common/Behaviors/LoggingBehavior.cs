using System.Diagnostics;
using MediatR;

namespace LeavePlanner.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull
{
	private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

	public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) => _logger = logger;

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		var name = typeof(TRequest).Name;
		var stopwatch = Stopwatch.StartNew();

		var response = await next();

		stopwatch.Stop();

		if (response is Result { IsSuccess: false } failure)
		{
			_logger.LogWarning("{Request} failed in {Elapsed}ms: {ErrorType} - {Error}",
				name, stopwatch.ElapsedMilliseconds, failure.ErrorType, failure.Error);
		}
		else
		{
			_logger.LogInformation("{Request} handled in {Elapsed}ms", name, stopwatch.ElapsedMilliseconds);
		}

		return response;
	}
}

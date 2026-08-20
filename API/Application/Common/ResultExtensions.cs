namespace LeavePlanner.Application.Common;

public static class ResultExtensions
{
	public static IResult ToHttpResult<T>(this Result<T> result) =>
		result.IsSuccess
			? Results.Ok(result.Value)
			: Failure(result);

	public static IResult ToHttpResult(this Result result, object? okPayload = null) =>
		result.IsSuccess
			? okPayload is null ? Results.Ok() : Results.Ok(okPayload)
			: Failure(result);

	private static IResult Failure(Result result) => result.ErrorType switch
	{
		ResultErrorType.NotFound => Results.NotFound(result.Error),
		_ => Results.BadRequest(result.Error)
	};
}

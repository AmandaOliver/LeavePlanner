namespace LeavePlanner.Application.Common;

public enum ResultErrorType
{
	Validation,
	NotFound
}

public class Result
{
	protected Result(bool isSuccess, string? error, ResultErrorType errorType)
	{
		IsSuccess = isSuccess;
		Error = error;
		ErrorType = errorType;
	}

	public bool IsSuccess { get; }
	public string? Error { get; }
	public ResultErrorType ErrorType { get; }

	public static Result Success() => new(true, null, ResultErrorType.Validation);
	public static Result Invalid(string error) => new(false, error, ResultErrorType.Validation);
	public static Result NotFound(string error) => new(false, error, ResultErrorType.NotFound);
}

public class Result<T> : Result
{
	private Result(bool isSuccess, T? value, string? error, ResultErrorType errorType)
		: base(isSuccess, error, errorType)
	{
		Value = value;
	}

	public T? Value { get; }

	public static Result<T> Success(T value) => new(true, value, null, ResultErrorType.Validation);
	public static new Result<T> Invalid(string error) => new(false, default, error, ResultErrorType.Validation);
	public static new Result<T> NotFound(string error) => new(false, default, error, ResultErrorType.NotFound);
}

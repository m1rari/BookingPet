namespace BuildingBlocks.Common.Results;

/// <summary>
/// Represents the result of an operation with success/failure and error information.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Invalid result: Success result cannot have an error.");
            
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Invalid result: Failure result must have an error.");
            
        IsSuccess = isSuccess;
        Error = error;
    }
    
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }
    
    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);
    public static Result<TValue> Failure<TValue>(Error error) => new(default!, false, error);
    
    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>
/// Represents the result of an operation with a return value.
/// </summary>
public class Result<TValue> : Result
{
    private readonly TValue? _value;
    
    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }
    
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of a failed result.");
        
    public static implicit operator Result<TValue>(TValue? value) => 
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
        
    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);
}


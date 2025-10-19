namespace BuildingBlocks.Common.Results;

/// <summary>
/// Represents an error with a code and message.
/// </summary>
public sealed record Error
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "The specified value was null.");
    
    public Error(string code, string message)
    {
        Code = code;
        Message = message;
    }
    
    public string Code { get; }
    public string Message { get; }
    
    public static implicit operator string(Error error) => error.Code;
}


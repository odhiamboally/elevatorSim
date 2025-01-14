namespace ES.Shared.Exceptions;
public class FullCapacityException : CustomException
{
    public FullCapacityException()
    {
            
    }

    public FullCapacityException(string message) : base(message) { }

    public FullCapacityException(string message, Exception innerException) : base(message, innerException) { }
}

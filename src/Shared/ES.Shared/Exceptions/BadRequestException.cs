using System.Net;

namespace ES.Shared.Exceptions;
public class BadRequestException : CustomException
{
    public BadRequestException(string message) : base(message, null, HttpStatusCode.BadRequest) { }
}

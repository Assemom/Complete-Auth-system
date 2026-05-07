using System.Net;

namespace App.Shared.Exceptions;

public class ValidationException : AppException
{
    public ValidationException(string message)
        : base(message, HttpStatusCode.BadRequest)
    {
    }
}

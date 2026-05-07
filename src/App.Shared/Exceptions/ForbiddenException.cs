using System.Net;

namespace App.Shared.Exceptions;

public class ForbiddenException : AppException
{
    public ForbiddenException(string message)
        : base(message, HttpStatusCode.Forbidden)
    {
    }
}

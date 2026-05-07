using System.Net;

namespace App.Shared.Exceptions;

public class ConflictException : AppException
{
    public ConflictException(string message)
        : base(message, HttpStatusCode.Conflict)
    {
    }
}

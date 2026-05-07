using System.Net;

namespace App.Shared.Exceptions;

public abstract class AppException : Exception
{
    protected AppException(string message, HttpStatusCode statusCode)
        : base(message)
    {
        StatusCode = (int)statusCode;
    }

    public int StatusCode { get; }
}

using System.Net;

namespace App.Shared.Exceptions;

public class ServerException : AppException
{
    public ServerException(string message)
        : base(message, HttpStatusCode.InternalServerError)
    {
    }
}

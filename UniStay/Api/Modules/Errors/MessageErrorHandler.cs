using Application.Messages.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Api.Modules.Errors
{
    public static class MessageErrorHandler
    {
        public static ObjectResult ToObjectResult(this MessageException exception)
        {
            return new ObjectResult(new { Message = exception.Message })
            {
                StatusCode = exception switch
                {
                    MessageNotFoundException => StatusCodes.Status404NotFound,
                    ReceiverNotFoundException => StatusCodes.Status400BadRequest,
                    CannotSendMessageToSelfException => StatusCodes.Status400BadRequest,
                    UserNotAuthorizedForMessageOperationException => StatusCodes.Status403Forbidden,
                    MessageOperationFailedException => StatusCodes.Status500InternalServerError,
                    _ => throw new NotImplementedException($"Handling for {exception.GetType().Name} is not implemented in MessageErrorHandler.")
                }
            };
        }
    }
}
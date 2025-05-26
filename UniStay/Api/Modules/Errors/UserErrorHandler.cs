using Application.Users.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Api.Modules.Errors
{
    public static class UserErrorHandler
    {
        public static ObjectResult ToObjectResult(this UserException exception)
        {
            return new ObjectResult(new { Message = exception.Message })
            {
                StatusCode = exception switch
                {
                    UserNotFoundException => StatusCodes.Status404NotFound,
                    UserAlreadyExistsException => StatusCodes.Status409Conflict,
                    UserOperationFailedException => StatusCodes.Status500InternalServerError,
                    _ => throw new NotImplementedException($"Handling for {exception.GetType().Name} is not implemented in UserErrorHandler.")
                }
            };
        }
    }
}
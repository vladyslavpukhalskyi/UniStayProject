using Application.Amenities.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Api.Modules.Errors
{
    public static class AmenityErrorHandler
    {
        public static ObjectResult ToObjectResult(this AmenityException exception)
        {
            return new ObjectResult(new { Message = exception.Message })
            {
                StatusCode = exception switch
                {
                    AmenityNotFoundException => StatusCodes.Status404NotFound,
                    AmenityAlreadyExistsException => StatusCodes.Status409Conflict,
                    AmenityOperationFailedException => StatusCodes.Status500InternalServerError,
                    _ => throw new NotImplementedException($"Handling for {exception.GetType().Name} is not implemented in AmenityErrorHandler.")
                }
            };
        }
    }
}
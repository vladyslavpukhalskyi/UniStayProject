using Application.Reviews.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Api.Modules.Errors
{
    public static class ReviewErrorHandler
    {
        public static ObjectResult ToObjectResult(this ReviewException exception)
        {
            return new ObjectResult(new { Message = exception.Message })
            {
                StatusCode = exception switch
                {
                    ReviewNotFoundException => StatusCodes.Status404NotFound,
                    ListingNotFoundForReviewException => StatusCodes.Status400BadRequest,
                    UserCannotReviewException => StatusCodes.Status403Forbidden,
                    UserNotAuthorizedForReviewOperationException => StatusCodes.Status403Forbidden,
                    ReviewOperationFailedException => StatusCodes.Status500InternalServerError,
                    _ => throw new NotImplementedException($"Handling for {exception.GetType().Name} is not implemented in ReviewErrorHandler.")
                }
            };
        }
    }
}
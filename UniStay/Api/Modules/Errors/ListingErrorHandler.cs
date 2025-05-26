using Application.Listings.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Api.Modules.Errors
{
    public static class ListingErrorHandler
    {
        public static ObjectResult ToObjectResult(this ListingException exception)
        {
            return new ObjectResult(new { Message = exception.Message })
            {
                StatusCode = exception switch
                {
                    ListingNotFoundException => StatusCodes.Status404NotFound,
                    InvalidAmenitiesProvidedException => StatusCodes.Status400BadRequest,
                    UserNotAuthorizedForListingOperationException => StatusCodes.Status403Forbidden,
                    ListingOperationFailedException => StatusCodes.Status500InternalServerError,
                    _ => throw new NotImplementedException($"Handling for {exception.GetType().Name} is not implemented in ListingErrorHandler.")
                }
            };
        }
    }
}
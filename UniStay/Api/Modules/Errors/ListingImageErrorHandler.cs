using Application.ListingImages.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Api.Modules.Errors
{
    public static class ListingImageErrorHandler
    {
        public static ObjectResult ToObjectResult(this ListingImageException exception)
        {
            return new ObjectResult(new { Message = exception.Message })
            {
                StatusCode = exception switch
                {
                    ListingImageNotFoundException => StatusCodes.Status404NotFound,
                    ListingNotFoundForImageException => StatusCodes.Status400BadRequest,
                    UserNotAuthorizedToManageListingImagesException => StatusCodes.Status403Forbidden,
                    ListingImageOperationFailedException => StatusCodes.Status500InternalServerError,
                    _ => throw new NotImplementedException($"Handling for {exception.GetType().Name} is not implemented in ListingImageErrorHandler.")
                }
            };
        }
    }
}
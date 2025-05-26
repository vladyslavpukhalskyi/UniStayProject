using Application.Favorites.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Api.Modules.Errors
{
    public static class FavoriteErrorHandler
    {
        public static ObjectResult ToObjectResult(this FavoriteException exception)
        {
            return new ObjectResult(new { Message = exception.Message })
            {
                StatusCode = exception switch
                {
                    FavoriteNotFoundException => StatusCodes.Status404NotFound,
                    ListingNotFoundForFavoriteException => StatusCodes.Status400BadRequest,
                    UserNotFoundForFavoriteException => StatusCodes.Status400BadRequest,
                    UserAlreadyFavoritedListingException => StatusCodes.Status409Conflict,
                    UserHasNotFavoritedListingException => StatusCodes.Status404NotFound,
                    FavoriteOperationFailedException => StatusCodes.Status500InternalServerError,
                    _ => throw new NotImplementedException($"Handling for {exception.GetType().Name} is not implemented in FavoriteErrorHandler.")
                }
            };
        }
    }
}
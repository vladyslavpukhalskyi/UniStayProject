using Application.Listings.Exceptions; // Розташування ваших ListingException
using Microsoft.AspNetCore.Http;     // Для StatusCodes
using Microsoft.AspNetCore.Mvc;      // Для ObjectResult
using System;                         // Для NotImplementedException

namespace Api.Modules.Errors // Або ваш бажаний простір імен для API модулів
{
    public static class ListingErrorHandler
    {
        public static ObjectResult ToObjectResult(this ListingException exception)
        {
            // Тіло відповіді буде містити повідомлення з винятку,
            // обгорнуте в об'єкт для JSON форматування.
            return new ObjectResult(new { Message = exception.Message })
            {
                StatusCode = exception switch
                {
                    ListingNotFoundException => StatusCodes.Status404NotFound,
                    
                    // Якщо надано невалідні ID зручностей - це помилка запиту
                    InvalidAmenitiesProvidedException => StatusCodes.Status400BadRequest,
                    
                    // Якщо користувач не має прав на операцію з оголошенням
                    UserNotAuthorizedForListingOperationException => StatusCodes.Status403Forbidden,
                    
                    ListingOperationFailedException => StatusCodes.Status500InternalServerError,
                    
                    // Обробка будь-яких інших підтипів ListingException
                    _ => throw new NotImplementedException($"Handling for {exception.GetType().Name} is not implemented in ListingErrorHandler.")
                }
            };
        }
    }
}
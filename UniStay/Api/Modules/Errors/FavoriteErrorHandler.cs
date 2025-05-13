using Application.Favorites.Exceptions; // Розташування ваших FavoriteException
using Microsoft.AspNetCore.Http;      // Для StatusCodes
using Microsoft.AspNetCore.Mvc;       // Для ObjectResult
using System;                          // Для NotImplementedException

namespace Api.Modules.Errors // Або ваш бажаний простір імен для API модулів
{
    public static class FavoriteErrorHandler
    {
        public static ObjectResult ToObjectResult(this FavoriteException exception)
        {
            // Тіло відповіді буде містити повідомлення з винятку,
            // обгорнуте в об'єкт для JSON форматування.
            return new ObjectResult(new { Message = exception.Message })
            {
                StatusCode = exception switch
                {
                    // Якщо сам запис Favorite (який пов'язує оголошення та користувачів) не знайдено за його ID
                    FavoriteNotFoundException => StatusCodes.Status404NotFound,
                    
                    // Якщо оголошення, яке намагаються додати в обране/видалити з обраного, не існує
                    ListingNotFoundForFavoriteException => StatusCodes.Status400BadRequest,
                    
                    // Якщо користувач, який виконує операцію, не знайдений (менш імовірно, якщо з контексту)
                    UserNotFoundForFavoriteException => StatusCodes.Status400BadRequest,
                    
                    // Якщо користувач вже додав це оголошення в обране
                    UserAlreadyFavoritedListingException => StatusCodes.Status409Conflict,
                    
                    // Якщо користувач намагається видалити з обраного те, що він не додавав (або запису Favorite для оголошення немає)
                    UserHasNotFavoritedListingException => StatusCodes.Status404NotFound, 
                    
                    FavoriteOperationFailedException => StatusCodes.Status500InternalServerError,
                    
                    // Обробка будь-яких інших підтипів FavoriteException
                    _ => throw new NotImplementedException($"Handling for {exception.GetType().Name} is not implemented in FavoriteErrorHandler.")
                }
            };
        }
    }
}
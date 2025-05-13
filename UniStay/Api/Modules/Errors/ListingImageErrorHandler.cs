using Application.ListingImages.Exceptions; // Розташування ваших ListingImageException
using Microsoft.AspNetCore.Http;          // Для StatusCodes
using Microsoft.AspNetCore.Mvc;           // Для ObjectResult
using System;                              // Для NotImplementedException

namespace Api.Modules.Errors // Або ваш бажаний простір імен для API модулів
{
    public static class ListingImageErrorHandler
    {
        public static ObjectResult ToObjectResult(this ListingImageException exception)
        {
            // Тіло відповіді буде містити повідомлення з винятку,
            // обгорнуте в об'єкт для JSON форматування.
            return new ObjectResult(new { Message = exception.Message })
            {
                StatusCode = exception switch
                {
                    ListingImageNotFoundException => StatusCodes.Status404NotFound,
                    
                    // Якщо оголошення не знайдено для прив'язки зображення - це помилка запиту
                    ListingNotFoundForImageException => StatusCodes.Status400BadRequest,
                    
                    // Якщо користувач не має прав на керування зображеннями для цього оголошення
                    UserNotAuthorizedToManageListingImagesException => StatusCodes.Status403Forbidden,
                    
                    ListingImageOperationFailedException => StatusCodes.Status500InternalServerError,
                    
                    // Обробка будь-яких інших підтипів ListingImageException
                    _ => throw new NotImplementedException($"Handling for {exception.GetType().Name} is not implemented in ListingImageErrorHandler.")
                }
            };
        }
    }
}
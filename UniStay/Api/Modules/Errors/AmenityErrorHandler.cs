using Application.Amenities.Exceptions; // Розташування ваших AmenityException
using Microsoft.AspNetCore.Http;      // Для StatusCodes
using Microsoft.AspNetCore.Mvc;       // Для ObjectResult
using System;                          // Для NotImplementedException

namespace Api.Modules.Errors // Або ваш бажаний простір імен для API модулів
{
    public static class AmenityErrorHandler
    {
        public static ObjectResult ToObjectResult(this AmenityException exception)
        {
            // Тіло відповіді буде містити повідомлення з винятку,
            // обгорнуте в об'єкт для JSON форматування.
            return new ObjectResult(new { Message = exception.Message })
            {
                StatusCode = exception switch
                {
                    AmenityNotFoundException => StatusCodes.Status404NotFound,
                    
                    // Якщо зручність з такою назвою вже існує
                    AmenityAlreadyExistsException => StatusCodes.Status409Conflict,
                    
                    AmenityOperationFailedException => StatusCodes.Status500InternalServerError,
                    
                    // Обробка будь-яких інших підтипів AmenityException
                    _ => throw new NotImplementedException($"Handling for {exception.GetType().Name} is not implemented in AmenityErrorHandler.")
                }
            };
        }
    }
}
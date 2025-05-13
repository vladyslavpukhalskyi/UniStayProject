using Application.Reviews.Exceptions; // Розташування ваших ReviewException
using Microsoft.AspNetCore.Http;    // Для StatusCodes
using Microsoft.AspNetCore.Mvc;     // Для ObjectResult
using System;                        // Для NotImplementedException

namespace Api.Modules.Errors // Або ваш бажаний простір імен для API модулів
{
    public static class ReviewErrorHandler
    {
        public static ObjectResult ToObjectResult(this ReviewException exception)
        {
            // За замовчуванням, тіло відповіді буде містити повідомлення з винятку,
            // обгорнуте в об'єкт для кращого JSON форматування.
            return new ObjectResult(new { Message = exception.Message })
            {
                StatusCode = exception switch
                {
                    ReviewNotFoundException => StatusCodes.Status404NotFound,
                    
                    // Якщо запит не може бути оброблений через відсутність оголошення (напр. при створенні відгуку)
                    ListingNotFoundForReviewException => StatusCodes.Status400BadRequest, 
                    
                    // Якщо користувач не має права виконувати дію за бізнес-логікою
                    UserCannotReviewException => StatusCodes.Status403Forbidden, 
                    UserNotAuthorizedForReviewOperationException => StatusCodes.Status403Forbidden,
                    
                    ReviewOperationFailedException => StatusCodes.Status500InternalServerError,
                    
                    // Обробка будь-яких інших підтипів ReviewException, які можуть бути додані пізніше
                    _ => throw new NotImplementedException($"Handling for {exception.GetType().Name} is not implemented in ReviewErrorHandler.")
                }
            };
        }
    }
}
// Файл: Application/Common/Result.cs
using System;

namespace Application.Common
{
    public readonly struct Result<TValue, TError>
    {
        private readonly TValue? _value;
        private readonly TError? _error;

        public bool IsError { get; }
        public bool IsSuccess => !IsError;

        private Result(TValue value)
        {
            IsError = false;
            _value = value;
            _error = default;
        }

        private Result(TError error)
        {
            IsError = true;
            _value = default;
            _error = error;
        }

        public static implicit operator Result<TValue, TError>(TValue value) => new(value);
        public static implicit operator Result<TValue, TError>(TError error) => new(error);

        public TResult Match<TResult>(
            Func<TValue, TResult> success,
            Func<TError, TResult> failure) =>
            IsSuccess ? success(_value!) : failure(_error!);
            
        // Додаткові властивості для прямого доступу (використовувати обережно)
        public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException("Result does not contain a success value.");
        public TError Error => IsError ? _error! : throw new InvalidOperationException("Result does not contain an error value.");
    }
}
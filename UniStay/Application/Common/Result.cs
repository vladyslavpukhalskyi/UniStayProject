using System;

namespace Application.Common 
{
    public class Result<TValue, TError>
    {
        private readonly TValue _value;
        private readonly TError _error;

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        private Result(TValue value)
        {
            IsSuccess = true;
            _value = value;
            _error = default!;
        }

        private Result(TError error)
        {
            IsSuccess = false;
            _value = default!;
            _error = error;
        }

        public static Result<TValue, TError> Success(TValue value) => new(value);
        public static Result<TValue, TError> Failure(TError error) => new(error);

        public TValue Value => IsSuccess
            ? _value
            : throw new InvalidOperationException("Cannot access Value when IsSuccess is false. Check IsSuccess property first.");

        public TError Error => IsFailure
            ? _error
            : throw new InvalidOperationException("Cannot access Error when IsFailure is false. Check IsFailure property first.");

        public TResult Match<TResult>(
            Func<TValue, TResult> onSuccess,
            Func<TError, TResult> onFailure)
        {
            return IsSuccess ? onSuccess(_value) : onFailure(_error);
        }

        public static implicit operator Result<TValue, TError>(TValue value) => Success(value);
        public static implicit operator Result<TValue, TError>(TError error) => Failure(error);
    }
}
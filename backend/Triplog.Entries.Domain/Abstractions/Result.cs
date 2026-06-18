using System;
using System.Collections.Generic;
using System.Text;

namespace Triplog.Entries.Domain.Abstractions
{
    public class Result
    {
        public bool IsSucess { get; }

        public bool IsFailure => !IsSucess;

        public Error Error { get; }

        protected Result(bool isSuccess, Error error) 
        {
            if (isSuccess && error != Error.None)
                throw new InvalidOperationException("A successful result cannot have an error.");
            if (!isSuccess && error == Error.None)
                throw new InvalidOperationException("A failure result must have an error.");

            IsSucess = isSuccess;
            Error = error;
        }

        public static Result Success() => new(true, Error.None);
        public static Result Failure(Error error) => new(false, error);

        public static Result<T> Success<T>(T value) => new(value, true, Error.None);

        public static Result<T> Failure<T>(Error error) => new(default!, false, error);
    }

    public class Result<T> : Result
    {
        public T _value { get; }

        protected internal Result(T value, bool isSuccess, Error error) : base(isSuccess, error)
            => _value = value;

        public T Value => IsSucess
            ? _value
            : throw new InvalidOperationException("Cannot accesss the value of a failed result.");
    }
}

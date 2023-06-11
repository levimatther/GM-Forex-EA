using System;


namespace Commons
{
    [Serializable]
    public class VoidResult<TStatus>
    {
        public VoidResult(TStatus status) { Status = status; }

        public TStatus Status { get; }

        public Exception? Exception { get; set; }
    }

    [Serializable]
    public class Result : VoidResult<bool>
    {
        public static implicit operator bool(Result result) => result.Status;

        public static Result Done() => new Result(true);

        public static Result Fail(Exception? exception = null) => new Result(false) { Exception = exception };

        public Result(bool status) : base(status) { }

        public Result Next(Func<Result> proc) => this ? proc() : this;

        public Result Next(Action proc)
        {
            if (this) proc();
            return this;
        }

        public Result<TNewValue> Next<TNewValue>(Func<Result<TNewValue>> proc) => this ? proc() : Result<TNewValue>.Fail(Exception);

        public Result Else(Func<Exception?, Result> proc) => this ? this : proc(Exception);

        public Result Else(Action<Exception?> proc)
        {
            if (!this) proc(Exception);
            return this;
        }

        public Result Throw(Func<Exception?, Exception?>? proc = null, bool throwAlways = true)
        {
            if (!this)
            {
                var ex = proc == null ? Exception : proc(Exception);
                if (ex != null) throw ex; else if (throwAlways) throw new ApplicationException();
            }
            return this;
        }
    }

    [Serializable]
    public class Result<TValue> : VoidResult<bool>
    {
        public static implicit operator bool(Result<TValue> result) => result.Status;

        public static Result<TValue> Done(TValue value) => new Result<TValue>(true, value);

        public static Result<TValue> Fail(Exception? exception = null) => new Result<TValue>(false, default!) { Exception = exception };

        public Result(bool status, TValue value) : base(status) => Value = value;

        public TValue Value { get; }

        public Result Next(Func<TValue, Result> proc) => this ? proc(Value) : Result.Fail(Exception);

        public Result Next(Action<TValue> proc)
        {
            if (this) proc(Value); 
            return this ? Result.Done() : Result.Fail(Exception);
        }

        public Result<TNewValue> Next<TNewValue>(Func<TValue, Result<TNewValue>> proc) => this ? proc(Value) : Result<TNewValue>.Fail(Exception);

        public Result<TValue> Else(Func<Exception?, Result<TValue>> proc) => this ? this : proc(Exception);

        public Result Else(Func<Exception?, Result> proc) => this ? Result.Done() : proc(Exception);

        public Result<TValue> Else(Action<Exception?> proc)
        {
            if (!this) proc(Exception);
            return this;
        }

        public Result<TValue> Throw(Func<Exception?, Exception?>? proc = null, bool throwAlways = true)
        {
            if (!this)
            {
                var ex = proc == null ? Exception : proc(Exception);
                if (ex != null) throw ex; else if (throwAlways) throw new ApplicationException();
            }
            return this;
        }
    }

    [Serializable]
    public class Result<TStatus, TValue> : VoidResult<TStatus>
    {
        public static Result<TStatus, TValue> Done(TStatus status, TValue value) => new Result<TStatus, TValue>(status, value);

        public static Result<TStatus, TValue> Fail(TStatus status, Exception? exception = null) =>
            new Result<TStatus, TValue>(status, default!) { Exception = exception };

        public Result(TStatus status, TValue value) : base(status) => Value = value;

        public TValue Value { get; }
    }

    public static class ResultExtensions
    {
        public static Result<TValue> IfValueExists<TValue>(this Result<TValue?> result) where TValue : class =>
            result.Value != null ? Result<TValue>.Done(result.Value) : Result<TValue>.Fail();
    }
}

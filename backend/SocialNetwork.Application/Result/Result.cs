namespace SocialNetwork.Application.Result
{
    public abstract record Result
    {
        public sealed record Success : Result;
        public sealed record Failure(string Error, int StatusCode = 400) : Result;
    }

    public abstract record Result<T>
    {
        public sealed record Success(T Value) : Result<T>;
        public sealed record Failure(string Error, int StatusCode = 400) : Result<T>;
    }
}
namespace ProjectCore.Shared.Exceptions
{
    public class NotFoundErrorResult : ErrorResult
    {
        public NotFoundErrorResult(string message) : base(message)
        {
        }

        public NotFoundErrorResult(string message, IReadOnlyCollection<Error> errors) : base(message, errors)
        {
        }
    }
}

namespace API.Exceptions
{
    public record ApiException
    {
        public ApiException(int statusCode, string message, string details)
        {
            StatusCode = statusCode;
            Message = message;
            Details = details;
        }

        public int StatusCode { get; init; }
        public string Message { get; init; }
        public string Details { get; init; }
    }
}
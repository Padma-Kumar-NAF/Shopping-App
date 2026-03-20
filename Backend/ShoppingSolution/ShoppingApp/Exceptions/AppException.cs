namespace ShoppingApp.Exceptions
{
    public class AppException : Exception
    {
        public int StatusCode { get; }

        public AppException(string message, int statusCode = 500)
            : base(message)
        {
            StatusCode = statusCode;
        }
        public AppException(string message, Exception innerException , int statusCode = 500)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
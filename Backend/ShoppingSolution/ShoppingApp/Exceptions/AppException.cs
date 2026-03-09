namespace ShoppingApp.Exceptions
{
    public class AppException : Exception
    {
        public int StatusCode { get; }
        public AppException(string message,int statusCode = 500)
        {
            StatusCode = statusCode;
        }
    }
}

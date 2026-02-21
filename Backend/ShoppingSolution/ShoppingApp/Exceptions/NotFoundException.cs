namespace ShoppingApp.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException() : base("Unauthorized access.")
        {

        }

        public NotFoundException(string message) : base(message)
        {

        }
    }
}

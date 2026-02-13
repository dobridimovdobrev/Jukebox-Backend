namespace Jukebox_Backend.Exceptions
{
    public class BusinessException : Exception
    {
        public BusinessException(string errorCode) : base(errorCode) { }
    }
}

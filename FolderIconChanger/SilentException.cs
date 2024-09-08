
namespace FolderIconChanger
{
    [Serializable]
    internal class SilentException : Exception
    {
        public SilentException()
        {
        }

        public SilentException(string? message) : base(message)
        {
        }

        public SilentException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
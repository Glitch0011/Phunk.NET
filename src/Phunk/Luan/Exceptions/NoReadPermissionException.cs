namespace Phunk.Luan.Exceptions
{
	public class NoReadPermissionException : LuanException
	{
		public override string Message { get; }

        public NoReadPermissionException(string message)
        {
            Message = message;
        }

        public override string ToString()
        {
            return Message;
        }
    }
}
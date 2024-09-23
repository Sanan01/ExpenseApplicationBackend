using System.Runtime.Serialization;

namespace ExpenseApplication.Exceptions
{
	[Serializable]
	public class TokenServiceException : Exception
	{
		public TokenServiceException() { }
		public TokenServiceException(string message) : base(message) { }
		public TokenServiceException(string message, Exception inner) : base(message, inner) { }
		protected TokenServiceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}

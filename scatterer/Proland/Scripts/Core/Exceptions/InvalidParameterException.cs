
using System;

namespace scatterer
{
	
	public class InvalidParameterException : ProlandException
	{
		public InvalidParameterException()
		{
			
		}
		
		public InvalidParameterException(string message)
			: base(message)
		{
			
		}
		
		public InvalidParameterException(string message, Exception inner)
			: base(message, inner)
		{
			
		}
	}
	
}


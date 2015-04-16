
using System;

namespace scatterer
{

	public class ProlandException : Exception
	{
		public ProlandException()
		{

		}

		public ProlandException(string message)
		: base(message)
		{

		}
		
		public ProlandException(string message, Exception inner)
		: base(message, inner)
		{

		}
	}

}

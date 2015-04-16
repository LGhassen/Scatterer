
using System;

namespace scatterer
{
	
	public class CacheCapacityException : ProlandException
	{
		public CacheCapacityException()
		{
			
		}
		
		public CacheCapacityException(string message)
			: base(message)
		{
			
		}
		
		public CacheCapacityException(string message, Exception inner)
			: base(message, inner)
		{
			
		}
	}
	
}

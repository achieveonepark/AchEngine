using System;

namespace AchEngine
{

	public static class IDisposableExt
	{

		public static void DisposeIfNotNull(this IDisposable self)
		{
			if (self == null) return;
			self.Dispose();
		}
	}
}
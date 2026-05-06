namespace AchEngine
{

	public static class ObjectExt
	{
		public static string FormatWithComma(this object self)
		{
			return string.Format("{0:#,##0}", self);
		}
	}
}
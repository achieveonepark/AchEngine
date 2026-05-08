using Newtonsoft.Json;

namespace AchEngine
{
	public interface IJson
	{
	}

	public static class IJsonExt
	{
		public static string ToReadableJson( this IJson self )
		{
			return JsonConvert.SerializeObject( self, Formatting.Indented );
		}

		public static string ToJson( this IJson self )
		{
			return JsonConvert.SerializeObject( self );
		}
	}
}
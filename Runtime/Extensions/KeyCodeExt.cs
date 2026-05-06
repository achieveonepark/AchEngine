using UnityEngine;

namespace AchEngine
{
	public static class KeyCodeExt
	{
		public static bool GetKey(this KeyCode self)
		{
			return Input.GetKey(self);
		}

		public static bool GetKeyDown(this KeyCode self)
		{
			return Input.GetKeyDown(self);
		}
	}
}
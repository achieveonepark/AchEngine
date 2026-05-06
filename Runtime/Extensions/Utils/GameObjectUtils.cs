using UnityEngine;

namespace AchEngine
{

	public static class GameObjectUtils
	{

		public static bool Exists(string name)
		{
			return GameObject.Find(name);
		}
	}
}
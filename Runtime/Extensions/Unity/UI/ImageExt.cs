using UnityEngine;
using UnityEngine.UI;

namespace AchEngine
{

	public static class ImageExt
	{

		public static void SetSpriteAndSnap(this Image self, Sprite sprite)
		{
			self.sprite = sprite;
			self.SetNativeSize();
		}
	}
}
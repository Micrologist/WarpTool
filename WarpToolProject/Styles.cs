using UnityEngine;

namespace WarpTool
{
	internal static class Styles
	{
		public static GUISkin MainSkin { get; private set; }
		public static GUIStyle WindowStyle { get; private set; }
		public static GUIStyle CloseBtnStyle { get; private set; }
		public static GUIStyle ScrollViewStyle { get; private set; }
		public static GUIStyle ScrollViewButtonStyle { get; private set; }

		public static void Initialize(GUISkin baseSkin, float windowWidth)
		{
			MainSkin = baseSkin;

			WindowStyle = new GUIStyle(MainSkin.window)
			{
				padding = new RectOffset(8, 8, 20, 8),
				contentOffset = new Vector2(0, -22),
				fixedWidth = windowWidth
			};

			CloseBtnStyle = new GUIStyle(MainSkin.button)
			{
				fontSize = 8
			};

			ScrollViewStyle = new GUIStyle(MainSkin.scrollView)
			{
				border = new RectOffset(0, 0, 0, 0),
				fixedWidth = 138,
				margin = new RectOffset(4, 4, 4, 4)
			};

			ScrollViewButtonStyle = new GUIStyle(MainSkin.button)
			{
				margin = new RectOffset(0, 0, 0, 0)
			};
		}
	}
}

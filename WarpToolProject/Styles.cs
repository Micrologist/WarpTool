using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace WarpTool
{
	internal static class Styles
	{
		public static GUISkin mainSkin;
		public static GUIStyle windowStyle;
		public static GUIStyle closeBtnStyle;
		public static GUIStyle scrollViewStyle;
		public static GUIStyle scrollViewButtonStyle;

		public static void Initialize(GUISkin baseSkin, float windowWidth)
		{
			mainSkin = baseSkin;

			windowStyle = new GUIStyle(mainSkin.window)
			{
				padding = new RectOffset(8, 8, 20, 8),
				contentOffset = new Vector2(0, -22),
				fixedWidth = windowWidth
			};

			closeBtnStyle = new GUIStyle(mainSkin.button)
			{
				fontSize = 8
			};

			scrollViewStyle = new GUIStyle(mainSkin.scrollView)
			{
				border = new RectOffset(0, 0, 0, 0),
				fixedWidth = 138,
				margin = new RectOffset(4, 4, 4, 4)
			};

			scrollViewButtonStyle = new GUIStyle(mainSkin.button)
			{
				margin = new RectOffset(0, 0, 0, 0)
			};
		}
	}
}

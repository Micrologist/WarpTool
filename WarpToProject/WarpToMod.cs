using BepInEx;
using SpaceWarp;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Mods;
using SpaceWarp.API.UI;
using SpaceWarp.API.UI.Appbar;
using UnityEngine;

namespace WarpTo
{
	[BepInPlugin("com.micrologist.warpto", "WarpTo", "0.0.1")]
	[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
	public class WarpToMod : BaseSpaceWarpPlugin
	{
		private GUISkin skin;
		private GUIStyle windowStyle;
		private bool showGUI;
		private Rect mainGuiRect;

		public override void OnInitialized()
		{
			Logger.LogInfo("Warp to loaded");
			skin = Skins.ConsoleSkin;

			windowStyle = new GUIStyle(skin.window)
			{
				padding = new RectOffset(8, 8, 20, 8),
				contentOffset = new Vector2(0, -22),
				fixedWidth = 250
			};

			mainGuiRect = new(Screen.width * 0.5f, Screen.height * 0.2f, 0, 0);

			Appbar.RegisterAppButton(
					"Warp To",
					"BTN-WarpToBtn",
					AssetManager.GetAsset<Texture2D>($"{SpaceWarpMetadata.ModID}/images/icon.png"),
					delegate { showGUI = !showGUI; }
			);
		}

		private void OnGUI()
		{
			if (!showGUI) return;

			mainGuiRect = GUILayout.Window(
				GUIUtility.GetControlID(FocusType.Passive),
				mainGuiRect,
				FillGUI,
				"<color=#696DFF>// WARP TO</color>",
				windowStyle,
				GUILayout.Height(0)
			);
		}

		private void FillGUI(int id)
		{
			
		}
	}
}

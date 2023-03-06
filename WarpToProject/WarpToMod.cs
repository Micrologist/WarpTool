using BepInEx;
using HarmonyLib;
using KSP.Game;
using KSP.Messages.PropertyWatchers;
using KSP.Sim.impl;
using KSP.UI.Binding;
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
		private GUIStyle closeBtnStyle;
		private GUIStyle scrollViewStyle;
		private bool showGUI;
		private Rect mainGuiRect;
		private Rect closeBtnRect;
		private VesselComponent activeVessel;
		private readonly float windowWidth = 250;
		private Vector2 targetScrollPosition = new();

		private List<string> warpTargetStrings = new()
		{
			"Apoapsis",
			"Periapsis",
			"Maneuver Node",
			"SOI Transition"
		};

		private string warpTarget = "Apoapsis";
		private bool isSelectingWarpTarget;

		public override void OnInitialized()
		{
			skin = Skins.ConsoleSkin;

			windowStyle = new GUIStyle(skin.window)
			{
				padding = new RectOffset(8, 8, 20, 8),
				contentOffset = new Vector2(0, -22),
				fixedWidth = windowWidth
			};

			closeBtnStyle = new GUIStyle(skin.button)
			{
				fontSize = 8
			};

			mainGuiRect = new(Screen.width * 0.5f, Screen.height * 0.2f, 0, 0);
			closeBtnRect = new Rect(windowWidth - 23, 6, 16, 16);

			scrollViewStyle = new GUIStyle(skin.scrollView)
			{
				border = new(0,0,0,0),
				fixedWidth = 138
			};

			Appbar.RegisterAppButton(
					"Warp To",
					"BTN-WarpToBtn",
					AssetManager.GetAsset<Texture2D>($"{SpaceWarpMetadata.ModID}/images/icon.png"),
					delegate { showGUI = !showGUI; }
			);
		}

		private void Start()
		{
			new Harmony("warptopatch").PatchAll();
		}

		private void OnGUI()
		{
			activeVessel = GameManager.Instance?.Game?.ViewController?.GetActiveVehicle(true)?.GetSimVessel(true);
			if (!showGUI || activeVessel == null) return;

			GUI.skin = skin;
			mainGuiRect = GUILayout.Window(
				GUIUtility.GetControlID(FocusType.Passive),
				mainGuiRect,
				FillGUI,
				"<color=#696DFF>// WARP TO</color>",
				windowStyle,
				GUILayout.Height(300)
			);
			mainGuiRect.position = ClampToScreen(mainGuiRect.position, mainGuiRect.size);


		}


		private void FillGUI(int id)
		{
			if (CloseButton())
			{
				CloseWindow();
			}
			GUILayout.Space(10);
			GUILayout.BeginHorizontal();
			GUILayout.Label("Warp to Apoapsis");
			if (GUILayout.Button("Click me")) WarpToApoapsis();
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Warp to ");
			if (isSelectingWarpTarget)
			{
				GUILayout.BeginVertical(scrollViewStyle);
				targetScrollPosition = GUILayout.BeginScrollView(targetScrollPosition, false, false, GUILayout.Width(138));
				foreach (string targetName in warpTargetStrings)
				{
					if (GUILayout.Button(targetName, GUILayout.Width(130)))
					{
						warpTarget = targetName;
						isSelectingWarpTarget = false;
					}
				}
				GUILayout.EndScrollView();
				GUILayout.EndVertical();
			}
			else
			{
				isSelectingWarpTarget = GUILayout.Button(warpTarget);
				GUILayout.Space(1);
			}
			GUILayout.EndHorizontal();


			GUI.DragWindow(new Rect(0, 0, 1000, 1000));
		}

		private void WarpToApoapsis()
		{
			double timeToApoapsis = activeVessel.Orbit.TimeToAp;
			double warpTime = GameManager.Instance.Game.UniverseModel.UniversalTime + timeToApoapsis;
			GameManager.Instance.Game.ViewController.TimeWarp.WarpTo(warpTime);
		}

		private Vector2 ClampToScreen(Vector2 position, Vector2 size)
		{
			float x = Mathf.Clamp(position.x, 0, Screen.width - size.x);
			float y = Mathf.Clamp(position.y, 0, Screen.height - size.y);
			return new Vector2(x, y);
		}

		private bool CloseButton()
		{
			return GUI.Button(closeBtnRect, "x", closeBtnStyle);
		}

		private void CloseWindow()
		{
			GameObject.Find("BTN-WarpToBtn")?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(false);
			showGUI = false;
		}


	}


	[HarmonyPatch(typeof(TimeWarp))]
	[HarmonyPatch("GetMaxRateIndex")]
	class MaxTimeWarpPatch
	{
		static void Postfix(ref int __result)
		{
			VesselComponent activeVessel = GameManager.Instance?.Game?.ViewController?.GetActiveVehicle(true)?.GetSimVessel(true);
			if(activeVessel != null && !activeVessel.IsInAtmosphere)
			{
				__result = PhysicsSettings.TimeWarpLevels.Length - 1;
			}
		}
	}
}

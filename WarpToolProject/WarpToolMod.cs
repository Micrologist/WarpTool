using BepInEx;
using HarmonyLib;
using KSP.Game;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;
using KSP.UI.Binding;
using SpaceWarp;
using SpaceWarp.API.Assets;
using SpaceWarp.API.Mods;
using SpaceWarp.API.UI;
using SpaceWarp.API.UI.Appbar;
using UnityEngine;

namespace WarpTool
{
	[BepInPlugin("com.micrologist.warptool", "WarpTool", "0.1.0")]
	[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
	public class WarpToolMod : BaseSpaceWarpPlugin
	{
		private bool showGUI;
		private readonly float windowWidth = 250;
		private Rect mainGuiRect;
		private Rect closeBtnRect;
		private Vector2 targetScrollPosition = new();

		private readonly List<string> warpTargetStrings = new()
		{
			"Apoapsis",
			"Periapsis",
			"Maneuver Node",
			"SOI Transition",
			"Orbit AN",
			"Orbit DN",
			"Target AN",
			"Target DN",
			"Closest Appr."
		};

		private string warpTarget = "Apoapsis";
		private bool isSelectingWarpTarget;
		private double warpTargetUT = 0;
		private double warpTargetTimeTo = 0;
		private int leadTimeInput = 1;
		private double leadTime;


		public override void OnInitialized()
		{
			Styles.Initialize(Skins.ConsoleSkin, windowWidth);

			mainGuiRect = new Rect(Screen.width * 0.5f, Screen.height * 0.2f, 0, 0);
			closeBtnRect = new Rect(windowWidth - 23, 6, 16, 16);

			Appbar.RegisterAppButton(
					"Warp Tool",
					"BTN-WarpToBtn",
					AssetManager.GetAsset<Texture2D>($"{SpaceWarpMetadata.ModID}/images/icon.png"),
					delegate { showGUI = !showGUI; }
			);
		}

		private void Start()
		{
			new Harmony("warptoolpatch").PatchAll();
		}

		private void Update()
		{
			GameData.UpdateState();
			if (!GameData.IsValid)
			{
				return;
			}
			UpdateTimes();
			AutoWarper.Update();
		}

		private void OnGUI()
		{
			if (!showGUI || !GameData.IsValid) return;

			GUI.skin = Styles.mainSkin;
			mainGuiRect = GUILayout.Window(
				GUIUtility.GetControlID(FocusType.Passive),
				mainGuiRect,
				FillGUI,
				"<color=#696DFF>// WARP TOOL</color>",
				Styles.windowStyle,
				GUILayout.Height(0)
			);
			Utilities.ClampWindowToScreen(ref mainGuiRect);
		}


		private void FillGUI(int id)
		{
			if (CloseButton())
			{
				CloseWindow();
			}

			GUILayout.Space(10);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Target");

			GUILayout.BeginVertical(Styles.scrollViewStyle);
			GUILayout.BeginScrollView(Vector2.zero, false, false, isSelectingWarpTarget ? GUILayout.Height(28 * warpTargetStrings.Count) : GUILayout.Height(28));
			if (isSelectingWarpTarget)
			{
				foreach (string targetName in warpTargetStrings)
				{
					if (GUILayout.Button(targetName, Styles.scrollViewButtonStyle))
					{
						warpTarget = targetName;
						isSelectingWarpTarget = false;
					}
				}
			}
			else
			{
				isSelectingWarpTarget = GUILayout.Button(warpTarget, Styles.scrollViewButtonStyle);
			}
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();

			GUILayout.Space(-5);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Time to");
			GUILayout.FlexibleSpace();
			GUILayout.Label(Utilities.SecondsToTimeString(warpTargetTimeTo, false) + "s");
			GUILayout.EndHorizontal();

			GUILayout.Space(-5);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Lead time");
			GUILayout.FlexibleSpace();
			GUILayout.Label($"{Utilities.SecondsToTimeString(leadTime, false)}s");
			GUILayout.EndHorizontal();

			
			GUILayout.BeginHorizontal();
			leadTimeInput = (int)GUILayout.HorizontalSlider(leadTimeInput, 0, 60);
			GUILayout.EndHorizontal();

			GUILayout.Space(10);

			GUILayout.BeginHorizontal();
			if (!AutoWarper.IsWarping)
			{
				if (GUILayout.Button("Warp"))
					AutoWarper.StartWarp(warpTargetUT - leadTime);
			}
			else
			{
				if (GUILayout.Button("Cancel"))
					AutoWarper.StopWarp();
			}
			GUILayout.EndHorizontal();

			GUI.DragWindow(new Rect(0, 0, 1000, 1000));
		}


		private void UpdateTimes()
		{
			warpTargetUT = warpTarget switch
			{
				"Apoapsis" => GameData.UT + GameData.ActiveVessel.Orbit.TimeToAp,
				"Periapsis" => GameData.UT + GameData.ActiveVessel.Orbit.TimeToPe,
				"Maneuver Node" => GameData.ManeuverNode != null ? GameData.ManeuverNode.Time : GameData.UT,
				"SOI Transition" => GameData.CurrentOrbit.UniversalTimeAtSoiEncounter,
				"Orbit AN" => GameData.UT + Utilities.GetTimeToAscendingNode(GameData.CurrentOrbit),
				"Orbit DN" => GameData.UT + Utilities.GetTimeToDescendingNode(GameData.CurrentOrbit),
				"Target AN" => GameData.Targeter.HasValidity ? GameData.Targeter.AscendingNodeOrbiter.UniversalTime : 0,
				"Target DN" => GameData.Targeter.HasValidity ? GameData.Targeter.DescendingNodeOrbiter.UniversalTime : 0,
				"Closest Approach" => GameData.Targeter.HasValidity ? GameData.Targeter.CloseApproachMarkerOrbiter.UniversalTime : 0,
				_ => GameData.UT,
			};
			warpTargetTimeTo = warpTargetUT - GameData.UT;

			if(warpTargetTimeTo < 0)
			{
				warpTargetTimeTo = 0;
				warpTargetUT = GameData.UT;
			}

			leadTime = leadTimeInput * 10;
		}

		private bool CloseButton()
		{
			return GUI.Button(closeBtnRect, "x", Styles.closeBtnStyle);
		}

		private void CloseWindow()
		{
			GameObject.Find("BTN-WarpToBtn")?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(false);
			showGUI = false;
		}
	}
}

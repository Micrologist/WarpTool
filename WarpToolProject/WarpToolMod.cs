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
	[BepInPlugin("com.micrologist.warptool", "WarpTool", "0.0.1")]
	[BepInDependency(SpaceWarpPlugin.ModGuid, SpaceWarpPlugin.ModVer)]
	public class WarpToolMod : BaseSpaceWarpPlugin
	{
		private GUISkin skin;
		private GUIStyle windowStyle;
		private GUIStyle closeBtnStyle;
		private GUIStyle scrollViewStyle;
		private GUIStyle scrollViewButtonStyle;
		private GUIStyle textFieldStyle;
		private bool showGUI;
		private Rect mainGuiRect;
		private Rect closeBtnRect;
		private double currentUT;
		private VesselComponent activeVessel;
		private readonly float windowWidth = 250;
		private Vector2 targetScrollPosition = new();

		private double warpTargetUT = 0;
		private double warpTargetTimeTo = 0;
		private int leadTimeInput = 1;
		private double leadTime;

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
		private ManeuverNodeData currentManeuver;

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

			mainGuiRect = new Rect(Screen.width * 0.5f, Screen.height * 0.2f, 0, 0);
			closeBtnRect = new Rect(windowWidth - 23, 6, 16, 16);

			scrollViewStyle = new GUIStyle(skin.scrollView)
			{
				border = new RectOffset(0, 0, 0, 0),
				fixedWidth = 138,
				margin = new RectOffset(4, 4, 4, 4)
			};

			scrollViewButtonStyle = new GUIStyle(skin.button)
			{
				margin = new RectOffset(0, 0, 0, 0)
			};

			textFieldStyle = new GUIStyle(skin.textField)
			{
				fixedWidth = 70,
				alignment = TextAnchor.MiddleRight,
				contentOffset = new(-4, 0),
			};

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

		private void OnGUI()
		{
			GetVesselManeuverAndTime();
			if (!showGUI || activeVessel == null) return;

			UpdateTimeToSelected();

			GUI.skin = skin;
			mainGuiRect = GUILayout.Window(
				GUIUtility.GetControlID(FocusType.Passive),
				mainGuiRect,
				FillGUI,
				"<color=#696DFF>// WARP TOOL</color>",
				windowStyle,
				GUILayout.Height(0)
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
			GUILayout.Label("Target");

			GUILayout.BeginVertical(scrollViewStyle);
			targetScrollPosition = GUILayout.BeginScrollView(targetScrollPosition, false, false, isSelectingWarpTarget ? GUILayout.Height(28 * warpTargetStrings.Count) : GUILayout.Height(28));
			if (isSelectingWarpTarget)
			{
				foreach (string targetName in warpTargetStrings)
				{
					if (GUILayout.Button(targetName, scrollViewButtonStyle))
					{
						warpTarget = targetName;
						isSelectingWarpTarget = false;
					}
				}
			}
			else
			{
				isSelectingWarpTarget = GUILayout.Button(warpTarget, scrollViewButtonStyle);
			}
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();

			GUILayout.Space(-5);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Time to");
			GUILayout.FlexibleSpace();
			GUILayout.Label(SecondsToTimeString(warpTargetTimeTo, false) + "s");
			GUILayout.EndHorizontal();

			GUILayout.Space(-5);

			GUILayout.BeginHorizontal();
			GUILayout.Label("Lead time");
			GUILayout.FlexibleSpace();
			GUILayout.Label($"{SecondsToTimeString(leadTime, false)}s");
			GUILayout.EndHorizontal();

			
			GUILayout.BeginHorizontal();
			leadTimeInput = (int)GUILayout.HorizontalSlider(leadTimeInput, 0, 20);
			leadTime = leadTimeInput / 2.0 * 60;
			GUILayout.EndHorizontal();

			GUILayout.Space(10);

			GUILayout.BeginHorizontal();
			if (GUILayout.Button("Warp"))
				Warp();
			GUILayout.EndHorizontal();

			GUI.DragWindow(new Rect(0, 0, 1000, 1000));
		}

		private void Warp()
		{
			UpdateTimeToSelected();
			if(warpTargetTimeTo - leadTime > 10)
			{
				var timeWarp = GameManager.Instance.Game.ViewController.TimeWarp;
				timeWarp.WarpTo(warpTargetUT - leadTime);
			}
		}

		private void UpdateTimeToSelected()
		{
			warpTargetUT = warpTarget switch
			{
				"Apoapsis" => currentUT + activeVessel.Orbit.TimeToAp,
				"Periapsis" => currentUT + activeVessel.Orbit.TimeToPe,
				"Maneuver Node" => currentManeuver != null ? currentManeuver.Time : currentUT,
				"SOI Transition" => activeVessel.Orbit.UniversalTimeAtSoiEncounter,
				"Orbit AN" => currentUT + GetTimeToAscendingNode(),
				"Orbit DN" => currentUT + GetTimeToDescendingNode(),
				"Target AN" => activeVessel.Orbiter.OrbitTargeter.HasValidity ? activeVessel.Orbiter.OrbitTargeter.AscendingNodeOrbiter.UniversalTime : 0,
				"Target DN" => activeVessel.Orbiter.OrbitTargeter.HasValidity ? activeVessel.Orbiter.OrbitTargeter.DescendingNodeOrbiter.UniversalTime : 0,
				"Closest Approach" => activeVessel.Orbiter.OrbitTargeter.HasValidity ? activeVessel.Orbiter.OrbitTargeter.CloseApproachMarkerOrbiter.UniversalTime : 0,
				_ => currentUT,
			};
			warpTargetTimeTo = warpTargetUT - currentUT;

			if(warpTargetTimeTo < 0)
			{
				warpTargetTimeTo = 0;
				warpTargetUT = currentUT;
			}
		}


		private double GetTimeToAscendingNode()
		{
			PatchedConicsOrbit orbit = activeVessel.Orbit;
			double time = orbit.GetDTforTrueAnomaly((360 - orbit.argumentOfPeriapsis) * Mathf.Deg2Rad, 0) % orbit.period;
			return time > 0 ? time : time + orbit.period;
		}

		private double GetTimeToDescendingNode()
		{
			PatchedConicsOrbit orbit = activeVessel.Orbit;
			double time = orbit.GetDTforTrueAnomaly((180 - orbit.argumentOfPeriapsis) * Mathf.Deg2Rad, 0) % orbit.period;
			return time > 0 ? time : time + orbit.period;
		}

		private void GetVesselManeuverAndTime()
		{
			var game = GameManager.Instance?.Game;
			if (game != null)
			{
				currentUT = game.UniverseModel?.UniversalTime ?? 0;
				activeVessel = game.ViewController?.GetActiveVehicle(true)?.GetSimVessel(true);
				if (activeVessel != null)
					currentManeuver = game.SpaceSimulation.Maneuvers.GetNodesForVessel(activeVessel.GlobalId).FirstOrDefault();
			}
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

		private string SecondsToTimeString(double seconds, bool addSpacing = true)
		{
			if (seconds == Double.PositiveInfinity)
			{
				return "∞";
			}
			else if (seconds == Double.NegativeInfinity)
			{
				return "-∞";
			}

			seconds = Math.Ceiling(seconds);

			string result = "";
			string spacing = "";
			if (addSpacing)
			{
				spacing = " ";
			}

			if (seconds < 0)
			{
				result += "-";
				seconds = Math.Abs(seconds);
			}

			int days = (int)(seconds / 21600);
			int hours = (int)((seconds - (days * 21600)) / 3600);
			int minutes = (int)((seconds - (hours * 3600) - (days * 21600)) / 60);
			int secs = (int)(seconds - (days * 21600) - (hours * 3600) - (minutes * 60));

			if (days > 0)
			{
				result += $"{days}{spacing}d ";
			}

			if (hours > 0 || days > 0)
			{
				{
					result += $"{hours}{spacing}h ";
				}
			}

			if (minutes > 0 || hours > 0 || days > 0)
			{
				if (hours > 0 || days > 0)
				{
					result += $"{minutes:00.}{spacing}m ";
				}
				else
				{
					result += $"{minutes}{spacing}m ";
				}
			}

			if (minutes > 0 || hours > 0 || days > 0)
			{
				result += $"{secs:00.}";
			}
			else
			{
				result += secs;
			}

			return result;

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
				__result = PhysicsSettings.TimeWarpLevels.Length - 2;
			}
		}
	}
}

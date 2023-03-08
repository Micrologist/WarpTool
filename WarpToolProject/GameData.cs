using KSP.Game;
using KSP.Sim.impl;
using KSP.Sim.Maneuver;

namespace WarpTool
{
	internal static class GameData
	{
		public static bool IsValid { get; private set; }
		public static double UT { get; private set; }

		public static double previousUT { get; private set; }
		public static VesselComponent ActiveVessel { get; private set; }
		public static PatchedConicsOrbit CurrentOrbit { get; private set; }
		public static ManeuverNodeData ManeuverNode { get; private set; }
		public static TimeWarp TimeWarp { get; private set; }
		public static OrbitTargeter Targeter { get; private set; }

		public static void UpdateState()
		{
			IsValid = false;
			var game = GameManager.Instance?.Game;

			if (game == null)
				return;

			ActiveVessel = game.ViewController?.GetActiveVehicle(true)?.GetSimVessel(true);
			if (ActiveVessel == null)
				return;

			ManeuverNode = game.SpaceSimulation.Maneuvers.GetNodesForVessel(ActiveVessel.GlobalId).FirstOrDefault();
			TimeWarp = game.ViewController.TimeWarp;
			CurrentOrbit = ActiveVessel.Orbit;
			Targeter = ActiveVessel.Orbiter.OrbitTargeter;
			previousUT = UT;
			UT = game.UniverseModel?.UniversalTime ?? 0;
			IsValid = (TimeWarp != null && CurrentOrbit != null && Targeter != null);
		}
	}
}

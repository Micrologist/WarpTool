using HarmonyLib;
using KSP.Game;
using KSP.Sim.impl;

namespace WarpTool
{
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

using KSP.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace WarpTool
{
	internal static class AutoWarper
	{
		
		public static bool IsWarping { get; private set; }
		private static double targetUT;

		public static void Update()
		{
			if(IsWarping && GameData.UT < GameData.previousUT)
			{
				StopWarp();
			}

			if (IsWarping && !GameData.TimeWarp.IsWarping && targetUT - GameData.UT > 10)
			{
				GameData.TimeWarp.CancelAutoWarp();
				GameData.TimeWarp.WarpTo(targetUT);
			}
			else if (IsWarping && targetUT - GameData.UT <= 1)
			{
				StopWarp();
			}
		}

		public static void StartWarp(double newTargetUT)
		{
			GameData.TimeWarp.CancelAutoWarp();
			GameData.TimeWarp.StopTimeWarp(true);
			targetUT = newTargetUT;
			IsWarping = true;
		}

		public static void StopWarp()
		{
			GameData.TimeWarp.CancelAutoWarp();
			GameData.TimeWarp.StopTimeWarp(true);
			IsWarping = false;
		}
	}
}

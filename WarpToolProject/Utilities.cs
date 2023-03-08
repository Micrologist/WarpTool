using UnityEngine;
using KSP.Sim.impl;

namespace WarpTool
{
	internal static class Utilities
	{
		public static void ClampWindowToScreen(ref Rect window)
		{
			var x = Mathf.Clamp(window.position.x, 0, Screen.width - window.size.x);
			var y = Mathf.Clamp(window.position.y, 0, Screen.height - window.size.y);
			window.position = new Vector2(x, y);
		}

		public static double GetTimeToAscendingNode(PatchedConicsOrbit orbit)
		{
			double time = orbit.GetDTforTrueAnomaly((360 - orbit.argumentOfPeriapsis) * Mathf.Deg2Rad, 0) % orbit.period;
			return time > 0 ? time : time + orbit.period;
		}

		public static double GetTimeToDescendingNode(PatchedConicsOrbit orbit)
		{
			double time = orbit.GetDTforTrueAnomaly((180 - orbit.argumentOfPeriapsis) * Mathf.Deg2Rad, 0) % orbit.period;
			return time > 0 ? time : time + orbit.period;
		}

		public static string SecondsToTimeString(double seconds, bool addSpacing = true)
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
}

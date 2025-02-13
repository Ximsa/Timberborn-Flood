using HarmonyLib;
using Timberborn.SkySystem;

namespace Timberborn.FloodSeason
{
  [HarmonyPatch(typeof(Sun))]
  internal class SunPatch
  {
    [HarmonyFinalizer]
    [HarmonyPatch("GetFogSettings")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality",
                                                     "IDE0051:Nicht verwendete private Member entfernen",
                                                     Justification = "Harmony")]
    private static void Prefix(ref string hazardousWeatherId)
    {
      if (hazardousWeatherId == "FloodWeather")
      {
        hazardousWeatherId = "";
      }
    }
  }
}
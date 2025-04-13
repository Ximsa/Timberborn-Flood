using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using JetBrains.Annotations;
using Timberborn.SkySystem;

namespace Timberborn.FloodSeason
{
  [HarmonyPatch(typeof(Sun))]
  [UsedImplicitly]
  internal class SunPatch
  {
    [HarmonyFinalizer]
    [HarmonyPatch("GetFogSettings")]
    [SuppressMessage("CodeQuality",
                     "IDE0051:Nicht verwendete private Member entfernen",
                     Justification = "Harmony")]
    private static void Prefix(ref string hazardousWeatherId)
    {
      if (hazardousWeatherId == "FloodWeather") hazardousWeatherId = "";
    }
  }
}
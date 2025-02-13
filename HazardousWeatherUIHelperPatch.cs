using HarmonyLib;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Timberborn.HazardousWeatherSystemUI;

namespace Timberborn.FloodSeason
{
  [HarmonyPatch(typeof(HazardousWeatherUIHelper))]
  [SuppressMessage("ReSharper", "InconsistentNaming")]
  internal class HazardousWeatherUIHelperPatch
  {
    [HarmonyFinalizer]
    [HarmonyPatch("UpdateCurrentUISpecification")]
    [SuppressMessage("CodeQuality",
                     "IDE0051:Nicht verwendete private Member entfernen",
                     Justification = "Harmony")]
    private static Exception Finalizer(Exception __exception, HazardousWeatherUIHelper __instance)
    {
      if (__exception is InvalidOperationException)
      {
        __instance._currentUISpecification = __instance._badtideWeatherUISpecification;
      }

      return null;
    }
  }
}
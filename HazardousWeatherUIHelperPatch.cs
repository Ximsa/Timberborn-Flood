
using HarmonyLib;
using System;
using System.Reflection;
using Timberborn.HazardousWeatherSystemUI;

namespace Timberborn_FloodSeason
{
    [HarmonyPatch(typeof(HazardousWeatherUIHelper))]
    class HazardousWeatherUIHelperPatch
    {
        [HarmonyFinalizer]
        [HarmonyPatch("UpdateCurrentUISpecification")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Nicht verwendete private Member entfernen", Justification = "Harmony")]
        static Exception Finalizer(Exception __exception, HazardousWeatherUIHelper __instance)
        {
            if(__exception is InvalidOperationException) 
            {
                __instance
                .GetType()
                .GetField("_currentUISpecification", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(__instance, __instance
                                          .GetType()
                                          .GetField("_badtideWeatherUISpecification", BindingFlags.NonPublic | BindingFlags.Instance)
                                          .GetValue(__instance));
            }
            return null;
        }
    }
}

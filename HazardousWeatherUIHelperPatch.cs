
using HarmonyLib;
using System;
using System.Reflection;
using Timberborn.HazardousWeatherSystemUI;

namespace Timberborn.FloodSeason
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
                __instance._currentUISpecification = __instance._badtideWeatherUISpecification;
            }
            return null;
        }
    }
}

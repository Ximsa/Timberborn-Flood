using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Timberborn.WeatherSystem;

namespace Timberborn_FloodSeason
{
    // prevent seasons to trigger
    [HarmonyPatch(typeof(TemperateWeatherDurationService))]
    [HarmonyPatch("GenerateDuration")]
    class TemperateWeatherDurationServicePatch
    {
#pragma warning disable IDE0051
        static bool Prefix(ref int __result)
        {
            __result = int.MaxValue;
            return false;
        }
    }
}

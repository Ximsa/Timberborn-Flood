using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Timberborn.HazardousWeatherSystem;

namespace Timberborn_FloodSeason
{
    [HarmonyPatch(typeof(HazardousWeatherService))]
    [HarmonyPatch("StartHazardousWeather")]
    class HazardousWeatherServiceStartHazardousWeatherPatch
    {
#pragma warning disable IDE0051
        static bool Prefix()
        {
            return false;
        }
    }
/*
    [HarmonyPatch(typeof(HazardousWeatherService))]
    [HarmonyPatch("EndHazardousWeather")]
    class HazardousWeatherServiceEndHazardousWeatherPatch
    {
#pragma warning disable IDE0051
        static bool Prefix()
        {
            return false;
        }
    }*/
}

using HarmonyLib;
using System;
using System.Reflection;
using Timberborn.HazardousWeatherSystem;
using Timberborn.WeatherSystem;

namespace Timberborn_FloodSeason
{
    [HarmonyPatch]
    class DroughtWaterStrengthModifierPatch
    {
        public static MethodBase TargetMethod()
        {
            // use normal reflection or helper methods in <AccessTools> to find the method/constructor
            // you want to patch and return its MethodInfo/ConstructorInfo
            //
            var type = Type.GetType("Timberborn.WaterSourceSystem.DroughtWaterStrengthModifier, Timberborn.WaterSourceSystem, Version = 0.0.0.0, Culture = neutral, PublicKeyToken = null");
            return AccessTools.FirstMethod(type, method => method.Name.Contains("GetStrengthModifier"));
        }
        public static void Postfix(
            ref float __result, 
            ref HazardousWeatherService ____hazardousWeatherService,
            WeatherService ____weatherService)
        {
            if (____hazardousWeatherService.CurrentCycleHazardousWeather is FloodWeather
                && ____weatherService.IsHazardousWeather)
            {
                __result = 3;
            }
        }
    }
}

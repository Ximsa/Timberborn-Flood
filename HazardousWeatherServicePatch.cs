using UnityEngine;
using HarmonyLib;
using Timberborn.HazardousWeatherSystem;
using Timberborn.MapStateSystem;
using Timberborn.SingletonSystem;
using System.Reflection;
using System;


namespace Timberborn.FloodSeason
{
    [HarmonyPatch(typeof(HazardousWeatherService))]
    class HazardousWeatherServicePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("SetForCycle")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Nicht verwendete private Member entfernen", Justification = "Harmony")]
        static bool Prefix(
            int cycle, 
            MapEditorMode ____mapEditorMode, 
            EventBus ____eventBus,
            HazardousWeatherHistory ____hazardousWeatherHistory,
            HazardousWeatherService __instance)
        {
            if (!____mapEditorMode.IsMapEditor)
            {
                var ps = __instance.GetType().GetProperties();
                PropertyInfo currentCycleHazardousWeatherInfo = ps[0]; // TODO: make cleaner
                PropertyInfo hazardousWeatherDurationInfo = ps[1];
                HazardousWeatherRandomizerReplacement hazardousWeatherRandomizer = HazardousWeatherRandomizerReplacementInstance.hazardousWeatherRandomizerReplacement;
                IHazardousWeather hazard = hazardousWeatherRandomizer.GetRandomWeatherForCycle(cycle);
                __instance.CurrentCycleHazardousWeather = hazard;
                //currentCycleHazardousWeatherInfo.SetValue(__instance, hazard);
                int cyclesCount = ____hazardousWeatherHistory.GetCyclesCount(hazard.Id);
                int hazardDuration = hazard.GetDurationAtCycle(cyclesCount + 1);
                __instance.HazardousWeatherDuration = hazardDuration;
                //hazardousWeatherDurationInfo.SetValue(__instance, hazardDuration);
                ____eventBus.Post(new HazardousWeatherSelectedEvent(hazard, hazardDuration));
                Debug.Log(hazard.GetType().Name + " \t" + hazardDuration);
            }
            return false;
        }
    }
}

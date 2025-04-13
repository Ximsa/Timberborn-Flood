using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using JetBrains.Annotations;
using Timberborn.HazardousWeatherSystem;
using Timberborn.MapStateSystem;
using Timberborn.SingletonSystem;
using UnityEngine;

namespace Timberborn.FloodSeason
{
  [HarmonyPatch(typeof(HazardousWeatherService))]
  [SuppressMessage("ReSharper", "InconsistentNaming")]
  [UsedImplicitly]
  internal class HazardousWeatherServicePatch
  {
    [HarmonyPrefix]
    [HarmonyPatch("SetForCycle")]
    [SuppressMessage("CodeQuality",
                     "IDE0051:Nicht verwendete private Member entfernen",
                     Justification = "Harmony")]
    private static bool Prefix(
      int cycle,
      MapEditorMode ____mapEditorMode,
      EventBus ____eventBus,
      HazardousWeatherHistory ____hazardousWeatherHistory,
      HazardousWeatherService __instance)
    {
      if (____mapEditorMode.IsMapEditor) return false;
      var hazardousWeatherRandomizer =
          HazardousWeatherRandomizerReplacementInstance.HazardousWeatherRandomizerReplacement;
      var hazard = hazardousWeatherRandomizer.GetRandomWeatherForCycle(cycle);
      __instance.CurrentCycleHazardousWeather = hazard;
      var cyclesCount = ____hazardousWeatherHistory.GetCyclesCount(hazard.Id);
      var hazardDuration = hazard.GetDurationAtCycle(cyclesCount + 1);
      __instance.HazardousWeatherDuration = hazardDuration;
      ____eventBus.Post(new HazardousWeatherSelectedEvent(hazard, hazardDuration));
      Debug.Log(hazard.GetType().Name + " \t" + hazardDuration);

      return false;
    }
  }
}
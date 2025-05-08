using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Timberborn.HazardousWeatherSystem;
using Timberborn.HazardousWeatherSystemUI;

namespace Timberborn.FloodSeason;

[HarmonyPatch(typeof(HazardousWeatherUIHelper))]
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
[SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Harmony")]
internal class HazardousWeatherUIHelperPatch
{
  [HarmonyFinalizer]
  [HarmonyPatch("UpdateCurrentUISpecification")]
  private static Exception Finalizer(Exception __exception, HazardousWeatherUIHelper __instance)
  {
    if (__exception is InvalidOperationException)
      __instance._currentUISpecification = __instance._badtideWeatherUISpecification;


    return null;
  }

  [HarmonyPatch("NameLocKey", MethodType.Getter)]
  [HarmonyPostfix]
  private static void NameLocKeyPatch(HazardousWeatherService ____hazardousWeatherService, ref string __result)
  {
    if (____hazardousWeatherService.CurrentCycleHazardousWeather is FloodWeather)
      __result = FloodWeatherUISpecification.NameLocKey;
  }

  [HarmonyPatch("ApproachingLocKey", MethodType.Getter)]
  [HarmonyPostfix]
  private static void ApproachingLocKeyPatch(HazardousWeatherService ____hazardousWeatherService, ref string __result)
  {
    if (____hazardousWeatherService.CurrentCycleHazardousWeather is FloodWeather)
      __result = FloodWeatherUISpecification.ApproachingLocKey;
  }

  [HarmonyPatch("InProgressLocKey", MethodType.Getter)]
  [HarmonyPostfix]
  private static void InProgressLocKeyPatch(HazardousWeatherService ____hazardousWeatherService, ref string __result)
  {
    if (____hazardousWeatherService.CurrentCycleHazardousWeather is FloodWeather)
      __result = FloodWeatherUISpecification.InProgressLocKey;
  }

  [HarmonyPatch("StartedNotificationLocKey", MethodType.Getter)]
  [HarmonyPostfix]
  private static void StartedNotificationLocKeyPatch(HazardousWeatherService ____hazardousWeatherService,
                                                     ref string __result)
  {
    if (____hazardousWeatherService.CurrentCycleHazardousWeather is FloodWeather)
      __result = FloodWeatherUISpecification.StartedNotificationLocKey;
  }

  [HarmonyPatch("EndedNotificationLocKey", MethodType.Getter)]
  [HarmonyPostfix]
  private static void EndedNotificationLocKeyPatch(HazardousWeatherService ____hazardousWeatherService,
                                                   ref string __result)
  {
    if (____hazardousWeatherService.CurrentCycleHazardousWeather is FloodWeather)
      __result = FloodWeatherUISpecification.EndedNotificationLocKey;
  }
}
using HarmonyLib;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Timberborn.HazardousWeatherSystem;
using Timberborn.WeatherSystem;
using UnityEngine;

namespace Timberborn.FloodSeason
{
  [HarmonyPatch]
  [SuppressMessage("ReSharper", "InconsistentNaming")]
  internal class DroughtWaterStrengthModifierPatch
  {
    public static MethodBase TargetMethod()
    {
      // use normal reflection or helper methods in <AccessTools> to find the method/constructor
      // you want to patch and return its MethodInfo/ConstructorInfo
      //
      var type = Type.GetType(
        "Timberborn.WaterSourceSystem.DroughtWaterStrengthModifier, Timberborn.WaterSourceSystem, Version = 0.0.0.0, Culture = neutral, PublicKeyToken = null");
      return AccessTools.FirstMethod(type, method => method.Name.Contains("GetStrengthModifier"));
    }

    public static void Postfix(
      ref float                   __result,
      ref HazardousWeatherService ____hazardousWeatherService,
      WeatherService              ____weatherService)
    {
      if (!(____hazardousWeatherService.CurrentCycleHazardousWeather is FloodWeather)
       || !____weatherService.IsHazardousWeather) return;
      if (SettingsInstance.Settings == null)
      {
        __result = 3;
        Debug.Log("SettingsInstance for FloodStrength not found");
      }
      else
      {
        __result = SettingsInstance.Settings.FloodStrength.Value;
      }
    }
  }
}
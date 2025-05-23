﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HarmonyLib;
using Timberborn.GameSound;

namespace Timberborn.FloodSeason;

[HarmonyPatch]
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
[SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Harmony")]
[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Harmony")]
internal class HazardousWeatherSoundPlayerPatch
{
  public static MethodBase TargetMethod()
  {
    // use normal reflection or helper methods in <AccessTools> to find the method/constructor
    // you want to patch and return its MethodInfo/ConstructorInfo
    //
    var type = Type.GetType(
      "Timberborn.HazardousWeatherSystemUI.HazardousWeatherSoundPlayer, Timberborn.HazardousWeatherSystemUI, Version = 0.0.0.0, Culture = neutral, PublicKeyToken = null");
    return AccessTools.FirstMethod(type, method => method.Name.Contains("OnHazardousWeatherStarted"));
  }

  [HarmonyFinalizer]
  private static Exception Finalizer(Exception __exception,
                                     GameUISoundController ____gameUISoundController) // TODO: play own sound
  {
    if (__exception is ArgumentException) ____gameUISoundController.PlayBadtideStartedSound();

    return null;
  }
}
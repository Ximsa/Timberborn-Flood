using HarmonyLib;
using System;
using System.Reflection;
using Timberborn.GameSound;

namespace Timberborn_FloodSeason
{
    [HarmonyPatch]
    class HazardousWeatherSoundPlayerPatch
    {
        public static MethodBase TargetMethod()
        {
            // use normal reflection or helper methods in <AccessTools> to find the method/constructor
            // you want to patch and return its MethodInfo/ConstructorInfo
            //
            var type = Type.GetType("Timberborn.HazardousWeatherSystemUI.HazardousWeatherSoundPlayer, Timberborn.HazardousWeatherSystemUI, Version = 0.0.0.0, Culture = neutral, PublicKeyToken = null");
            return AccessTools.FirstMethod(type, method => method.Name.Contains("OnHazardousWeatherStarted"));
        }

        [HarmonyFinalizer]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Nicht verwendete private Member entfernen", Justification = "Harmony")]
        static Exception Finalizer(Exception __exception, GameUISoundController ____gameUISoundController) // TODO: play own sound
        {
            if (__exception is ArgumentException)
            {
                ____gameUISoundController.PlayBadtideStartedSound();
            }
            return null;
        }
    }
}

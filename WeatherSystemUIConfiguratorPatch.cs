using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using Timberborn.WeatherSystemUI;
using UnityEngine;

namespace Timberborn_FloodSeason
{
    [HarmonyPatch(typeof(WeatherSystemUIConfigurator))]
    [HarmonyPatch("Configure")]
    class WeatherSystemUIConfiguratorPatch
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051", Justification = "Harmony")]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            /*
            int weatherPanelIndex = 0;
            for (; weatherPanelIndex < codes.Count; weatherPanelIndex++)
            {
                Debug.Log(weatherPanelIndex);
                CodeInstruction code = codes[weatherPanelIndex];
                Debug.Log(code.opcode);
                Debug.Log(code.operand);
            }*/
            codes.RemoveRange(3,3); // remove WeatherPanel
            return codes;
        }
    }
}

using Bindito.Core;
using HarmonyLib;
using Timberborn.HazardousWeatherSystem;

namespace Timberborn_FloodSeason
{
    [HarmonyPatch(typeof(HazardousWeatherSystemConfigurator))]
    class HazardousWeatherSystemConfiguratorPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Configure")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Nicht verwendete private Member entfernen", Justification = "Harmony")]
        static void ConfigurePostFix(IContainerDefinition containerDefinition)
        {
            containerDefinition.Bind<FloodWeather>().AsSingleton();
            containerDefinition.Bind<HazardousWeatherRandomizerReplacement>().AsSingleton();
        }
    }
}
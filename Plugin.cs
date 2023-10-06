using BepInEx;
using Bindito.Core;
using UnityEngine;
using Timberborn.BaseComponentSystem;
using Timberborn.TimeSystem;
using Timberborn.Common;
using Timberborn.SingletonSystem;
using HarmonyLib;
using System;
using TimberApi.ConfiguratorSystem;
using TimberApi.SceneSystem;

namespace Timberborn_FloodSeason
{
    [BepInPlugin("org.bepinex.plugins.flood", "Flood", "0.1.7")]
    [Configurator(SceneEntrypoint.InGame)]
    public class Plugin : BaseUnityPlugin, IConfigurator
    {
        public void Configure(IContainerDefinition containerDefinition) // TimberAPI InGame entry
        {
            containerDefinition.Bind<WaterControl>().AsSingleton();
            containerDefinition.Bind<WeatherPanel>().AsSingleton();
        }

#pragma warning disable IDE0051
        private void Awake(){ // BepInEx entry
            Debug.Log("Harmony");
            Harmony harmony = new Harmony("flood"); 
            harmony.PatchAll();
        }
    }
}

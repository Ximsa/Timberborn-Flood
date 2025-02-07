using HarmonyLib;
using Timberborn.ModManagerScene;
using UnityEngine;

namespace Timberborn.FloodSeason
{
    public class Plugin : IModStarter
    {
        public void StartMod()
        {
            Debug.Log("Hello flood!");
            Harmony harmony = new Harmony("flood");
            harmony.PatchAll();
        }
    }
}

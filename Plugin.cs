using HarmonyLib;
using System;
using Timberborn.ModManagerScene;


namespace Timberborn_FloodSeason
{
    public class Plugin : IModStarter
    {
        public void StartMod()
        {
            Console.WriteLine("Hello flood!");
            Harmony harmony = new Harmony("flood");
            harmony.PatchAll();
        }
    }
}

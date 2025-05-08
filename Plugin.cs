using HarmonyLib;
using JetBrains.Annotations;
using Timberborn.ModManagerScene;
using UnityEngine;

namespace Timberborn.FloodSeason;

[PublicAPI]
public class Plugin : IModStarter
{
  public static IModEnvironment ModEnvironment { get; private set; }

  public void StartMod(IModEnvironment modEnvironment)
  {
    ModEnvironment = modEnvironment;
    Debug.Log($"Hello Flood! Paths: {modEnvironment.ModPath} {modEnvironment.OriginPath}");
    var harmony = new Harmony("flood");
    harmony.PatchAll();
  }
}
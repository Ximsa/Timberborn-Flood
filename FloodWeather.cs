using System;
using Timberborn.Common;
using Timberborn.GameSceneLoading;
using Timberborn.HazardousWeatherSystem;
using Timberborn.MapStateSystem;
using Timberborn.Persistence;
using Timberborn.SceneLoading;
using Timberborn.SingletonSystem;
using Timberborn.WorldPersistence;
using UnityEngine;

namespace Timberborn.FloodSeason
{
  internal class FloodWeather : IHazardousWeather, ISaveableSingleton, ILoadableSingleton
  {
    private static readonly SingletonKey FloodWeatherKey = new SingletonKey("FloodWeather");
    private static readonly PropertyKey<int> MinDurationKey = new PropertyKey<int>("MinWeatherDuration");
    private static readonly PropertyKey<int> MaxDurationKey = new PropertyKey<int>("MaxWeatherDuration");
    private static readonly PropertyKey<float> HandicapMultiplierKey = new PropertyKey<float>("HandicapMultiplier");
    private static readonly PropertyKey<int> HandicapCyclesKey = new PropertyKey<int>("HandicapCycles");

    private static readonly PropertyKey<int> CyclesBeforeRandomizingKey =
        new PropertyKey<int>("CyclesBeforeRandomizing");

    private readonly MapEditorMode mapEditorMode;
    private readonly IRandomNumberGenerator randomNumberGenerator;
    private readonly SceneLoader sceneLoader;
    private readonly Settings settings;

    private readonly ISingletonLoader singletonLoader;
    private int cyclesBeforeRandomizingFloodWeather = 1;
    private int handicapCycles = 1;
    private float handicapMultiplier = 1;
    private int maxDuration = 10;
    private int minDuration = 3;

    public FloodWeather(
      ISingletonLoader singletonLoader,
      IRandomNumberGenerator randomNumberGenerator,
      MapEditorMode mapEditorMode,
      SceneLoader sceneLoader,
      Settings settings)
    {
      this.singletonLoader = singletonLoader;
      this.randomNumberGenerator = randomNumberGenerator;
      this.mapEditorMode = mapEditorMode;
      this.sceneLoader = sceneLoader;
      this.settings = settings;
    }

    public float ChanceForFlood { get; private set; }

    public string Id => FloodWeatherKey.Name;

    public int GetDurationAtCycle(int cycle)
    {
      var handicap =
          GetHandicapMultiplier(cycle, handicapMultiplier, handicapCycles);
      var inclusiveMin = handicap * minDuration;
      var inclusiveMax = handicap * maxDuration;
      var num = (int)Math.Round(randomNumberGenerator.Range(inclusiveMin, inclusiveMax),
                                MidpointRounding.AwayFromZero);
      if (minDuration > 0) num = Math.Max(num, 1);

      return num;
    }

    public void Load()
    {
      try
      {
        if (mapEditorMode.IsMapEditor) return;
        if (singletonLoader.TryGetSingleton(FloodWeatherKey, out var singleton))
        {
          Initialize(
            singleton.Get(MinDurationKey),
            singleton.Get(MaxDurationKey),
            singleton.Get(HandicapMultiplierKey),
            singleton.Get(HandicapCyclesKey),
            singleton.Get(CyclesBeforeRandomizingKey));
          return;
        }

        sceneLoader.TryGetSceneParameters(out GameSceneParameters gameSceneParameters);
        if (gameSceneParameters != null) Initialize(gameSceneParameters.NewGameConfiguration.NewGameMode);
      }
      catch (Exception ex)
      {
        Debug.Log(ex.Message);
        Debug.Log(ex.StackTrace);
        Debug.Log("Failed to load Flood settings from save file, defaulting to sane defaults.");
        Initialize();
      }
    }

    public void Save(ISingletonSaver singletonSaver)
    {
      if (mapEditorMode.IsMapEditor) return;
      var singleton = singletonSaver.GetSingleton(FloodWeatherKey);
      singleton.Set(MinDurationKey, minDuration);
      singleton.Set(MaxDurationKey, maxDuration);
      singleton.Set(HandicapMultiplierKey, handicapMultiplier);
      singleton.Set(HandicapCyclesKey, handicapCycles);
      singleton.Set(CyclesBeforeRandomizingKey, cyclesBeforeRandomizingFloodWeather);
    }

    private void Initialize(NewGameMode newGameMode)
    {
      Initialize(
        newGameMode.BadtideDuration.Min,
        newGameMode.BadtideDuration.Max,
        newGameMode.BadtideDurationHandicapMultiplier,
        newGameMode.BadtideDurationHandicapCycles,
        newGameMode.CyclesBeforeRandomizingBadtide);
    }

    public bool CanOccurAtCycle(int cycle)
    {
      return cycle > cyclesBeforeRandomizingFloodWeather;
    }

    private void Initialize(int minDuration = 6,
                            int maxDuration = 12,
                            float handicapMultiplier = 0.5f,
                            int handicapCycles = 3,
                            int cyclesBeforeRandomizingBadtide = 3)
    {
      this.minDuration = minDuration;
      this.maxDuration = maxDuration;
      this.handicapMultiplier = handicapMultiplier;
      this.handicapCycles = handicapCycles;
      cyclesBeforeRandomizingFloodWeather = cyclesBeforeRandomizingBadtide;
      ChanceForFlood = (float)settings.ChanceForFlood.Value / 100;
    }

    private static float GetHandicapMultiplier(int cycle, float handicapMultiplier, float handicapCycles)
    {
      if (!(handicapCycles > 0f)) return 1f;
      var t = Mathf.Clamp01((cycle - 1) / handicapCycles);
      return Mathf.Lerp(handicapMultiplier, 1f, t);
    }
  }
}
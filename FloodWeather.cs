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
    public float ChanceForFlood { get; private set; }

    public FloodWeather(
      ISingletonLoader       singletonLoader,
      IRandomNumberGenerator randomNumberGenerator,
      MapEditorMode          mapEditorMode,
      SceneLoader            sceneLoader,
      Settings               settings)
    {
      this._singletonLoader       = singletonLoader;
      this._randomNumberGenerator = randomNumberGenerator;
      this._mapEditorMode         = mapEditorMode;
      this._sceneLoader           = sceneLoader;
      this._settings              = settings;
    }

    public string Id => FloodWeatherKey.Name;

    private void Initialize(NewGameMode newGameMode)
    {
      this.Initialize(
        newGameMode.BadtideDuration.Min,
        newGameMode.BadtideDuration.Max,
        newGameMode.BadtideDurationHandicapMultiplier,
        newGameMode.BadtideDurationHandicapCycles,
        newGameMode.CyclesBeforeRandomizingBadtide);
    }

    public void Save(ISingletonSaver singletonSaver)
    {
      if (_mapEditorMode.IsMapEditor) return;
      var singleton = singletonSaver.GetSingleton(FloodWeatherKey);
      singleton.Set(MinDurationKey,             _minDuration);
      singleton.Set(MaxDurationKey,             _maxDuration);
      singleton.Set(HandicapMultiplierKey,      _handicapMultiplier);
      singleton.Set(HandicapCyclesKey,          _handicapCycles);
      singleton.Set(CyclesBeforeRandomizingKey, _cyclesBeforeRandomizingFloodWeather);
    }

    public void Load()
    {
      try
      {
        if (_mapEditorMode.IsMapEditor) return;
        if (_singletonLoader.HasSingleton(FloodWeatherKey))
        {
          var singleton = _singletonLoader.GetSingleton(FloodWeatherKey);
          Initialize(
            singleton.Get(MinDurationKey),
            singleton.Get(MaxDurationKey),
            singleton.Get(HandicapMultiplierKey),
            singleton.Get(HandicapCyclesKey),
            singleton.Get(CyclesBeforeRandomizingKey));
          return;
        }

        _sceneLoader.TryGetSceneParameters(out GameSceneParameters gameSceneParameters);
        if (gameSceneParameters != null)
        {
          Initialize(gameSceneParameters.NewGameConfiguration.NewGameMode);
        }
      }
      catch (Exception ex)
      {
        Debug.Log(ex.Message);
        Debug.Log(ex.StackTrace);
        Debug.Log("Failed to load Flood settings from save file, defaulting to sane defaults.");
        Initialize();
      }
    }

    public bool CanOccurAtCycle(int cycle)
    {
      return cycle > _cyclesBeforeRandomizingFloodWeather;
    }

    public int GetDurationAtCycle(int cycle)
    {
      var handicapMultiplier =
          GetHandicapMultiplier(cycle, _handicapMultiplier, _handicapCycles);
      var inclusiveMin = handicapMultiplier * _minDuration;
      var inclusiveMax = handicapMultiplier * _maxDuration;
      var num = (int)Math.Round(_randomNumberGenerator.Range(inclusiveMin, inclusiveMax),
                                MidpointRounding.AwayFromZero);
      if (_minDuration > 0)
      {
        num = Math.Max(num, 1);
      }

      return num;
    }

    private void Initialize(int   minDuration                    = 6,
                            int   maxDuration                    = 12,
                            float handicapMultiplier             = 0.5f,
                            int   handicapCycles                 = 3,
                            int   cyclesBeforeRandomizingBadtide = 3)
    {
      _minDuration                         = minDuration;
      _maxDuration                         = maxDuration;
      _handicapMultiplier                  = handicapMultiplier;
      _handicapCycles                      = handicapCycles;
      _cyclesBeforeRandomizingFloodWeather = cyclesBeforeRandomizingBadtide;
      ChanceForFlood                       = (float)_settings.ChanceForFlood.Value / 100;
    }

    private static float GetHandicapMultiplier(int cycle, float handicapMultiplier, float handicapCycles)
    {
      if (!(handicapCycles > 0f)) return 1f;
      var t = Mathf.Clamp01((cycle - 1) / handicapCycles);
      return Mathf.Lerp(handicapMultiplier, 1f, t);
    }


    private static readonly SingletonKey       FloodWeatherKey       = new SingletonKey("FloodWeather");
    private static readonly PropertyKey<int>   MinDurationKey        = new PropertyKey<int>("MinWeatherDuration");
    private static readonly PropertyKey<int>   MaxDurationKey        = new PropertyKey<int>("MaxWeatherDuration");
    private static readonly PropertyKey<float> HandicapMultiplierKey = new PropertyKey<float>("HandicapMultiplier");
    private static readonly PropertyKey<int>   HandicapCyclesKey     = new PropertyKey<int>("HandicapCycles");

    private static readonly PropertyKey<int> CyclesBeforeRandomizingKey =
        new PropertyKey<int>("CyclesBeforeRandomizing");

    private readonly ISingletonLoader       _singletonLoader;
    private readonly IRandomNumberGenerator _randomNumberGenerator;
    private readonly MapEditorMode          _mapEditorMode;
    private readonly SceneLoader            _sceneLoader;
    private readonly Settings               _settings;
    private          int                    _minDuration                         = 3;
    private          int                    _maxDuration                         = 10;
    private          float                  _handicapMultiplier                  = 1;
    private          int                    _handicapCycles                      = 1;
    private          int                    _cyclesBeforeRandomizingFloodWeather = 1;
  }
}
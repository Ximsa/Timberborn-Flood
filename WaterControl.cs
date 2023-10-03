using Bindito.Core;
using System;
using System.Linq;
using Timberborn.BaseComponentSystem;
using Timberborn.Common;
using Timberborn.Core;
using Timberborn.GameSceneLoading;
using Timberborn.Persistence;
using Timberborn.QuickNotificationSystem;
using Timberborn.SceneLoading;
using Timberborn.SingletonSystem;
using Timberborn.TickSystem;
using Timberborn.WaterSourceSystem;
using UnityEngine;

namespace Timberborn_FloodSeason
{
    class WaterControl : BaseComponent, ISaveableSingleton, ILoadableSingleton, ITickableSingleton
    {
        public WaterSource[] waterSources;
        public WaterContaminationSource[] waterContaminationSources;

        public float[] OriginalWaterStrengths;
        // Badtide
        public bool IsBadTide;
        public float TimeToBadTideToggle;
        public float BadtideMinDuration;
        public float BadtideMaxDuration;
        // Drought
        public float TimeToDroughtToggle;
        public float DroughtMaxDuration;
        public float DroughtMinDuration;
        public bool IsDrought;
        // Temperate
        public float TemperateMinDuration;
        public float TemperateMaxDuration;
        // Flood
        public float TimeToFloodTrigger;
        public float FloodMultiplier;

        public float HandicapDuration = 10;
        public int EventCount;


        private static readonly SingletonKey WaterControlKey = new SingletonKey("WaterControl");

        private static readonly ListKey<float> OriginalWaterStrengthsKey = new ListKey<float>("OriginalWaterStrengths");
        private static readonly PropertyKey<float> TemperateMinDurationKey = new PropertyKey<float>("TemperateMinDuration");
        private static readonly PropertyKey<float> TemperateMaxDurationKey = new PropertyKey<float>("TemperateMaxDuration");

        private static readonly PropertyKey<float> TimeToBadTideToggleKey = new PropertyKey<float>("TimeToBadTideToggle");
        private static readonly PropertyKey<float> BadTideMinDurationKey = new PropertyKey<float>("BadTideMinDuration");
        private static readonly PropertyKey<float> BadTideMaxDurationKey = new PropertyKey<float>("BadTideMaxDuration");
        private static readonly PropertyKey<bool> IsBadTideKey = new PropertyKey<bool>("IsBadTide");

        private static readonly PropertyKey<float> TimeToDroughtToggleKey = new PropertyKey<float>("TimeToDroughtToggle");
        private static readonly PropertyKey<float> DroughtMaxDurationKey = new PropertyKey<float>("DroughtMaxDuration");
        private static readonly PropertyKey<float> DroughtMinDurationKey = new PropertyKey<float>("DroughtMinDuration");
        private static readonly PropertyKey<bool> IsDroughtKey = new PropertyKey<bool>("IsDrought");

        private static readonly PropertyKey<float> TimeToFloodTriggerKey = new PropertyKey<float>("TimeToFloodTrigger");
        private static readonly PropertyKey<float> FloodMultiplierKey = new PropertyKey<float>("FloodMultiplier");

        private static readonly PropertyKey<int> EventCountKey = new PropertyKey<int>("EventCount");
        private static readonly PropertyKey<float> HandicapDurationKey = new PropertyKey<float>("HandicapDuration");

        // compatibility with vanilla
        private static readonly SingletonKey WeatherServiceKey = new SingletonKey("WeatherService");
        private static readonly PropertyKey<int> CycleKey = new PropertyKey<int>("Cycle");

        private static readonly SingletonKey TemperateWeatherDurationServiceKey = new SingletonKey("TemperateWeatherDurationService");
        private static readonly PropertyKey<int> MinTemperateWeatherDurationKey = new PropertyKey<int>("MinTemperateWeatherDuration");
        private static readonly PropertyKey<int> MaxTemperateWeatherDurationKey = new PropertyKey<int>("MaxTemperateWeatherDuration");

        private static readonly SingletonKey DroughtWeatherKey = new SingletonKey("DroughtWeather");
        private static readonly PropertyKey<int> MinDroughtDurationKey = new PropertyKey<int>("MinDroughtDuration");
        private static readonly PropertyKey<int> MaxDroughtDurationKey = new PropertyKey<int>("MaxDroughtDuration");
        private static readonly PropertyKey<int> HandicapCyclesKey = new PropertyKey<int>("HandicapCycles");

        private static readonly SingletonKey BadtideWeatherKey = new SingletonKey("BadtideWeather");
        private static readonly PropertyKey<int> MinBadtideWeatherDurationKey = new PropertyKey<int>("MinBadtideWeatherDuration");
        private static readonly PropertyKey<int> MaxBadtideWeatherDurationKey = new PropertyKey<int>("MaxBadtideWeatherDuration");


        private IRandomNumberGenerator _randomNumberGenerator;
        private ISingletonLoader _singletonLoader;
        private MapEditorMode _mapEditorMode;
        private SceneLoader _sceneLoader;
        private QuickNotificationService _quickNotificationService;

        private bool first_tick = true;

        [Inject]
        public void InjectDependencies(IRandomNumberGenerator randomNumberGenerator, ISingletonLoader singletonLoader, MapEditorMode mapEditorMode, SceneLoader sceneLoader, QuickNotificationService quickNotificationService)
        {
            _randomNumberGenerator = randomNumberGenerator;
            _singletonLoader = singletonLoader;
            _mapEditorMode = mapEditorMode;
            _sceneLoader = sceneLoader;
            _quickNotificationService = quickNotificationService;
        }
        public void Save(ISingletonSaver singletonSaver)
        {
            if (!this._mapEditorMode.IsMapEditor)
            {
                IObjectSaver singleton = singletonSaver.GetSingleton(WaterControlKey);
                singleton.Set(OriginalWaterStrengthsKey, OriginalWaterStrengths);
                singleton.Set(TemperateMinDurationKey, TemperateMinDuration);
                singleton.Set(TemperateMaxDurationKey, TemperateMaxDuration);

                singleton.Set(TimeToBadTideToggleKey, TimeToBadTideToggle);
                singleton.Set(BadTideMinDurationKey, BadtideMinDuration);
                singleton.Set(BadTideMaxDurationKey, BadtideMaxDuration);
                singleton.Set(IsBadTideKey, IsBadTide);

                singleton.Set(TimeToDroughtToggleKey, TimeToDroughtToggle);
                singleton.Set(DroughtMaxDurationKey, DroughtMaxDuration);
                singleton.Set(DroughtMinDurationKey, DroughtMinDuration);
                singleton.Set(IsDroughtKey, IsDrought);

                singleton.Set(TimeToFloodTriggerKey, TimeToFloodTrigger);
                singleton.Set(FloodMultiplierKey, FloodMultiplier);

                singleton.Set(EventCountKey, EventCount);
                singleton.Set(HandicapDurationKey, HandicapDuration);
            }
        }

        public void Load()
        {
            if (!this._mapEditorMode.IsMapEditor)
            {
                if (this._singletonLoader.HasSingleton(WaterControlKey)) // load from file
                {
                    try
                    {
                        IObjectLoader singleton = this._singletonLoader.GetSingleton(WaterControlKey);
                        OriginalWaterStrengths = singleton.Get(OriginalWaterStrengthsKey).ToArray();
                        TemperateMinDuration = singleton.Get(TemperateMinDurationKey);
                        TemperateMaxDuration = singleton.Get(TemperateMaxDurationKey);

                        TimeToBadTideToggle = singleton.Get(TimeToBadTideToggleKey);
                        BadtideMinDuration = singleton.Get(BadTideMinDurationKey);
                        BadtideMaxDuration = singleton.Get(BadTideMaxDurationKey);
                        IsBadTide = singleton.Get(IsBadTideKey);

                        TimeToDroughtToggle = singleton.Get(TimeToDroughtToggleKey);
                        DroughtMinDuration = singleton.Get(BadTideMinDurationKey);
                        DroughtMaxDuration = singleton.Get(BadTideMaxDurationKey);
                        IsDrought = singleton.Get(IsDroughtKey);

                        TimeToFloodTrigger = singleton.Get(TimeToFloodTriggerKey);
                        FloodMultiplier = singleton.Get(FloodMultiplierKey);

                        EventCount = singleton.Get(EventCountKey);
                        HandicapDuration = singleton.Get(HandicapDurationKey);

                        return;
                    }
                    catch (Exception)
                    {   // tried to load incompatible save, jump to guesswork
                        Debug.LogWarning("Flood tried to load an incompatible save, guessing ...");
                    }
                }
                if (this._sceneLoader.HasSceneParameters<GameSceneParameters>()) // load from new game
                {
                    GameSceneParameters sceneParameters = this._sceneLoader.GetSceneParameters<GameSceneParameters>();
                    if (sceneParameters != null && sceneParameters.NewGame) // if sceneParameters.NewGame is false, sceneParameters.NewGameConfiguration is null
                    {
                        NewGameMode newGameMode = sceneParameters.NewGameConfiguration.NewGameMode;

                        TemperateMinDuration = newGameMode.TemperateWeatherDuration.Min * 400;
                        TemperateMaxDuration = newGameMode.TemperateWeatherDuration.Max * 400;
                        OriginalWaterStrengths = new float[0];

                        TimeToBadTideToggle = _randomNumberGenerator.Range(TemperateMinDuration * 3f, TemperateMaxDuration * 4f);
                        BadtideMinDuration = newGameMode.BadtideDuration.Min * 400; // a day has (very) roughly 400 seconds
                        BadtideMaxDuration = newGameMode.BadtideDuration.Max * 400;
                        IsBadTide = false;

                        TimeToDroughtToggle = _randomNumberGenerator.Range(TemperateMinDuration, TemperateMaxDuration);
                        DroughtMinDuration = (newGameMode.DroughtDuration.Min + 3) * 400; // Droughts decay their water in around 3 days
                        DroughtMaxDuration = (newGameMode.DroughtDuration.Max + 3) * 400;
                        IsDrought = false;

                        TimeToFloodTrigger = _randomNumberGenerator.Range(TemperateMinDuration * 3f, TemperateMaxDuration * 4f);
                        FloodMultiplier = 1;

                        EventCount = 0;
                        HandicapDuration = newGameMode.DroughtDurationHandicapCycles;
                        return;
                    }
                }
                // got to do guesswork for the parameters
                TemperateMinDuration = 5 * 400;
                TemperateMaxDuration = 15 * 400;
                OriginalWaterStrengths = new float[0];

                TimeToBadTideToggle = _randomNumberGenerator.Range(TemperateMinDuration, TemperateMaxDuration);
                BadtideMinDuration = 7 * 400; // a day has (very) roughly 400 seconds
                BadtideMaxDuration = 20 * 400;
                IsBadTide = false;

                TimeToDroughtToggle = _randomNumberGenerator.Range(TemperateMinDuration, TemperateMaxDuration);
                DroughtMinDuration = 7 * 400;
                DroughtMaxDuration = 21 * 400;
                IsDrought = false;

                TimeToFloodTrigger = _randomNumberGenerator.Range(TemperateMinDuration, TemperateMaxDuration);
                FloodMultiplier = 1;

                EventCount = 5; // assume the game ran for a bit
                HandicapDuration = 10;

                if (this._singletonLoader.HasSingleton(TemperateWeatherDurationServiceKey))
                {
                    IObjectLoader singleton = this._singletonLoader.GetSingleton(TemperateWeatherDurationServiceKey);
                    TemperateMinDuration = singleton.Get(MinTemperateWeatherDurationKey) * 400;
                    TemperateMaxDuration = singleton.Get(MaxTemperateWeatherDurationKey) * 400;
                }
                if (this._singletonLoader.HasSingleton(DroughtWeatherKey))
                {
                    IObjectLoader singleton = this._singletonLoader.GetSingleton(DroughtWeatherKey);
                    DroughtMinDuration = (3+singleton.Get(MinDroughtDurationKey)) * 400;
                    DroughtMaxDuration = (3+singleton.Get(MaxDroughtDurationKey)) * 400;
                    HandicapDuration = singleton.Get(HandicapCyclesKey);
                }
                if (this._singletonLoader.HasSingleton(BadtideWeatherKey))
                {
                    IObjectLoader singleton = this._singletonLoader.GetSingleton(BadtideWeatherKey);
                    BadtideMinDuration = singleton.Get(MinBadtideWeatherDurationKey) * 400;
                    BadtideMaxDuration = singleton.Get(MaxBadtideWeatherDurationKey) * 400;
                }
                if (this._singletonLoader.HasSingleton(WeatherServiceKey))
                {
                    IObjectLoader singleton = this._singletonLoader.GetSingleton(WeatherServiceKey);
                    EventCount = singleton.Get(CycleKey);
                }
            }
        }
        public void Tick()
        {
            if (first_tick)
            {
                UpdateSourcesArray();
                first_tick = false;
            }
            if (OriginalWaterStrengths == null || OriginalWaterStrengths.Length == 0)
            {
                OriginalWaterStrengths = waterSources.Select(source => source.SpecifiedStrength).ToArray();
            }
            // it gets worse and worse and worse and worse, handicap ~ linear until handicapduration is reached, then handicap ~ sqrt(ln)
            float handicap = MathF.Max(0.3f, MathF.Min(EventCount / HandicapDuration, MathF.Sqrt(MathF.Log(EventCount + 1f) / MathF.Log(MathF.Max(5.0f,HandicapDuration)))));

            TimeToBadTideToggle -= Time.fixedDeltaTime;
            TimeToDroughtToggle -= Time.fixedDeltaTime;
            TimeToFloodTrigger -= Time.fixedDeltaTime;

            if (TimeToBadTideToggle < 0)
            {
                EventCount++;
                IsBadTide = !IsBadTide;
                if (IsBadTide)
                {
                    var msg = "Badtide started";
                    TimeToBadTideToggle = _randomNumberGenerator.Range(BadtideMinDuration * handicap, BadtideMaxDuration * handicap);
                    float badTideSevereness = _randomNumberGenerator.Range(0.5f, 1f);
                    foreach (WaterContaminationSource source in waterSources.Select(source => source.GetComponentFast<WaterContaminationSource>()))
                    {
                        source.SetContamination(badTideSevereness);
                    }
                    _quickNotificationService.SendWarningNotification(msg);
                    Debug.Log(msg + "\tSevereness:\t" + badTideSevereness + "\tToggle:\t" + TimeToBadTideToggle);
                }
                else
                {
                    var msg = "Badtide ended";
                    TimeToBadTideToggle = _randomNumberGenerator.Range(TemperateMinDuration * 1.5f, TemperateMaxDuration * 3f);
                    foreach (WaterContaminationSource source in waterSources.Select(source => source.GetComponentFast<WaterContaminationSource>()))
                    {
                        source.ResetContamination();
                    }
                    _quickNotificationService.SendWarningNotification(msg);
                    Debug.Log(msg + "\tToggle:\t" + TimeToBadTideToggle);
                }
            }
            if (TimeToDroughtToggle < 0)
            {
                EventCount++;
                IsDrought = !IsDrought;
                var msg = "Drought " + (IsDrought ? "started" : "ended");
                TimeToDroughtToggle = IsDrought ? _randomNumberGenerator.Range(DroughtMinDuration * handicap, DroughtMaxDuration * handicap) : _randomNumberGenerator.Range(TemperateMinDuration, TemperateMaxDuration * 3f);
                FloodMultiplier = IsDrought ? 0 : FloodMultiplier;
                _quickNotificationService.SendWarningNotification(msg);
                Debug.Log(msg+"\tToggle:\t"+ TimeToDroughtToggle);
            }
            if (TimeToFloodTrigger < 0)
            {
                var msg = "Flood started";
                EventCount++;
                if (_randomNumberGenerator.Range(0f, 1f) < 0.35) // Floods usually come in waves
                {
                    TimeToFloodTrigger = _randomNumberGenerator.Range(TemperateMinDuration , TemperateMaxDuration * 3f);
                }
                else
                {
                    TimeToFloodTrigger = _randomNumberGenerator.Range(400, 1200); // waves spawn with 1-3 days delay inbetween
                }
                FloodMultiplier = (IsDrought ? _randomNumberGenerator.Range(0.1f, 0.5f) : _randomNumberGenerator.Range(1.0f, 4.0f)) * handicap;
                _quickNotificationService.SendWarningNotification(msg);
                Debug.Log(msg + "\tSevereness\t" + FloodMultiplier + "\tTrigger:\t" + TimeToFloodTrigger);
            }
            UpdateWaterStrength();
        }

        private void UpdateSourcesArray()
        {
            waterSources = (WaterSource[])FindObjectsOfType(typeof(WaterSource));
            waterContaminationSources = (WaterContaminationSource[])FindObjectsOfType(typeof(WaterContaminationSource));
        }

        private void UpdateWaterStrength() // increase water strength until flood multiplier is reached
        {
            float strength = 0;
            foreach ((WaterSource source, float originalStrength) in waterSources.Zip(OriginalWaterStrengths, (x, y) => (x, y)))
            {
                bool isNoBadtideSource = source.GetComponentFast<WaterContaminationSource>().Contamination == 0;
                if (IsBadTide || isNoBadtideSource) // only handle contaminated sources if badtide is happening
                {
                    if (source.CurrentStrength / originalStrength < FloodMultiplier) // increase water level
                    {
                        source.ReinitializeStrength(source.CurrentStrength + (IsDrought ? 0.002f : 0.001f) * originalStrength); // Drought reaches threshold quicker so there is less water overall
                        strength = source.CurrentStrength;
                    }
                    else
                    {
                        FloodMultiplier = IsDrought ? 0 : 1; // target reached
                    }
                }
                // decay water level
                float lowerDroughtBound = (!isNoBadtideSource && !IsBadTide) ? originalStrength : 0; // prevent badtide sources to dry up completely, except on a badtide event
                strength = source.CurrentStrength;
                float diff = source.CurrentStrength - originalStrength;
                source.ReinitializeStrength(IsDrought ? Mathf.Max(lowerDroughtBound, source.CurrentStrength * 0.9995f - 0.0005f) : originalStrength + diff * 0.9998f); // roughly decay 20% a day, 50% a day for drought +-*/
            }
            /*Debug.Log(FloodMultiplier 
                + "\t" + SerializeFloatArray(OriginalWaterStrengths)
                + "\t" + SerializeFloatArray(waterSources.Select(x => MathF.Floor(x.CurrentStrength * 100) / 100.0f).ToArray()));*/
        }

        private string SerializeFloatArray(float[] a)
        {
            return string.Join(";", a);
        }
        private float[] DeserializeFloatArray(string s)
        {
            return Array.ConvertAll(s.Split(';'), float.Parse);
        }
    }
}

using Bindito.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Timberborn.BaseComponentSystem;
using Timberborn.Common;
using Timberborn.Core;
using Timberborn.GameSceneLoading;
using Timberborn.HazardousWeatherSystem;
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
        public float BadTideMinDuration;
        public float BadTideMaxDuration;
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

        public int EventCount;

        private static readonly SingletonKey WaterControlKey = new SingletonKey("WaterControl");
        private static readonly PropertyKey<string> OriginalWaterStrengthsKey = new PropertyKey<string>("OriginalWaterStrengths"); // dunno why ListKey<float> wont work
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
                singleton.Set(OriginalWaterStrengthsKey, SerializeFloatArray(OriginalWaterStrengths));
                singleton.Set(TemperateMinDurationKey, TemperateMinDuration);
                singleton.Set(TemperateMaxDurationKey, TemperateMaxDuration);

                singleton.Set(TimeToBadTideToggleKey, TimeToBadTideToggle);
                singleton.Set(BadTideMinDurationKey, BadTideMinDuration);
                singleton.Set(BadTideMaxDurationKey, BadTideMaxDuration);
                singleton.Set(IsBadTideKey, IsBadTide);

                singleton.Set(TimeToDroughtToggleKey, TimeToDroughtToggle);
                singleton.Set(DroughtMaxDurationKey, DroughtMaxDuration);
                singleton.Set(DroughtMinDurationKey, DroughtMinDuration);
                singleton.Set(IsDroughtKey, IsDrought);

                singleton.Set(TimeToFloodTriggerKey, TimeToFloodTrigger);
                singleton.Set(FloodMultiplierKey, FloodMultiplier);

                singleton.Set(EventCountKey, EventCount);
            }
        }

        public void Load()
        {
            if (!this._mapEditorMode.IsMapEditor)
            {
                if (this._singletonLoader.HasSingleton(WaterControlKey)) // load from file
                {
                    IObjectLoader singleton = this._singletonLoader.GetSingleton(WaterControlKey);
                    OriginalWaterStrengths = DeserializeFloatArray(singleton.Get(OriginalWaterStrengthsKey));
                    TemperateMinDuration = singleton.Get(TemperateMinDurationKey);
                    TemperateMaxDuration = singleton.Get(TemperateMaxDurationKey);

                    TimeToBadTideToggle = singleton.Get(TimeToBadTideToggleKey);
                    BadTideMinDuration = singleton.Get(BadTideMinDurationKey);
                    BadTideMaxDuration = singleton.Get(BadTideMaxDurationKey);
                    IsBadTide = singleton.Get(IsBadTideKey);

                    TimeToDroughtToggle = singleton.Get(TimeToDroughtToggleKey);
                    DroughtMinDuration = singleton.Get(BadTideMinDurationKey);
                    DroughtMaxDuration = singleton.Get(BadTideMaxDurationKey);
                    IsDrought = singleton.Get(IsDroughtKey);

                    TimeToFloodTrigger = singleton.Get(TimeToFloodTriggerKey);
                    FloodMultiplier = singleton.Get(FloodMultiplierKey);

                    EventCount = singleton.Get(EventCountKey);
                    return;
                }
                if (this._sceneLoader.HasSceneParameters<GameSceneParameters>()) // load from new game
                {
                    GameSceneParameters sceneParameters = this._sceneLoader.GetSceneParameters<GameSceneParameters>();
                    if (sceneParameters != null && sceneParameters.NewGame)
                    {
                        NewGameMode newGameMode = sceneParameters.NewGameConfiguration.NewGameMode;

                        TemperateMinDuration = newGameMode.TemperateWeatherDuration.Min * 400;
                        TemperateMaxDuration = newGameMode.TemperateWeatherDuration.Min * 400;
                        OriginalWaterStrengths = null;

                        TimeToBadTideToggle = _randomNumberGenerator.Range(TemperateMinDuration*3f, TemperateMaxDuration*4f);
                        BadTideMinDuration = newGameMode.BadtideDuration.Min * 400; // a day has (very) roughly 400 seconds
                        BadTideMaxDuration = newGameMode.BadtideDuration.Max * 400;
                        IsBadTide = false;

                        TimeToDroughtToggle = _randomNumberGenerator.Range(TemperateMinDuration, TemperateMaxDuration);
                        DroughtMinDuration = (newGameMode.DroughtDuration.Min + 3) * 400; // Droughts decay their water in around 3 days
                        DroughtMaxDuration = (newGameMode.DroughtDuration.Max + 3) * 400;
                        IsDrought = false;

                        TimeToFloodTrigger = _randomNumberGenerator.Range(TemperateMinDuration*3f, TemperateMaxDuration*4f);
                        FloodMultiplier = 1;

                        EventCount = 0;
                        return;
                    }
                }
            }
        }
        public void Tick()
        {
            if(first_tick)
            {
                UpdateSourcesArray();
                first_tick = false; 
            }
            if (OriginalWaterStrengths == null)
            {
                OriginalWaterStrengths = waterSources.Select(source => source.SpecifiedStrength).ToArray();
            }
            float handicap = MathF.Max(0.3f, MathF.Min(EventCount / 10, 1));

            TimeToBadTideToggle -= Time.fixedDeltaTime;
            TimeToDroughtToggle -= Time.fixedDeltaTime;
            TimeToFloodTrigger -= Time.fixedDeltaTime;

            if(TimeToBadTideToggle < 0)
            {
                EventCount++;
                IsBadTide = !IsBadTide;
                if(IsBadTide)
                {
                    _quickNotificationService.SendWarningNotification("Badtide started");
                    TimeToBadTideToggle = _randomNumberGenerator.Range(BadTideMinDuration * handicap, BadTideMaxDuration * handicap);
                    float badTideSevereness = _randomNumberGenerator.Range(0.5f, 1f);
                    foreach (WaterContaminationSource source in waterSources.Select(source => source.GetComponentFast<WaterContaminationSource>()))
                    {
                        source.SetContamination(badTideSevereness);
                    }
                }
                else
                {
                    _quickNotificationService.SendWarningNotification("Badtide ended");
                    TimeToBadTideToggle = _randomNumberGenerator.Range(TemperateMinDuration, TemperateMaxDuration * 3f);
                    foreach (WaterContaminationSource source in waterSources.Select(source => source.GetComponentFast<WaterContaminationSource>()))
                    {
                        source.ResetContamination();
                    }
                }
            }
            if(TimeToDroughtToggle < 0)
            {
                EventCount++;
                IsDrought = !IsDrought;
                _quickNotificationService.SendWarningNotification("Drought "+ (IsDrought? "started" : "ended"));
                TimeToDroughtToggle = IsDrought ? _randomNumberGenerator.Range(DroughtMinDuration * handicap, DroughtMaxDuration * handicap) : _randomNumberGenerator.Range(TemperateMinDuration, TemperateMaxDuration*2f);
                FloodMultiplier = IsDrought? 0 : FloodMultiplier;
            }
            if(TimeToFloodTrigger < 0)
            {
                EventCount++;
                if(_randomNumberGenerator.Range(0f,1f) < 0.40) // Floods usually come in waves
                {
                    TimeToFloodTrigger = _randomNumberGenerator.Range(TemperateMinDuration * 1.5f, TemperateMaxDuration * 3f);
                }
                else
                {
                    TimeToFloodTrigger = _randomNumberGenerator.Range(200, 800); // waves spawn with 0.5-2 days delay inbetween
                }
                FloodMultiplier = IsDrought? _randomNumberGenerator.Range(0.1f, 0.5f) : _randomNumberGenerator.Range(1.0f, 4.0f);
                _quickNotificationService.SendWarningNotification("Flood started");
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
                        source.ReinitializeStrength(source.CurrentStrength + (IsDrought? 0.005f : 0.001f) * originalStrength); // Drought reaches threshold quicker so there is less water overall
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

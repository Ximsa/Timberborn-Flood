using Bindito.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Timberborn.BaseComponentSystem;
using Timberborn.Common;
using Timberborn.Core;
using Timberborn.GameSceneLoading;
using Timberborn.Persistence;
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
        private bool first_tick = true;

        [Inject]
        public void InjectDependencies(IRandomNumberGenerator randomNumberGenerator, ISingletonLoader singletonLoader, MapEditorMode mapEditorMode, SceneLoader sceneLoader)
        {
            _randomNumberGenerator = randomNumberGenerator;
            _singletonLoader = singletonLoader;
            _mapEditorMode = mapEditorMode;
            _sceneLoader = sceneLoader;
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

                        TimeToBadTideToggle = _randomNumberGenerator.Range(TemperateMinDuration*2f, TemperateMaxDuration*3f);
                        BadTideMinDuration = newGameMode.BadtideDuration.Min * 400; // a day has (very) roughly 400 seconds
                        BadTideMaxDuration = newGameMode.BadtideDuration.Max * 400;
                        IsBadTide = false;

                        TimeToDroughtToggle = _randomNumberGenerator.Range(TemperateMinDuration, TemperateMaxDuration);
                        DroughtMinDuration = newGameMode.DroughtDuration.Min * 400;
                        DroughtMaxDuration = newGameMode.DroughtDuration.Max * 400;
                        IsDrought = false;

                        TimeToFloodTrigger = _randomNumberGenerator.Range(TemperateMinDuration*2f, TemperateMaxDuration*3f);
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
                    Debug.Log("Start BadTide");
                    TimeToBadTideToggle = _randomNumberGenerator.Range(BadTideMinDuration * handicap, BadTideMaxDuration * handicap);
                    float badTideSevereness = _randomNumberGenerator.Range(0.5f, 1f);
                    foreach (WaterContaminationSource source in waterSources.Select(source => source.GetComponentFast<WaterContaminationSource>()))
                    {
                        source.SetContamination(badTideSevereness);
                    }
                }
                else
                {
                    Debug.Log("Stop BadTide");
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
                Debug.Log((IsDrought ? "Start" : "Stop") + " Drought");
                TimeToDroughtToggle = IsDrought ? _randomNumberGenerator.Range(DroughtMinDuration * handicap, DroughtMaxDuration * handicap) : _randomNumberGenerator.Range(TemperateMinDuration, TemperateMaxDuration*2f);
                FloodMultiplier = 0;
            }
            if(TimeToFloodTrigger < 0)
            {
                Debug.Log("Start Flood");
                EventCount++;
                if(_randomNumberGenerator.Range(0f,1f) < 0.75*handicap) // Floods usually comes in waves
                {
                    TimeToFloodTrigger = _randomNumberGenerator.Range(TemperateMinDuration / handicap, (TemperateMaxDuration / handicap) * 2f);
                }
                else
                {
                    TimeToFloodTrigger = _randomNumberGenerator.Range(200, 900); // waves spawn with 0.5-2.5 days delay inbetween
                }
                FloodMultiplier = IsDrought ? _randomNumberGenerator.Range(0.0f, 1f) : _randomNumberGenerator.Range(1.0f, 3.5f);
            }
            DecayWaterStrength();
            IncreaseWaterStrength();
        }

        private void UpdateSourcesArray()
        {
            waterSources = (WaterSource[])FindObjectsOfType(typeof(WaterSource));
            waterContaminationSources = (WaterContaminationSource[])FindObjectsOfType(typeof(WaterContaminationSource));
        }

        private void DecayWaterStrength() // slowly decay the waterlevels to original level
        {
            foreach((WaterSource source, float originalStrength) in waterSources.Zip(OriginalWaterStrengths, (x,y) => (x,y))) // TODO: why is the identity needed? Doc says something different
            {
                float diff = source.CurrentStrength - originalStrength;
                source.SetStrength(IsDrought ? MathF.Max(0, source.CurrentStrength * 0.999f) : originalStrength + diff * 0.9999f); // roughly decay 20% a day +-*/
            }
        }

        private void IncreaseWaterStrength() // increase water strength until flood multiplier is reached
        {
            Debug.Log(FloodMultiplier);
            foreach ((WaterSource source, float originalStrength) in waterSources.Zip(OriginalWaterStrengths, (x, y) => (x, y)))
            {
                if (source.CurrentStrength / originalStrength < FloodMultiplier)
                {
                    if(IsBadTide || source.GetComponentFast<WaterContaminationSource>().Contamination == 0) // only increase contaminated sources if badtide is happening
                    {
                        source.SetStrength((float)(source.CurrentStrength + 0.003 * originalStrength)); // roughly reach flood level in a day +-*/
                        Debug.Log(source.CurrentStrength);
                    }
                }
                else
                {
                    FloodMultiplier = IsDrought? 0:1;
                }
            }
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

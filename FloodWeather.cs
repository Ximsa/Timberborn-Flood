using System;
using Timberborn.Common;
using Timberborn.GameSceneLoading;
using Timberborn.MapStateSystem;
using Timberborn.Persistence;
using Timberborn.SceneLoading;
using Timberborn.SingletonSystem;
using UnityEngine;

namespace Timberborn.HazardousWeatherSystem
{
	public class FloodWeather : IHazardousWeather, ISaveableSingleton, ILoadableSingleton
	{
		public float ChanceForFlood { get; private set; }

		public FloodWeather(ISingletonLoader singletonLoader, IRandomNumberGenerator randomNumberGenerator, MapEditorMode mapEditorMode, SceneLoader sceneLoader)
		{
			this._singletonLoader = singletonLoader;
			this._randomNumberGenerator = randomNumberGenerator;
			this._mapEditorMode = mapEditorMode;
			this._sceneLoader = sceneLoader;
		}

		public string Id
		{
			get
			{
				return FloodWeather.FloodWeatherKey.Name;
			}
		}

		public void Initialize(NewGameMode newGameMode)
		{
			this.Initialize(newGameMode.BadtideDuration.Min, newGameMode.BadtideDuration.Max, newGameMode.BadtideDurationHandicapMultiplier, newGameMode.BadtideDurationHandicapCycles, newGameMode.CyclesBeforeRandomizingBadtide, newGameMode.ChanceForBadtide);
		}

		public void Save(ISingletonSaver singletonSaver)
		{
			if (!this._mapEditorMode.IsMapEditor)
			{
				IObjectSaver singleton = singletonSaver.GetSingleton(FloodWeather.FloodWeatherStoreKey);
				singleton.Set(FloodWeather.MinDurationKey, this._minDuration);
				singleton.Set(FloodWeather.MaxDurationKey, this._maxDuration);
				singleton.Set(FloodWeather.HandicapMultiplierKey, this._handicapMultiplier);
				singleton.Set(FloodWeather.HandicapCyclesKey, this._handicapCycles);
				singleton.Set(FloodWeather.CyclesBeforeRandomizingKey, this._cyclesBeforeRandomizingFloodWeather);
				singleton.Set(FloodWeather.ChanceForFloodWeatherKey, this.ChanceForFlood);
			}
		}

		public void Load()
		{
			if (!this._mapEditorMode.IsMapEditor)
			{
				if (this._singletonLoader.HasSingleton(FloodWeather.FloodWeatherKey))
				{
					IObjectLoader singleton = this._singletonLoader.GetSingleton(FloodWeather.FloodWeatherKey);
					this.Initialize(singleton.Get(FloodWeather.MinDurationKey), singleton.Get(FloodWeather.MaxDurationKey), singleton.Get(FloodWeather.HandicapMultiplierKey), singleton.Get(FloodWeather.HandicapCyclesKey), singleton.Get(FloodWeather.CyclesBeforeRandomizingKey), singleton.Get(FloodWeather.ChanceForFloodWeatherKey));
					return;
				}
				GameSceneParameters gameSceneParameters;
				this._sceneLoader.TryGetSceneParameters<GameSceneParameters>(out gameSceneParameters);
				this.Initialize(gameSceneParameters.NewGameConfiguration.NewGameMode);
			}
		}

		public bool CanOccurAtCycle(int cycle)
		{
			return cycle > this._cyclesBeforeRandomizingFloodWeather;
		}

		public int GetDurationAtCycle(int cycle)
		{
			float handicapMultiplier = GetHandicapMultiplier(cycle, this._handicapMultiplier, (float)this._handicapCycles);
			float inclusiveMin = handicapMultiplier * (float)this._minDuration;
			float inclusiveMax = handicapMultiplier * (float)this._maxDuration;
			int num = (int)Math.Round((double)this._randomNumberGenerator.Range(inclusiveMin, inclusiveMax), MidpointRounding.AwayFromZero);
			if (this._minDuration > 0)
			{
				num = Math.Max(num, 1);
			}
			return num;
		}

		private void Initialize(int minDuration, int maxDuration, float handicapMultiplier, int handicapCycles, int cyclesBeforeRandomizingBadtide, float chanceForFlood)
		{
			this._minDuration = minDuration;
			this._maxDuration = maxDuration;
			this._handicapMultiplier = handicapMultiplier;
			this._handicapCycles = handicapCycles;
			this._cyclesBeforeRandomizingFloodWeather = cyclesBeforeRandomizingBadtide;
			this.ChanceForFlood = chanceForFlood;
		}

		private float GetHandicapMultiplier(int cycle, float handicapMultiplier, float handicapCycles)
		{
			if (handicapCycles > 0f)
			{
				float t = Mathf.Clamp01((float)(cycle - 1) / handicapCycles);
				return Mathf.Lerp(handicapMultiplier, 1f, t);
			}
			return 1f;
		}


		private static readonly SingletonKey FloodWeatherKey = new SingletonKey("BadtideWeather");
		private static readonly SingletonKey FloodWeatherStoreKey = new SingletonKey("FloodWeather");
		private static readonly PropertyKey<int> MinDurationKey = new PropertyKey<int>("MinBadtideWeatherDuration");
		private static readonly PropertyKey<int> MaxDurationKey = new PropertyKey<int>("MaxBadtideWeatherDuration");
		private static readonly PropertyKey<float> HandicapMultiplierKey = new PropertyKey<float>("HandicapMultiplier");
		private static readonly PropertyKey<int> HandicapCyclesKey = new PropertyKey<int>("HandicapCycles");
		private static readonly PropertyKey<int> CyclesBeforeRandomizingKey = new PropertyKey<int>("CyclesBeforeRandomizing");
		private static readonly PropertyKey<float> ChanceForFloodWeatherKey = new PropertyKey<float>("ChanceBadtideWeather");
		private readonly ISingletonLoader _singletonLoader;
		private readonly IRandomNumberGenerator _randomNumberGenerator;
		private readonly MapEditorMode _mapEditorMode;
		private readonly SceneLoader _sceneLoader;
		private int _minDuration;
		private int _maxDuration;
		private float _handicapMultiplier;
		private int _handicapCycles;
		private int _cyclesBeforeRandomizingFloodWeather;
	}
}

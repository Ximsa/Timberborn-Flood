using Timberborn.Common;
using Timberborn.HazardousWeatherSystem;
using UnityEngine;

namespace Timberborn.FloodSeason
{
  class HazardousWeatherRandomizerReplacement
  {
    // Token: 0x0600002F RID: 47 RVA: 0x00002B37 File Offset: 0x00000D37
    public HazardousWeatherRandomizerReplacement(
      DroughtWeather         droughtWeather,
      BadtideWeather         badtideWeather,
      FloodWeather           floodWeather,
      IRandomNumberGenerator randomNumberGenerator)
    {
      this._droughtWeather        = droughtWeather;
      this._badtideWeather        = badtideWeather;
      this._floodWeather          = floodWeather;
      this._randomNumberGenerator = randomNumberGenerator;

      HazardousWeatherRandomizerReplacementInstance.HazardousWeatherRandomizerReplacement = this;
    }

    public IHazardousWeather GetRandomWeatherForCycle(int cycle)
    {
      IHazardousWeather result = _droughtWeather;

      float roll = _randomNumberGenerator.Range(0.0f, 1.0f);                                // roll for flood
      if (roll < this._floodWeather.ChanceForFlood && _floodWeather.CanOccurAtCycle(cycle)) // fixed 25% chance
      {
        result = _floodWeather;
      }
      else
      {
        roll = _randomNumberGenerator.Range(0.0f, 1.0f); // roll for badtide
        result = _badtideWeather.CanOccurAtCycle(cycle) && roll < _badtideWeather.ChanceForBadtide
            ? _badtideWeather
            : result;
      }

      Debug.Log(result.GetType().Name);
      return result;
    }

    private readonly DroughtWeather         _droughtWeather;
    private readonly BadtideWeather         _badtideWeather;
    private readonly FloodWeather           _floodWeather;
    private readonly IRandomNumberGenerator _randomNumberGenerator;
  }

  static class HazardousWeatherRandomizerReplacementInstance
  {
    public static HazardousWeatherRandomizerReplacement HazardousWeatherRandomizerReplacement;
  }
}
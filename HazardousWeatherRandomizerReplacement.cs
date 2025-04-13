using Timberborn.Common;
using Timberborn.HazardousWeatherSystem;
using UnityEngine;

namespace Timberborn.FloodSeason
{
  internal class HazardousWeatherRandomizerReplacement
  {
    private readonly BadtideWeather badtideWeather;
    private readonly DroughtWeather droughtWeather;
    private readonly FloodWeather floodWeather;

    private readonly IRandomNumberGenerator randomNumberGenerator;

    // Token: 0x0600002F RID: 47 RVA: 0x00002B37 File Offset: 0x00000D37
    public HazardousWeatherRandomizerReplacement(
      DroughtWeather droughtWeather,
      BadtideWeather badtideWeather,
      FloodWeather floodWeather,
      IRandomNumberGenerator randomNumberGenerator)
    {
      this.droughtWeather = droughtWeather;
      this.badtideWeather = badtideWeather;
      this.floodWeather = floodWeather;
      this.randomNumberGenerator = randomNumberGenerator;

      HazardousWeatherRandomizerReplacementInstance.HazardousWeatherRandomizerReplacement = this;
    }

    public IHazardousWeather GetRandomWeatherForCycle(int cycle)
    {
      IHazardousWeather result = droughtWeather;

      var roll = randomNumberGenerator.Range(0.0f, 1.0f); // roll for flood
      if (roll < floodWeather.ChanceForFlood && floodWeather.CanOccurAtCycle(cycle)) // fixed 25% chance
      {
        result = floodWeather;
      }
      else
      {
        roll = randomNumberGenerator.Range(0.0f, 1.0f); // roll for badtide
        result = badtideWeather.CanOccurAtCycle(cycle) && roll < badtideWeather.ChanceForBadtide
            ? badtideWeather
            : result;
      }

      Debug.Log(result.GetType().Name);
      return result;
    }
  }

  internal static class HazardousWeatherRandomizerReplacementInstance
  {
    public static HazardousWeatherRandomizerReplacement HazardousWeatherRandomizerReplacement;
  }
}
using Bindito.Core;

namespace Timberborn.FloodSeason;

[Context("Game")]
internal class FloodConfigurator : Configurator
{
  protected override void Configure()
  {
    Bind<FloodWeather>().AsSingleton();
    Bind<HazardousWeatherRandomizerReplacement>().AsSingleton();
  }
}
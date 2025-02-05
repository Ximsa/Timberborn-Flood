using Bindito.Core;

namespace Timberborn.FloodSeason
{
	[Context("Game")]
	[Context("MapEditor")]
	internal class FloodConfigurator : Configurator
	{
		// Token: 0x06000049 RID: 73 RVA: 0x00002F0C File Offset: 0x0000110C
		public override void Configure()
		{
			base.Bind<HazardousWeatherRandomizerReplacement>().AsSingleton();
			base.Bind<FloodWeather>().AsSingleton();
		}
	}
}
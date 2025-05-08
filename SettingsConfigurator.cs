using Bindito.Core;

namespace Timberborn.FloodSeason;

[Context("MainMenu")]
[Context("Game")]
internal class SettingsConfigurator : Configurator
{
  // Token: 0x06000049 RID: 73 RVA: 0x00002F0C File Offset: 0x0000110C
  protected override void Configure()
  {
    Bind<Settings>().AsSingleton();
  }
}
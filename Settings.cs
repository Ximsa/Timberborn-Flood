using ModSettings.Common;
using ModSettings.Core;
using Timberborn.Modding;
using Timberborn.SettingsSystem;

namespace Timberborn.FloodSeason
{
  internal class Settings : ModSettingsOwner
  {
    public RangeIntModSetting ChanceForFlood { get; } =
      new RangeIntModSetting(25,
                             0,
                             100,
                             ModSettingDescriptor.CreateLocalized("Ximsa.FloodSeason.ChanceForFlood"));

    public ModSetting<float> FloodStrength { get; } =
      new ModSetting<float>(3,
                            ModSettingDescriptor.CreateLocalized("Ximsa.FloodSeason.FloodStrength"));

    public Settings(ISettings                settings,
                    ModSettingsOwnerRegistry modSettingsOwnerRegistry,
                    ModRepository            modRepository)
        : base(settings, modSettingsOwnerRegistry, modRepository)
    {
      SettingsInstance.Settings = this;
    }

    public override    string HeaderLocKey => "Ximsa.FloodSeason.Header";
    protected override string ModId        => "Ximsa.FloodSeason";
  }

  internal static class SettingsInstance
  {
    public static Settings Settings;
  }
}
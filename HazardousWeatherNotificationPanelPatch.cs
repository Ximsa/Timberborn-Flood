using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Timberborn.HazardousWeatherSystemUI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Timberborn.FloodSeason;

[HarmonyPatch(typeof(HazardousWeatherNotificationPanel))]
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
[SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Harmony")]
public class HazardousWeatherNotificationPanelPatch
{
  [HarmonyPatch("ShowNotification")]
  [HarmonyPostfix]
  private static void ShowNotificationPatch(
    bool isHazardous, Image ____background, HazardousWeatherUIHelper ____hazardousWeatherUIHelper)
  {
    if (____hazardousWeatherUIHelper.NameLocKey != FloodWeatherUISpecification.NameLocKey || !isHazardous)
    {
      ____background.image = null;
      return;
    }

    ____background.image = TextureProvider.GetTexture(
      Plugin.ModEnvironment.OriginPath + "/Sprites/" + FloodWeatherUISpecification.NotificationBackgroundClass +
      ".png");
  }
}
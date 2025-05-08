using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using JetBrains.Annotations;
using Timberborn.CoreUI;
using Timberborn.WeatherSystemUI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Timberborn.FloodSeason;

[HarmonyPatch(typeof(WeatherPanel))]
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
[UsedImplicitly]
[SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Harmony")]
public class WeatherPanelPatch
{
  private static Sprite originalSprite;
  private static Sprite floodSprite;

  [HarmonyPatch("UpdateHazardousWeatherClasses")]
  [HarmonyPostfix]
  private static void UpdateHazardousWeatherClassesPatch(VisualElement ____root, string ____hazardousWeatherClass,
                                                         SimpleProgressBar ____simpleProgressBar)
  {
    var sprite = ____simpleProgressBar.GetFieldValue<Sprite>("_image");
    if (sprite is null)
      return;
    originalSprite ??= sprite;
    floodSprite ??=
        Sprite.Create(
          TextureProvider.GetTexture(
            Plugin.ModEnvironment.OriginPath + "/Sprites/" + FloodWeatherUISpecification.InProgressClass + ".png"),
          originalSprite.rect, originalSprite.pivot);

    ____simpleProgressBar.SetFieldValue(
      "_image",
      ____hazardousWeatherClass == FloodWeatherUISpecification.InProgressClass ? floodSprite : originalSprite);
  }
}
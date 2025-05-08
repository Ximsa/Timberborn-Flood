using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Timberborn.FloodSeason;

public static class TextureProvider
{
  private static readonly Dictionary<string, Texture2D> CachedTextures = new();

  public static Texture2D GetTexture(string path)
  {
    if (CachedTextures.TryGetValue(path, out var cachedTexture)) return cachedTexture;

    var texture = new Texture2D(1, 1);
    var data = File.ReadAllBytes(path);
    texture.LoadImage(data, false);
    CachedTextures.Add(path, texture);
    return texture;
  }
}
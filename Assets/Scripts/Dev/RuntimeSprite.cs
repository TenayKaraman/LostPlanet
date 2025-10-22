using System.Collections.Generic;
using UnityEngine;

namespace LostPlanet.Dev
{
    /// <summary>
    /// Basit, runtime kare sprite üretici ve atayıcı.
    /// 1x1 world unit boyutunda placeholder görsel ekler.
    /// </summary>
    public static class RuntimeSprite
    {
        static readonly Dictionary<Color, Sprite> _cache = new Dictionary<Color, Sprite>();

        public static SpriteRenderer EnsureRenderer(GameObject go, Color color, int sortingOrder = 0, float worldSize = 1f)
        {
            var sr = go.GetComponent<SpriteRenderer>();
            if (!sr) sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetOrBuild(color);
            sr.sortingOrder = sortingOrder;
            // 16 px = 1 world unit olacak şekilde sprite üretildi, o yüzden scale = worldSize yeterli
            go.transform.localScale = new Vector3(worldSize, worldSize, 1f);
            return sr;
        }

        static Sprite GetOrBuild(Color c)
        {
            if (_cache.TryGetValue(c, out var sp)) return sp;
            var built = BuildSolidSprite(c, 16);
            _cache[c] = built;
            return built;
        }

        static Sprite BuildSolidSprite(Color color, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();

            // pixelsPerUnit = size → 1 world unit = tam kare
            var rect = new Rect(0, 0, size, size);
            var pivot = new Vector2(0.5f, 0.5f);
            var sprite = Sprite.Create(tex, rect, pivot, size);
            sprite.name = $"RuntimeSprite_{ColorUtility.ToHtmlStringRGB(color)}";
            return sprite;
        }
    }
}

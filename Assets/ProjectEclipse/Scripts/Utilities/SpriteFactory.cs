using System.Collections.Generic;
using UnityEngine;

namespace ProjectEclipse.Utilities
{
    public static class SpriteFactory
    {
        private static readonly Dictionary<Color32, Sprite> SquareSprites = new Dictionary<Color32, Sprite>();

        public static Sprite GetSquareSprite(Color color)
        {
            Color32 key = color;
            Sprite sprite;
            if (SquareSprites.TryGetValue(key, out sprite))
            {
                return sprite;
            }

            Texture2D texture = new Texture2D(16, 16);
            texture.filterMode = FilterMode.Point;
            Color[] pixels = new Color[16 * 16];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            texture.name = "Runtime Placeholder";
            sprite = Sprite.Create(texture, new Rect(0f, 0f, 16f, 16f), new Vector2(0.5f, 0.5f), 16f);
            SquareSprites[key] = sprite;
            return sprite;
        }
    }
}


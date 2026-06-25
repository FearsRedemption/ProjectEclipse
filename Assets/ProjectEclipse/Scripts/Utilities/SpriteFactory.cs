using System.Collections.Generic;
using UnityEngine;

namespace ProjectEclipse.Utilities
{
    public static class SpriteFactory
    {
        private static readonly Dictionary<Color32, Sprite> SquareSprites = new Dictionary<Color32, Sprite>();
        private static readonly Dictionary<Color32, Sprite> ItemDropSprites = new Dictionary<Color32, Sprite>();
        private static Sprite slashSprite;
        private static Sprite sparkleSprite;
        private static Sprite portalSprite;
        private static Sprite creatureSilhouetteSprite;
        private static Sprite roomBackgroundSprite;
        private static Sprite groundFillSprite;
        private static Sprite platformStripSprite;

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
            texture.name = "Runtime Solid";
            sprite = Sprite.Create(texture, new Rect(0f, 0f, 16f, 16f), new Vector2(0.5f, 0.5f), 16f);
            SquareSprites[key] = sprite;
            return sprite;
        }

        public static Sprite GetItemDropSprite(Color color)
        {
            Color32 key = color;
            Sprite sprite;
            if (ItemDropSprites.TryGetValue(key, out sprite))
            {
                return sprite;
            }

            int size = 48;
            Texture2D texture = CreateTransparentTexture(size, size, "Runtime Item Drop");
            Color[] pixels = new Color[size * size];
            Color rim = Color.Lerp(color, Color.white, 0.55f);
            Color shadow = Color.Lerp(color, Color.black, 0.35f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = Mathf.Abs((x + 0.5f) / size * 2f - 1f);
                    float ny = Mathf.Abs((y + 0.5f) / size * 2f - 1f);
                    float diamond = nx + ny;
                    Color pixel = Color.clear;
                    if (diamond <= 0.98f)
                    {
                        pixel = Color.Lerp(shadow, rim, (float)y / size);
                        pixel.a = 1f;
                    }
                    else if (diamond <= 1.12f)
                    {
                        pixel = new Color(1f, 1f, 1f, 0.55f);
                    }

                    pixels[y * size + x] = pixel;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            sprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            ItemDropSprites[key] = sprite;
            return sprite;
        }

        public static Sprite GetSlashSprite()
        {
            if (slashSprite != null)
            {
                return slashSprite;
            }

            int width = 96;
            int height = 48;
            Texture2D texture = CreateTransparentTexture(width, height, "Runtime Slash");
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = (x + 0.5f) / width * 2f - 1f;
                    float ny = (y + 0.5f) / height * 2f - 1f;
                    float outer = nx * nx + ny * ny * 2.8f;
                    float inner = (nx + 0.18f) * (nx + 0.18f) + ny * ny * 4.2f;
                    Color pixel = Color.clear;
                    if (outer <= 1f && inner >= 0.34f && nx > -0.86f)
                    {
                        float edge = Mathf.Clamp01((1f - outer) * 3.2f);
                        float bright = Mathf.Clamp01((inner - 0.34f) * 2.4f);
                        float alpha = Mathf.Clamp01(0.25f + edge * 0.55f + bright * 0.28f);
                        pixel = new Color(1f, 1f, 1f, alpha);
                    }

                    pixels[y * width + x] = pixel;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            slashSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 48f);
            return slashSprite;
        }

        public static Sprite GetSparkleSprite()
        {
            if (sparkleSprite != null)
            {
                return sparkleSprite;
            }

            int size = 32;
            Texture2D texture = CreateTransparentTexture(size, size, "Runtime Sparkle");
            Color[] pixels = new Color[size * size];
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - center.x);
                    float dy = Mathf.Abs(y - center.y);
                    float cross = Mathf.Min(dx * 0.34f + dy, dy * 0.34f + dx);
                    float diagonal = Mathf.Abs(dx - dy) + Mathf.Min(dx, dy) * 0.45f;
                    float alpha = Mathf.Clamp01(1.1f - Mathf.Min(cross, diagonal) / 8.5f);
                    if (alpha < 0.05f)
                    {
                        alpha = 0f;
                    }

                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            sparkleSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 32f);
            return sparkleSprite;
        }

        public static Sprite GetPortalSprite()
        {
            if (portalSprite != null)
            {
                return portalSprite;
            }

            int width = 64;
            int height = 96;
            Texture2D texture = CreateTransparentTexture(width, height, "Runtime Portal");
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = (x + 0.5f) / width * 2f - 1f;
                    float ny = (y + 0.5f) / height * 2f - 1f;
                    float ellipse = nx * nx * 1.25f + ny * ny;
                    Color pixel = Color.clear;
                    if (ellipse <= 1f)
                    {
                        float ring = Mathf.Abs(ellipse - 0.78f);
                        float swirl = Mathf.Sin((nx + ny * 0.5f) * 12f) * 0.08f;
                        float alpha = ring < 0.16f ? 0.95f : 0.22f + swirl;
                        pixel = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
                    }

                    pixels[y * width + x] = pixel;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            portalSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.42f), 64f);
            return portalSprite;
        }

        public static Sprite GetCreatureSilhouetteSprite()
        {
            if (creatureSilhouetteSprite != null)
            {
                return creatureSilhouetteSprite;
            }

            int size = 64;
            Texture2D texture = CreateTransparentTexture(size, size, "Runtime Creature Silhouette");
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (x + 0.5f) / size * 2f - 1f;
                    float ny = (y + 0.5f) / size * 2f - 1f;
                    bool body = nx * nx * 1.35f + (ny + 0.16f) * (ny + 0.16f) * 1.9f < 0.72f;
                    bool head = nx * nx * 2.2f + (ny - 0.42f) * (ny - 0.42f) * 2.7f < 0.26f;
                    bool footLeft = (nx + 0.32f) * (nx + 0.32f) * 6f + (ny + 0.76f) * (ny + 0.76f) * 18f < 0.18f;
                    bool footRight = (nx - 0.32f) * (nx - 0.32f) * 6f + (ny + 0.76f) * (ny + 0.76f) * 18f < 0.18f;
                    pixels[y * size + x] = body || head || footLeft || footRight ? Color.white : Color.clear;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            creatureSilhouetteSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.08f), 64f);
            return creatureSilhouetteSprite;
        }

        public static Sprite GetRoomBackgroundSprite()
        {
            if (roomBackgroundSprite != null)
            {
                return roomBackgroundSprite;
            }

            int width = 128;
            int height = 96;
            Texture2D texture = CreateTransparentTexture(width, height, "Runtime Room Background");
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                float vertical = (float)y / (height - 1);
                for (int x = 0; x < width; x++)
                {
                    float wave = Mathf.Sin((x * 0.08f) + vertical * 3.5f) * 0.035f;
                    float trunk = IsBackgroundTrunk(x, y) ? 0.18f : 0f;
                    float canopy = IsBackgroundCanopy(x, y) ? 0.12f : 0f;
                    float value = 0.58f + vertical * 0.22f + wave - trunk + canopy;
                    pixels[y * width + x] = new Color(value, value, value, 1f);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            roomBackgroundSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 32f);
            return roomBackgroundSprite;
        }

        public static Sprite GetGroundFillSprite()
        {
            if (groundFillSprite != null)
            {
                return groundFillSprite;
            }

            int width = 128;
            int height = 64;
            Texture2D texture = CreateTransparentTexture(width, height, "Runtime Ground Fill");
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                float vertical = (float)y / (height - 1);
                for (int x = 0; x < width; x++)
                {
                    bool topLip = y > height - 9;
                    bool pebble = ((x * 17 + y * 31) % 43) < 3;
                    float value = topLip ? 0.86f : Mathf.Lerp(0.48f, 0.66f, vertical);
                    if (pebble)
                    {
                        value += 0.08f;
                    }

                    pixels[y * width + x] = new Color(value, value, value, 1f);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            groundFillSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 32f);
            return groundFillSprite;
        }

        public static Sprite GetPlatformStripSprite()
        {
            if (platformStripSprite != null)
            {
                return platformStripSprite;
            }

            int width = 128;
            int height = 32;
            Texture2D texture = CreateTransparentTexture(width, height, "Runtime Platform Strip");
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                float vertical = (float)y / (height - 1);
                for (int x = 0; x < width; x++)
                {
                    bool top = y > height - 6;
                    bool seam = x % 24 == 0 || x % 24 == 1;
                    float grain = Mathf.Sin(x * 0.28f + y * 0.2f) * 0.06f;
                    float value = top ? 0.88f : Mathf.Lerp(0.42f, 0.64f, vertical) + grain;
                    if (seam && !top)
                    {
                        value -= 0.1f;
                    }

                    pixels[y * width + x] = new Color(value, value, value, 1f);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            platformStripSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.68f), 32f);
            return platformStripSprite;
        }

        private static Texture2D CreateTransparentTexture(int width, int height, string name)
        {
            Texture2D texture = new Texture2D(width, height);
            texture.name = name;
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.filterMode = FilterMode.Bilinear;
            return texture;
        }

        private static bool IsBackgroundTrunk(int x, int y)
        {
            int trunkA = Mathf.Abs(x - 24);
            int trunkB = Mathf.Abs(x - 86);
            return (trunkA < 4 && y < 76) || (trunkB < 5 && y < 68);
        }

        private static bool IsBackgroundCanopy(int x, int y)
        {
            float a = (x - 24) * (x - 24) * 0.006f + (y - 76) * (y - 76) * 0.018f;
            float b = (x - 86) * (x - 86) * 0.005f + (y - 70) * (y - 70) * 0.016f;
            return a < 1f || b < 1f;
        }
    }
}

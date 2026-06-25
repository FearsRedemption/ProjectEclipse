using System.Collections.Generic;
using ProjectEclipse.Equipment;
using UnityEngine;

namespace ProjectEclipse.Utilities
{
    public static class SpriteFactory
    {
        private static readonly Dictionary<Color32, Sprite> SquareSprites = new Dictionary<Color32, Sprite>();
        private static readonly Dictionary<Color32, Sprite> ItemDropSprites = new Dictionary<Color32, Sprite>();
        private static Sprite slashSprite;
        private static Sprite shoutWaveSprite;
        private static Sprite enemyProjectileSprite;
        private static Sprite sparkleSprite;
        private static Sprite portalSprite;
        private static Sprite portalColumnSprite;
        private static Sprite portalPadSprite;
        private static Sprite creatureSilhouetteSprite;
        private static Sprite roomBackgroundSprite;
        private static Sprite groundFillSprite;
        private static Sprite platformStripSprite;
        private static Sprite playerBaseBodySprite;
        private static Sprite playerUndershirtSprite;
        private static Sprite playerShortsSprite;
        private static Sprite playerHairFaceSprite;
        private static readonly Dictionary<string, Sprite> EquipmentOverlaySprites = new Dictionary<string, Sprite>();

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

        public static Sprite GetShoutWaveSprite()
        {
            if (shoutWaveSprite != null)
            {
                return shoutWaveSprite;
            }

            int size = 96;
            Texture2D texture = CreateTransparentTexture(size, size, "Runtime Shout Wave");
            Color[] pixels = new Color[size * size];
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center) / (size * 0.5f);
                    float ring = Mathf.Abs(distance - 0.72f);
                    float innerRing = Mathf.Abs(distance - 0.38f);
                    float alpha = 0f;
                    if (ring < 0.08f)
                    {
                        alpha = Mathf.Clamp01(1f - ring / 0.08f) * 0.72f;
                    }
                    else if (innerRing < 0.04f)
                    {
                        alpha = Mathf.Clamp01(1f - innerRing / 0.04f) * 0.36f;
                    }

                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            shoutWaveSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 48f);
            return shoutWaveSprite;
        }

        public static Sprite GetEnemyProjectileSprite()
        {
            if (enemyProjectileSprite != null)
            {
                return enemyProjectileSprite;
            }

            int size = 48;
            Texture2D texture = CreateTransparentTexture(size, size, "Runtime Enemy Projectile");
            Color[] pixels = new Color[size * size];
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 offset = new Vector2(x, y) - center;
                    float distance = offset.magnitude / (size * 0.5f);
                    float tail = Mathf.Clamp01((center.x - x) / (size * 0.48f));
                    Color pixel = Color.clear;
                    if (distance < 0.42f)
                    {
                        pixel = new Color(1f, 1f, 1f, Mathf.Clamp01(1f - distance * 1.3f));
                    }
                    else if (y > size * 0.38f && y < size * 0.62f && x < center.x && tail > 0.1f)
                    {
                        pixel = new Color(1f, 1f, 1f, tail * 0.45f);
                    }

                    pixels[y * size + x] = pixel;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            enemyProjectileSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 48f);
            return enemyProjectileSprite;
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

        public static Sprite GetPortalColumnSprite()
        {
            if (portalColumnSprite != null)
            {
                return portalColumnSprite;
            }

            int width = 64;
            int height = 112;
            Texture2D texture = CreateTransparentTexture(width, height, "Runtime Portal Column");
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = (x + 0.5f) / width * 2f - 1f;
                    float vertical = (float)y / (height - 1);
                    bool column = Mathf.Abs(nx) < 0.28f && y > 14 && y < 100;
                    bool cap = (Mathf.Abs(nx) < 0.48f && ((y > 94 && y < 106) || (y > 8 && y < 20)));
                    bool core = nx * nx * 1.7f + (vertical * 2f - 1.05f) * (vertical * 2f - 1.05f) < 0.52f;
                    Color pixel = Color.clear;
                    if (column || cap)
                    {
                        float light = Mathf.Lerp(0.42f, 0.84f, Mathf.Clamp01((nx + 1f) * 0.5f));
                        pixel = new Color(light, light, light, 1f);
                    }
                    else if (core)
                    {
                        pixel = new Color(1f, 1f, 1f, 0.42f);
                    }

                    pixels[y * width + x] = pixel;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            portalColumnSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.08f), 64f);
            return portalColumnSprite;
        }

        public static Sprite GetPortalPadSprite()
        {
            if (portalPadSprite != null)
            {
                return portalPadSprite;
            }

            int width = 96;
            int height = 36;
            Texture2D texture = CreateTransparentTexture(width, height, "Runtime Portal Pad");
            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float nx = (x + 0.5f) / width * 2f - 1f;
                    float ny = (y + 0.5f) / height * 2f - 1f;
                    float ellipse = nx * nx + ny * ny * 4.2f;
                    Color pixel = Color.clear;
                    if (ellipse <= 1f)
                    {
                        float rim = ellipse > 0.72f ? 0.88f : 0.54f;
                        pixel = new Color(rim, rim, rim, 1f);
                    }
                    else if (ellipse <= 1.22f)
                    {
                        pixel = new Color(1f, 1f, 1f, 0.28f);
                    }

                    pixels[y * width + x] = pixel;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            portalPadSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 64f);
            return portalPadSprite;
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

        public static Sprite GetPlayerBaseBodySprite()
        {
            if (playerBaseBodySprite == null)
            {
                playerBaseBodySprite = CreatePlayerLayerSprite("Runtime Player Base Body", DrawPlayerBaseBody);
            }

            return playerBaseBodySprite;
        }

        public static Sprite GetPlayerUndershirtSprite()
        {
            if (playerUndershirtSprite == null)
            {
                playerUndershirtSprite = CreatePlayerLayerSprite("Runtime Player Undershirt", DrawPlayerUndershirt);
            }

            return playerUndershirtSprite;
        }

        public static Sprite GetPlayerShortsSprite()
        {
            if (playerShortsSprite == null)
            {
                playerShortsSprite = CreatePlayerLayerSprite("Runtime Player Shorts", DrawPlayerShorts);
            }

            return playerShortsSprite;
        }

        public static Sprite GetPlayerHairFaceSprite()
        {
            if (playerHairFaceSprite == null)
            {
                playerHairFaceSprite = CreatePlayerLayerSprite("Runtime Player Hair Face", DrawPlayerHairFace);
            }

            return playerHairFaceSprite;
        }

        public static Sprite GetEquipmentOverlaySprite(EquipmentSlot slot, Color color)
        {
            Color32 keyColor = color;
            string key = slot + "_" + keyColor.r + "_" + keyColor.g + "_" + keyColor.b + "_" + keyColor.a;
            Sprite sprite;
            if (EquipmentOverlaySprites.TryGetValue(key, out sprite))
            {
                return sprite;
            }

            sprite = CreatePlayerLayerSprite("Runtime " + slot + " Overlay", pixels => DrawEquipmentOverlay(pixels, slot, color));
            EquipmentOverlaySprites[key] = sprite;
            return sprite;
        }

        private static Texture2D CreateTransparentTexture(int width, int height, string name)
        {
            Texture2D texture = new Texture2D(width, height);
            texture.name = name;
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.filterMode = FilterMode.Bilinear;
            return texture;
        }

        private delegate void PlayerLayerPainter(Color[] pixels);

        private static Sprite CreatePlayerLayerSprite(string name, PlayerLayerPainter painter)
        {
            int width = 96;
            int height = 96;
            Texture2D texture = CreateTransparentTexture(width, height, name);
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }

            painter(pixels);
            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.08f), 96f);
        }

        private static void DrawPlayerBaseBody(Color[] pixels)
        {
            Color skin = new Color(0.78f, 0.57f, 0.42f, 1f);
            Color skinShade = new Color(0.61f, 0.39f, 0.29f, 1f);
            FillEllipse(pixels, 48, 69, 12, 14, skin);
            FillRect(pixels, 44, 52, 52, 59, skin);
            FillEllipse(pixels, 48, 43, 10, 16, skin);
            FillEllipse(pixels, 37, 43, 5, 19, skinShade);
            FillEllipse(pixels, 59, 43, 5, 19, skinShade);
            FillEllipse(pixels, 42, 24, 5, 18, skin);
            FillEllipse(pixels, 54, 24, 5, 18, skin);
            FillEllipse(pixels, 42, 8, 5, 4, skinShade);
            FillEllipse(pixels, 54, 8, 5, 4, skinShade);
        }

        private static void DrawPlayerUndershirt(Color[] pixels)
        {
            Color cloth = new Color(0.92f, 0.94f, 0.88f, 1f);
            Color trim = new Color(0.62f, 0.7f, 0.66f, 1f);
            FillEllipse(pixels, 48, 45, 11, 13, cloth);
            FillRect(pixels, 40, 34, 56, 49, cloth);
            FillRect(pixels, 41, 50, 55, 52, trim);
            FillRect(pixels, 40, 33, 56, 35, trim);
        }

        private static void DrawPlayerShorts(Color[] pixels)
        {
            Color cloth = new Color(0.36f, 0.45f, 0.49f, 1f);
            Color trim = new Color(0.18f, 0.25f, 0.28f, 1f);
            FillRect(pixels, 38, 25, 58, 34, cloth);
            FillEllipse(pixels, 43, 24, 6, 8, cloth);
            FillEllipse(pixels, 53, 24, 6, 8, cloth);
            FillRect(pixels, 39, 33, 57, 35, trim);
        }

        private static void DrawPlayerHairFace(Color[] pixels)
        {
            Color hair = new Color(0.34f, 0.16f, 0.08f, 1f);
            Color eye = new Color(0.08f, 0.1f, 0.12f, 1f);
            FillEllipse(pixels, 48, 76, 13, 8, hair);
            FillEllipse(pixels, 39, 70, 5, 7, hair);
            FillEllipse(pixels, 57, 70, 5, 7, hair);
            FillRect(pixels, 43, 69, 45, 71, eye);
            FillRect(pixels, 51, 69, 53, 71, eye);
        }

        private static void DrawEquipmentOverlay(Color[] pixels, EquipmentSlot slot, Color color)
        {
            Color fill = Color.Lerp(color, Color.white, 0.18f);
            Color shade = Color.Lerp(color, Color.black, 0.28f);
            switch (slot)
            {
                case EquipmentSlot.Helmet:
                    FillEllipse(pixels, 48, 75, 13, 7, fill);
                    FillRect(pixels, 36, 68, 60, 74, fill);
                    FillRect(pixels, 38, 67, 58, 69, shade);
                    break;
                case EquipmentSlot.Chest:
                    FillEllipse(pixels, 48, 43, 13, 15, fill);
                    FillRect(pixels, 37, 31, 59, 47, fill);
                    FillRect(pixels, 39, 45, 57, 48, shade);
                    break;
                case EquipmentSlot.Gloves:
                    FillEllipse(pixels, 36, 25, 5, 6, fill);
                    FillEllipse(pixels, 60, 25, 5, 6, fill);
                    break;
                case EquipmentSlot.Boots:
                    FillEllipse(pixels, 41, 7, 6, 4, fill);
                    FillEllipse(pixels, 55, 7, 6, 4, fill);
                    FillRect(pixels, 37, 9, 45, 14, shade);
                    FillRect(pixels, 51, 9, 59, 14, shade);
                    break;
                case EquipmentSlot.Offhand:
                    FillEllipse(pixels, 64, 40, 8, 15, fill);
                    FillRect(pixels, 63, 29, 66, 51, shade);
                    break;
                case EquipmentSlot.Back:
                    FillEllipse(pixels, 48, 36, 17, 28, fill);
                    FillRect(pixels, 37, 50, 59, 55, shade);
                    break;
                case EquipmentSlot.Necklace:
                case EquipmentSlot.Belt:
                case EquipmentSlot.Ring1:
                case EquipmentSlot.Ring2:
                case EquipmentSlot.Earring1:
                case EquipmentSlot.Earring2:
                    FillRect(pixels, 39, 53, 57, 55, fill);
                    FillRect(pixels, 46, 50, 50, 54, shade);
                    break;
                case EquipmentSlot.Mainhand:
                    FillRotatedBlade(pixels, fill, shade);
                    break;
            }
        }

        private static void FillRotatedBlade(Color[] pixels, Color fill, Color shade)
        {
            for (int y = 20; y < 70; y++)
            {
                int x = 63 + (y - 20) / 8;
                FillRect(pixels, x, y, x + 3, y + 2, fill);
            }

            FillRect(pixels, 59, 28, 69, 31, shade);
            FillRect(pixels, 60, 22, 64, 29, shade);
        }

        private static void FillEllipse(Color[] pixels, int centerX, int centerY, int radiusX, int radiusY, Color color)
        {
            int width = 96;
            int minX = Mathf.Max(0, centerX - radiusX);
            int maxX = Mathf.Min(width - 1, centerX + radiusX);
            int minY = Mathf.Max(0, centerY - radiusY);
            int maxY = Mathf.Min(width - 1, centerY + radiusY);
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float nx = (x - centerX) / Mathf.Max(1f, radiusX);
                    float ny = (y - centerY) / Mathf.Max(1f, radiusY);
                    if (nx * nx + ny * ny <= 1f)
                    {
                        pixels[y * width + x] = color;
                    }
                }
            }
        }

        private static void FillRect(Color[] pixels, int minX, int minY, int maxX, int maxY, Color color)
        {
            int width = 96;
            int x0 = Mathf.Clamp(minX, 0, width - 1);
            int x1 = Mathf.Clamp(maxX, 0, width - 1);
            int y0 = Mathf.Clamp(minY, 0, width - 1);
            int y1 = Mathf.Clamp(maxY, 0, width - 1);
            for (int y = y0; y <= y1; y++)
            {
                for (int x = x0; x <= x1; x++)
                {
                    pixels[y * width + x] = color;
                }
            }
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

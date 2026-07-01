using System.Collections.Generic;
using ProjectEclipse.Enemies;
using ProjectEclipse.Equipment;
using ProjectEclipse.Items;
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
        private static readonly Dictionary<string, Sprite> CreatureSprites = new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, Sprite> WeaponOverlaySprites = new Dictionary<string, Sprite>();
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

        public static Sprite GetCreatureSprite(EnemyDefinition definition)
        {
            if (definition == null)
            {
                return GetCreatureSilhouetteSprite();
            }

            string id = definition.EnemyId.ToLowerInvariant();
            string label = definition.DisplayName.ToLowerInvariant();
            string key = id + "_" + label;
            Sprite sprite;
            if (CreatureSprites.TryGetValue(key, out sprite))
            {
                return sprite;
            }

            int stage;
            Color first;
            Color second;
            Color third;
            if (TryGetTreeCreatureVisual(key, out stage, out first, out second, out third))
            {
                sprite = CreateCreatureLayerSprite("Runtime " + definition.DisplayName, pixels => DrawTreeCreature(pixels, stage, first, second, third));
            }
            else if (TryGetOreCreatureVisual(key, out stage, out first, out second, out third))
            {
                sprite = CreateCreatureLayerSprite("Runtime " + definition.DisplayName, pixels => DrawOreCreature(pixels, stage, first, second, third));
            }
            else if (key.Contains("route_gate") || key.Contains("sentinel") || key.Contains("boss"))
            {
                sprite = CreateCreatureLayerSprite("Runtime Route Gate Sentinel", DrawRouteGateSentinel);
            }
            else if (key.Contains("ore_node") || key.Contains("ore node") || key.Contains("node"))
            {
                sprite = CreateCreatureLayerSprite("Runtime Copper Ore Node", DrawCopperOreNode);
            }
            else if (key.Contains("oreling"))
            {
                sprite = CreateCreatureLayerSprite("Runtime Copper Oreling", DrawCopperOreling);
            }
            else if (key.Contains("orelet") || key.Contains("copper"))
            {
                sprite = CreateCreatureLayerSprite("Runtime Copper Orelet", DrawCopperOrelet);
            }
            else
            {
                sprite = GetCreatureSilhouetteSprite();
            }

            CreatureSprites[key] = sprite;
            return sprite;
        }

        private static bool TryGetTreeCreatureVisual(string key, out int stage, out Color bark, out Color leaf, out Color light)
        {
            stage = 0;
            bark = new Color(0.42f, 0.28f, 0.15f, 1f);
            leaf = new Color(0.3f, 0.62f, 0.25f, 1f);
            light = new Color(0.62f, 0.86f, 0.42f, 1f);

            if (key.Contains("birch"))
            {
                bark = new Color(0.74f, 0.7f, 0.54f, 1f);
                leaf = new Color(0.42f, 0.65f, 0.34f, 1f);
                light = new Color(0.75f, 0.9f, 0.48f, 1f);
                stage = GetTreeStage(key);
                return true;
            }

            if (key.Contains("pine"))
            {
                bark = new Color(0.38f, 0.25f, 0.13f, 1f);
                leaf = new Color(0.2f, 0.52f, 0.26f, 1f);
                light = new Color(0.48f, 0.76f, 0.32f, 1f);
                stage = GetTreeStage(key);
                return true;
            }

            if (key.Contains("sapling"))
            {
                bark = new Color(0.36f, 0.22f, 0.12f, 1f);
                leaf = new Color(0.28f, 0.66f, 0.27f, 1f);
                light = new Color(0.65f, 0.9f, 0.38f, 1f);
                stage = 0;
                return true;
            }

            return false;
        }

        private static int GetTreeStage(string key)
        {
            if (key.Contains("tree"))
            {
                return 2;
            }

            if (key.Contains("ling"))
            {
                return 1;
            }

            return 0;
        }

        private static bool TryGetOreCreatureVisual(string key, out int stage, out Color rock, out Color ore, out Color light)
        {
            stage = GetOreStage(key);
            rock = new Color(0.48f, 0.46f, 0.42f, 1f);
            ore = new Color(0.88f, 0.36f, 0.12f, 1f);
            light = new Color(1f, 0.62f, 0.24f, 1f);

            if (key.Contains("coal"))
            {
                rock = new Color(0.16f, 0.16f, 0.17f, 1f);
                ore = new Color(0.74f, 0.52f, 0.3f, 1f);
                light = new Color(1f, 0.72f, 0.38f, 1f);
                return true;
            }

            if (key.Contains("tin"))
            {
                rock = new Color(0.5f, 0.53f, 0.53f, 1f);
                ore = new Color(0.78f, 0.88f, 0.88f, 1f);
                light = new Color(0.94f, 1f, 1f, 1f);
                return true;
            }

            if (key.Contains("zync") || key.Contains("zinc"))
            {
                rock = new Color(0.43f, 0.48f, 0.43f, 1f);
                ore = new Color(0.68f, 0.9f, 0.62f, 1f);
                light = new Color(0.86f, 1f, 0.78f, 1f);
                return true;
            }

            if (key.Contains("iron"))
            {
                rock = new Color(0.43f, 0.41f, 0.38f, 1f);
                ore = new Color(0.72f, 0.47f, 0.3f, 1f);
                light = new Color(0.9f, 0.66f, 0.46f, 1f);
                return true;
            }

            if (key.Contains("rock") || key.Contains("stone"))
            {
                rock = new Color(0.5f, 0.52f, 0.54f, 1f);
                ore = new Color(0.7f, 0.73f, 0.76f, 1f);
                light = new Color(0.88f, 0.9f, 0.92f, 1f);
                return true;
            }

            if (key.Contains("copper") || key.Contains("orelet") || key.Contains("oreling") || key.Contains("ore_node") || key.Contains("ore node"))
            {
                return true;
            }

            return false;
        }

        private static int GetOreStage(string key)
        {
            if (key.Contains("node"))
            {
                return 2;
            }

            if (key.Contains("ling"))
            {
                return 1;
            }

            return 0;
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

        public static Sprite GetWeaponOverlaySprite(WeaponDefinition weapon)
        {
            if (weapon == null)
            {
                return null;
            }

            string key = weapon.ItemId + "_" + weapon.PlaceholderColor;
            Sprite sprite;
            if (WeaponOverlaySprites.TryGetValue(key, out sprite))
            {
                return sprite;
            }

            sprite = CreatePlayerLayerSprite("Runtime " + weapon.DisplayName + " PaperDoll", pixels => DrawWeaponOverlay(pixels, weapon));
            WeaponOverlaySprites[key] = sprite;
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

        private delegate void CreatureLayerPainter(Color[] pixels);

        private static Sprite CreateCreatureLayerSprite(string name, CreatureLayerPainter painter)
        {
            int width = 96;
            int height = 96;
            Texture2D texture = CreateTransparentTexture(width, height, name);
            texture.filterMode = FilterMode.Point;
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

        private static void DrawOreCreature(Color[] pixels, int stage, Color rock, Color ore, Color oreLight)
        {
            Color outline = Color.Lerp(rock, Color.black, 0.72f);
            Color rockShade = Color.Lerp(rock, Color.black, 0.28f);
            Color rockLight = Color.Lerp(rock, Color.white, 0.3f);
            Color eye = new Color(1f, 0.86f, 0.45f, 1f);

            if (stage <= 0)
            {
                FillEllipse(pixels, 48, 31, 18, 16, outline);
                FillEllipse(pixels, 48, 32, 16, 14, rock);
                FillEllipse(pixels, 38, 18, 7, 7, outline);
                FillEllipse(pixels, 58, 18, 7, 7, outline);
                FillEllipse(pixels, 38, 19, 5, 5, rockLight);
                FillEllipse(pixels, 58, 19, 5, 5, rockShade);
                FillRect(pixels, 42, 42, 48, 45, outline);
                FillRect(pixels, 49, 42, 55, 45, outline);
                FillRect(pixels, 42, 43, 48, 45, rock);
                FillRect(pixels, 49, 43, 55, 45, rockShade);
                FillEllipse(pixels, 44, 49, 4, 4, ore);
                FillEllipse(pixels, 56, 41, 3, 3, oreLight);
                FillRect(pixels, 43, 28, 53, 31, outline);
                FillRect(pixels, 45, 29, 47, 30, eye);
                FillRect(pixels, 50, 29, 52, 30, eye);
                return;
            }

            if (stage == 1)
            {
                FillEllipse(pixels, 48, 38, 23, 22, outline);
                FillEllipse(pixels, 48, 39, 20, 19, rock);
                FillEllipse(pixels, 36, 24, 10, 9, outline);
                FillEllipse(pixels, 60, 24, 10, 9, outline);
                FillEllipse(pixels, 36, 25, 8, 7, rockShade);
                FillEllipse(pixels, 60, 25, 8, 7, rockLight);
                FillEllipse(pixels, 31, 40, 7, 13, outline);
                FillEllipse(pixels, 65, 40, 7, 13, outline);
                FillEllipse(pixels, 31, 40, 5, 11, rock);
                FillEllipse(pixels, 65, 40, 5, 11, rockShade);
                FillRect(pixels, 38, 52, 46, 58, outline);
                FillRect(pixels, 51, 52, 59, 58, outline);
                FillRect(pixels, 39, 53, 46, 58, rockShade);
                FillRect(pixels, 51, 53, 58, 58, rock);
                FillEllipse(pixels, 39, 45, 5, 5, ore);
                FillEllipse(pixels, 54, 48, 6, 6, ore);
                FillEllipse(pixels, 59, 36, 5, 5, oreLight);
                FillEllipse(pixels, 47, 58, 4, 4, ore);
                FillRect(pixels, 41, 36, 55, 39, outline);
                FillRect(pixels, 44, 37, 47, 38, eye);
                FillRect(pixels, 51, 37, 54, 38, eye);
                return;
            }

            FillEllipse(pixels, 48, 30, 30, 24, outline);
            FillEllipse(pixels, 48, 31, 27, 21, rock);
            FillEllipse(pixels, 29, 27, 14, 15, outline);
            FillEllipse(pixels, 68, 26, 15, 16, outline);
            FillEllipse(pixels, 29, 28, 12, 13, rockLight);
            FillEllipse(pixels, 68, 27, 13, 14, rockShade);
            FillEllipse(pixels, 43, 50, 22, 10, outline);
            FillEllipse(pixels, 43, 51, 20, 8, rock);
            FillEllipse(pixels, 62, 48, 18, 10, outline);
            FillEllipse(pixels, 62, 49, 16, 8, rockLight);
            FillEllipse(pixels, 31, 48, 9, 7, ore);
            FillEllipse(pixels, 45, 55, 8, 7, oreLight);
            FillEllipse(pixels, 57, 40, 10, 9, ore);
            FillEllipse(pixels, 67, 57, 7, 6, oreLight);
            FillEllipse(pixels, 74, 31, 6, 7, ore);
            FillEllipse(pixels, 42, 30, 6, 6, oreLight);
            FillEllipse(pixels, 25, 31, 5, 5, ore);
            FillEllipse(pixels, 52, 20, 7, 6, ore);
            FillRect(pixels, 38, 31, 59, 35, outline);
            FillRect(pixels, 42, 32, 46, 33, eye);
            FillRect(pixels, 53, 32, 57, 33, eye);
        }

        private static void DrawTreeCreature(Color[] pixels, int stage, Color bark, Color leaf, Color light)
        {
            Color outline = Color.Lerp(bark, Color.black, 0.72f);
            Color barkShade = Color.Lerp(bark, Color.black, 0.3f);
            Color leafShade = Color.Lerp(leaf, Color.black, 0.24f);
            Color eye = new Color(1f, 0.86f, 0.42f, 1f);

            if (stage <= 0)
            {
                FillEllipse(pixels, 48, 24, 17, 15, outline);
                FillEllipse(pixels, 48, 25, 15, 13, leaf);
                FillEllipse(pixels, 39, 31, 8, 7, leafShade);
                FillEllipse(pixels, 57, 33, 9, 8, light);
                FillRect(pixels, 41, 36, 55, 57, outline);
                FillRect(pixels, 43, 37, 53, 57, bark);
                FillRect(pixels, 44, 45, 51, 49, barkShade);
                FillRect(pixels, 36, 57, 48, 62, outline);
                FillRect(pixels, 48, 57, 60, 62, outline);
                FillRect(pixels, 37, 58, 48, 60, barkShade);
                FillRect(pixels, 48, 58, 59, 60, bark);
                FillRect(pixels, 44, 39, 47, 40, eye);
                FillRect(pixels, 51, 39, 54, 40, eye);
                return;
            }

            if (stage == 1)
            {
                FillEllipse(pixels, 48, 24, 26, 18, outline);
                FillEllipse(pixels, 48, 25, 23, 15, leaf);
                FillEllipse(pixels, 32, 31, 13, 10, leafShade);
                FillEllipse(pixels, 64, 32, 14, 11, light);
                FillRect(pixels, 38, 36, 58, 67, outline);
                FillRect(pixels, 41, 37, 55, 67, bark);
                FillRect(pixels, 42, 44, 53, 49, barkShade);
                FillEllipse(pixels, 36, 55, 10, 8, outline);
                FillEllipse(pixels, 60, 55, 10, 8, outline);
                FillEllipse(pixels, 36, 55, 8, 6, barkShade);
                FillEllipse(pixels, 60, 55, 8, 6, bark);
                FillRect(pixels, 33, 67, 47, 73, outline);
                FillRect(pixels, 50, 67, 65, 73, outline);
                FillRect(pixels, 34, 68, 47, 71, barkShade);
                FillRect(pixels, 50, 68, 64, 71, bark);
                FillRect(pixels, 43, 40, 46, 41, eye);
                FillRect(pixels, 51, 40, 54, 41, eye);
                return;
            }

            FillEllipse(pixels, 47, 22, 30, 18, outline);
            FillEllipse(pixels, 47, 23, 27, 15, leaf);
            FillEllipse(pixels, 29, 31, 16, 13, leafShade);
            FillEllipse(pixels, 68, 32, 18, 14, light);
            FillEllipse(pixels, 48, 41, 24, 13, leaf);
            FillRect(pixels, 36, 43, 60, 78, outline);
            FillRect(pixels, 39, 44, 57, 78, bark);
            FillRect(pixels, 41, 48, 54, 55, barkShade);
            FillRect(pixels, 46, 44, 49, 78, Color.Lerp(bark, Color.white, 0.12f));
            FillEllipse(pixels, 30, 64, 13, 9, outline);
            FillEllipse(pixels, 66, 64, 13, 9, outline);
            FillEllipse(pixels, 30, 64, 10, 7, barkShade);
            FillEllipse(pixels, 66, 64, 10, 7, bark);
            FillRect(pixels, 28, 78, 47, 85, outline);
            FillRect(pixels, 50, 78, 69, 85, outline);
            FillRect(pixels, 30, 79, 47, 82, barkShade);
            FillRect(pixels, 50, 79, 67, 82, bark);
            FillRect(pixels, 42, 47, 46, 48, eye);
            FillRect(pixels, 51, 47, 55, 48, eye);
        }

        private static void DrawRouteGateSentinel(Color[] pixels)
        {
            Color outline = new Color(0.11f, 0.08f, 0.16f, 1f);
            Color stone = new Color(0.36f, 0.33f, 0.42f, 1f);
            Color stoneLight = new Color(0.56f, 0.5f, 0.66f, 1f);
            Color glow = new Color(0.82f, 0.66f, 1f, 1f);
            Color eye = new Color(1f, 0.86f, 0.42f, 1f);

            FillEllipse(pixels, 48, 26, 30, 19, outline);
            FillEllipse(pixels, 48, 27, 27, 16, stone);
            FillRect(pixels, 30, 32, 66, 67, outline);
            FillRect(pixels, 33, 35, 63, 67, stone);
            FillRect(pixels, 38, 39, 58, 63, stoneLight);
            FillEllipse(pixels, 30, 47, 10, 16, outline);
            FillEllipse(pixels, 66, 47, 10, 16, outline);
            FillEllipse(pixels, 30, 48, 8, 13, stone);
            FillEllipse(pixels, 66, 48, 8, 13, stone);
            FillRect(pixels, 38, 23, 58, 29, outline);
            FillRect(pixels, 40, 24, 56, 27, glow);
            FillRect(pixels, 40, 49, 56, 53, outline);
            FillRect(pixels, 43, 50, 46, 51, eye);
            FillRect(pixels, 51, 50, 54, 51, eye);
            FillEllipse(pixels, 48, 38, 8, 7, glow);
            FillEllipse(pixels, 48, 38, 4, 4, Color.white);
            FillRect(pixels, 34, 67, 44, 76, outline);
            FillRect(pixels, 52, 67, 62, 76, outline);
            FillRect(pixels, 35, 68, 43, 73, stone);
            FillRect(pixels, 53, 68, 61, 73, stone);
        }

        private static void DrawCopperOrelet(Color[] pixels)
        {
            Color outline = new Color(0.12f, 0.1f, 0.09f, 1f);
            Color rock = new Color(0.47f, 0.45f, 0.42f, 1f);
            Color rockLight = new Color(0.66f, 0.63f, 0.56f, 1f);
            Color copper = new Color(0.9f, 0.38f, 0.14f, 1f);

            FillEllipse(pixels, 48, 31, 18, 16, outline);
            FillEllipse(pixels, 48, 32, 16, 14, rock);
            FillEllipse(pixels, 38, 18, 7, 7, outline);
            FillEllipse(pixels, 58, 18, 7, 7, outline);
            FillEllipse(pixels, 38, 19, 5, 5, rockLight);
            FillEllipse(pixels, 58, 19, 5, 5, rock);
            FillRect(pixels, 42, 42, 48, 45, outline);
            FillRect(pixels, 49, 42, 55, 45, outline);
            FillRect(pixels, 42, 43, 48, 45, rock);
            FillRect(pixels, 49, 43, 55, 45, rock);
            FillEllipse(pixels, 44, 49, 4, 4, copper);
            FillEllipse(pixels, 56, 41, 3, 3, copper);
            FillRect(pixels, 43, 28, 53, 31, new Color(0.18f, 0.13f, 0.1f, 1f));
            FillRect(pixels, 45, 29, 47, 30, new Color(1f, 0.86f, 0.45f, 1f));
            FillRect(pixels, 50, 29, 52, 30, new Color(1f, 0.86f, 0.45f, 1f));
        }

        private static void DrawCopperOreling(Color[] pixels)
        {
            Color outline = new Color(0.11f, 0.09f, 0.08f, 1f);
            Color rock = new Color(0.5f, 0.47f, 0.42f, 1f);
            Color rockShade = new Color(0.34f, 0.31f, 0.28f, 1f);
            Color copper = new Color(0.88f, 0.36f, 0.12f, 1f);
            Color copperLight = new Color(1f, 0.58f, 0.22f, 1f);

            FillEllipse(pixels, 48, 38, 23, 22, outline);
            FillEllipse(pixels, 48, 39, 20, 19, rock);
            FillEllipse(pixels, 36, 24, 10, 9, outline);
            FillEllipse(pixels, 60, 24, 10, 9, outline);
            FillEllipse(pixels, 36, 25, 8, 7, rockShade);
            FillEllipse(pixels, 60, 25, 8, 7, rock);
            FillEllipse(pixels, 31, 40, 7, 13, outline);
            FillEllipse(pixels, 65, 40, 7, 13, outline);
            FillEllipse(pixels, 31, 40, 5, 11, rock);
            FillEllipse(pixels, 65, 40, 5, 11, rockShade);
            FillRect(pixels, 38, 52, 46, 58, outline);
            FillRect(pixels, 51, 52, 59, 58, outline);
            FillRect(pixels, 39, 53, 46, 58, rockShade);
            FillRect(pixels, 51, 53, 58, 58, rock);
            FillEllipse(pixels, 41, 54, 8, 5, outline);
            FillEllipse(pixels, 57, 54, 8, 5, outline);
            FillEllipse(pixels, 41, 54, 6, 3, rock);
            FillEllipse(pixels, 57, 54, 6, 3, rockShade);
            FillEllipse(pixels, 39, 45, 5, 5, copper);
            FillEllipse(pixels, 54, 48, 6, 6, copper);
            FillEllipse(pixels, 59, 36, 5, 5, copperLight);
            FillEllipse(pixels, 47, 58, 4, 4, copper);
            FillRect(pixels, 41, 36, 55, 39, new Color(0.16f, 0.11f, 0.09f, 1f));
            FillRect(pixels, 44, 37, 47, 38, new Color(1f, 0.86f, 0.45f, 1f));
            FillRect(pixels, 51, 37, 54, 38, new Color(1f, 0.86f, 0.45f, 1f));
        }

        private static void DrawCopperOreNode(Color[] pixels)
        {
            Color outline = new Color(0.1f, 0.08f, 0.07f, 1f);
            Color rock = new Color(0.43f, 0.4f, 0.37f, 1f);
            Color rockLight = new Color(0.62f, 0.58f, 0.5f, 1f);
            Color copper = new Color(0.86f, 0.32f, 0.1f, 1f);
            Color copperLight = new Color(1f, 0.64f, 0.24f, 1f);

            FillEllipse(pixels, 48, 30, 30, 24, outline);
            FillEllipse(pixels, 48, 31, 27, 21, rock);
            FillEllipse(pixels, 29, 27, 14, 15, outline);
            FillEllipse(pixels, 68, 26, 15, 16, outline);
            FillEllipse(pixels, 29, 28, 12, 13, rockLight);
            FillEllipse(pixels, 68, 27, 13, 14, rock);
            FillEllipse(pixels, 43, 50, 22, 10, outline);
            FillEllipse(pixels, 43, 51, 20, 8, rock);
            FillEllipse(pixels, 62, 48, 18, 10, outline);
            FillEllipse(pixels, 62, 49, 16, 8, rockLight);

            FillEllipse(pixels, 31, 48, 9, 7, copper);
            FillEllipse(pixels, 45, 55, 8, 7, copperLight);
            FillEllipse(pixels, 57, 40, 10, 9, copper);
            FillEllipse(pixels, 67, 57, 7, 6, copperLight);
            FillEllipse(pixels, 74, 31, 6, 7, copper);
            FillEllipse(pixels, 42, 30, 6, 6, copperLight);
            FillEllipse(pixels, 25, 31, 5, 5, copper);
            FillEllipse(pixels, 52, 20, 7, 6, copper);

            FillRect(pixels, 38, 31, 59, 35, new Color(0.15f, 0.1f, 0.08f, 1f));
            FillRect(pixels, 42, 32, 46, 33, new Color(1f, 0.86f, 0.45f, 1f));
            FillRect(pixels, 53, 32, 57, 33, new Color(1f, 0.86f, 0.45f, 1f));
        }

        private static void DrawPlayerBaseBody(Color[] pixels)
        {
            Color outline = new Color(0.19f, 0.1f, 0.08f, 1f);
            Color skin = new Color(0.83f, 0.61f, 0.43f, 1f);
            Color skinLight = new Color(0.96f, 0.74f, 0.53f, 1f);
            Color skinShade = new Color(0.57f, 0.34f, 0.24f, 1f);

            FillEllipse(pixels, 42, 21, 6, 17, outline);
            FillEllipse(pixels, 54, 21, 6, 17, outline);
            FillEllipse(pixels, 42, 22, 4, 15, skin);
            FillEllipse(pixels, 54, 22, 4, 15, skin);
            FillEllipse(pixels, 39, 8, 7, 5, outline);
            FillEllipse(pixels, 55, 8, 7, 5, outline);
            FillEllipse(pixels, 39, 9, 5, 3, skinShade);
            FillEllipse(pixels, 55, 9, 5, 3, skinShade);

            FillEllipse(pixels, 48, 42, 13, 18, outline);
            FillEllipse(pixels, 48, 43, 11, 16, skin);
            FillRect(pixels, 44, 54, 52, 61, outline);
            FillRect(pixels, 45, 54, 51, 61, skin);

            FillEllipse(pixels, 35, 42, 5, 16, outline);
            FillEllipse(pixels, 61, 42, 5, 16, outline);
            FillEllipse(pixels, 36, 42, 3, 14, skinShade);
            FillEllipse(pixels, 60, 42, 3, 14, skin);
            FillEllipse(pixels, 34, 25, 6, 5, outline);
            FillEllipse(pixels, 62, 25, 6, 5, outline);
            FillEllipse(pixels, 34, 26, 4, 3, skin);
            FillEllipse(pixels, 62, 26, 4, 3, skinLight);

            FillEllipse(pixels, 34, 68, 4, 6, outline);
            FillEllipse(pixels, 62, 68, 4, 6, outline);
            FillEllipse(pixels, 34, 68, 2, 4, skinShade);
            FillEllipse(pixels, 62, 68, 2, 4, skin);
            FillEllipse(pixels, 48, 70, 15, 16, outline);
            FillEllipse(pixels, 48, 69, 13, 14, skin);
            FillEllipse(pixels, 52, 71, 7, 7, skinLight);
        }

        private static void DrawPlayerUndershirt(Color[] pixels)
        {
            Color outline = new Color(0.2f, 0.12f, 0.09f, 1f);
            Color cloth = new Color(0.92f, 0.84f, 0.66f, 1f);
            Color shadow = new Color(0.62f, 0.43f, 0.28f, 1f);
            Color trim = new Color(0.98f, 0.92f, 0.72f, 1f);

            FillEllipse(pixels, 48, 44, 13, 16, outline);
            FillEllipse(pixels, 48, 44, 11, 14, cloth);
            FillRect(pixels, 38, 34, 58, 48, cloth);
            FillRect(pixels, 39, 47, 57, 50, shadow);
            FillRect(pixels, 42, 53, 54, 56, trim);
            FillRect(pixels, 46, 47, 50, 57, shadow);
            FillRect(pixels, 36, 45, 41, 50, cloth);
            FillRect(pixels, 55, 45, 60, 50, cloth);
        }

        private static void DrawPlayerShorts(Color[] pixels)
        {
            Color outline = new Color(0.16f, 0.09f, 0.07f, 1f);
            Color cloth = new Color(0.42f, 0.29f, 0.22f, 1f);
            Color trim = new Color(0.21f, 0.13f, 0.1f, 1f);
            Color light = new Color(0.58f, 0.4f, 0.28f, 1f);

            FillRect(pixels, 37, 25, 59, 36, outline);
            FillRect(pixels, 39, 27, 57, 35, cloth);
            FillEllipse(pixels, 42, 24, 7, 8, outline);
            FillEllipse(pixels, 54, 24, 7, 8, outline);
            FillEllipse(pixels, 42, 25, 5, 6, cloth);
            FillEllipse(pixels, 54, 25, 5, 6, cloth);
            FillRect(pixels, 39, 34, 57, 36, trim);
            FillRect(pixels, 42, 29, 47, 33, light);
        }

        private static void DrawPlayerHairFace(Color[] pixels)
        {
            Color hairDark = new Color(0.16f, 0.07f, 0.04f, 1f);
            Color hair = new Color(0.42f, 0.2f, 0.08f, 1f);
            Color hairLight = new Color(0.68f, 0.35f, 0.12f, 1f);
            Color eye = new Color(0.08f, 0.09f, 0.11f, 1f);
            Color eyeLight = new Color(0.86f, 0.92f, 1f, 1f);
            Color cheek = new Color(0.86f, 0.42f, 0.34f, 0.75f);

            FillEllipse(pixels, 48, 79, 15, 8, hairDark);
            FillEllipse(pixels, 40, 75, 9, 11, hairDark);
            FillEllipse(pixels, 56, 75, 9, 11, hairDark);
            FillEllipse(pixels, 48, 80, 12, 6, hair);
            FillEllipse(pixels, 39, 73, 7, 9, hair);
            FillEllipse(pixels, 58, 73, 7, 9, hair);
            FillRect(pixels, 37, 81, 44, 87, hair);
            FillRect(pixels, 47, 82, 54, 89, hair);
            FillRect(pixels, 55, 79, 62, 85, hairDark);
            FillRect(pixels, 43, 78, 51, 82, hairLight);

            FillRect(pixels, 40, 68, 45, 72, eye);
            FillRect(pixels, 52, 68, 57, 72, eye);
            FillRect(pixels, 42, 70, 43, 71, eyeLight);
            FillRect(pixels, 54, 70, 55, 71, eyeLight);
            FillRect(pixels, 47, 65, 50, 66, new Color(0.32f, 0.16f, 0.1f, 1f));
            FillEllipse(pixels, 39, 64, 3, 2, cheek);
            FillEllipse(pixels, 57, 64, 3, 2, cheek);
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

        private static void DrawWeaponOverlay(Color[] pixels, WeaponDefinition weapon)
        {
            string id = weapon.ItemId.ToLowerInvariant();
            Color outline = new Color(0.13f, 0.1f, 0.08f, 1f);
            Color grip = new Color(0.28f, 0.17f, 0.1f, 1f);
            Color blade = Color.Lerp(weapon.PlaceholderColor, Color.white, 0.28f);
            Color shade = Color.Lerp(weapon.PlaceholderColor, Color.black, 0.34f);

            if (id.Contains("stone"))
            {
                FillRect(pixels, 58, 31, 70, 35, outline);
                FillRect(pixels, 60, 32, 68, 34, grip);
                FillRotatedWeaponBlade(pixels, 63, 33, 18, 4, blade, shade, true);
            }
            else if (id.Contains("copper"))
            {
                FillRect(pixels, 58, 31, 69, 34, outline);
                FillRect(pixels, 60, 32, 67, 33, grip);
                FillRotatedWeaponBlade(pixels, 63, 33, 21, 3, blade, shade, false);
                FillEllipse(pixels, 68, 47, 3, 3, new Color(1f, 0.62f, 0.26f, 1f));
            }
            else
            {
                FillRect(pixels, 58, 31, 68, 34, outline);
                FillRect(pixels, 60, 32, 66, 33, grip);
                FillRotatedWeaponBlade(pixels, 63, 33, 16, 2, blade, shade, false);
            }
        }

        private static void FillRotatedWeaponBlade(Color[] pixels, int startX, int startY, int length, int thickness, Color fill, Color shade, bool chunky)
        {
            for (int i = 0; i < length; i++)
            {
                int x = startX + i / 4;
                int y = startY + i;
                int halfThickness = Mathf.Max(1, thickness - i / (chunky ? 10 : 13));
                FillRect(pixels, x - halfThickness, y, x + halfThickness, y + 1, fill);
                if (i % 3 == 0)
                {
                    FillRect(pixels, x + halfThickness, y, x + halfThickness, y + 1, shade);
                }
            }

            FillRect(pixels, startX + length / 4 - 1, startY + length, startX + length / 4 + 1, startY + length + 2, fill);
        }

        private static void FillRotatedBlade(Color[] pixels, Color fill, Color shade)
        {
            for (int y = 27; y < 63; y++)
            {
                int x = 62 + (y - 27) / 9;
                FillRect(pixels, x, y, x + 3, y + 2, fill);
            }

            FillRect(pixels, 58, 31, 68, 34, shade);
            FillRect(pixels, 60, 25, 64, 32, shade);
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

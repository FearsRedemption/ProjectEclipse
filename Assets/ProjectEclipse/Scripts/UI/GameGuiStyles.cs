using System.Collections.Generic;
using UnityEngine;

namespace ProjectEclipse.UI
{
    public static class GameGuiStyles
    {
        public static readonly Color PanelColor = new Color(0.08f, 0.1f, 0.12f, 0.94f);
        public static readonly Color SubPanelColor = new Color(0.12f, 0.15f, 0.17f, 0.96f);
        public static readonly Color SlotColor = new Color(0.16f, 0.19f, 0.2f, 1f);
        public static readonly Color SlotBorderColor = new Color(0.48f, 0.55f, 0.55f, 1f);
        public static readonly Color SelectedColor = new Color(1f, 0.78f, 0.32f, 1f);
        public static readonly Color MutedTextColor = new Color(0.7f, 0.76f, 0.72f, 1f);

        private static readonly Dictionary<Color32, Texture2D> Textures = new Dictionary<Color32, Texture2D>();
        private static readonly Dictionary<string, Texture2D> StyleTextures = new Dictionary<string, Texture2D>();
        private static bool initialized;

        public static GUIStyle Window { get; private set; }
        public static GUIStyle InventoryWindow { get; private set; }
        public static GUIStyle InventorySurface { get; private set; }
        public static GUIStyle Panel { get; private set; }
        public static GUIStyle SubPanel { get; private set; }
        public static GUIStyle HeaderLabel { get; private set; }
        public static GUIStyle Label { get; private set; }
        public static GUIStyle SmallLabel { get; private set; }
        public static GUIStyle MutedLabel { get; private set; }
        public static GUIStyle CenterLabel { get; private set; }
        public static GUIStyle Button { get; private set; }
        public static GUIStyle SelectedButton { get; private set; }
        public static GUIStyle StackLabel { get; private set; }
        public static GUIStyle BadgeLabel { get; private set; }
        public static GUIStyle FeedbackLabel { get; private set; }

        public static void ApplySkin(GUISkin skin)
        {
            EnsureInitialized(skin);
            if (skin == null)
            {
                return;
            }

            skin.window = Window;
            skin.box = SubPanel;
            skin.label = Label;
            skin.button = Button;
        }

        public static Texture2D GetTexture(Color color)
        {
            Color32 key = color;
            Texture2D texture;
            if (Textures.TryGetValue(key, out texture))
            {
                return texture;
            }

            texture = new Texture2D(1, 1);
            texture.name = "Runtime UI Texture";
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.filterMode = FilterMode.Point;
            texture.SetPixel(0, 0, color);
            texture.Apply();
            Textures[key] = texture;
            return texture;
        }

        public static void DrawBox(Rect rect, Color fill, Color border, float borderThickness)
        {
            GUI.DrawTexture(rect, GetTexture(border));
            Rect inner = new Rect(
                rect.x + borderThickness,
                rect.y + borderThickness,
                Mathf.Max(0f, rect.width - borderThickness * 2f),
                Mathf.Max(0f, rect.height - borderThickness * 2f));
            GUI.DrawTexture(inner, GetTexture(fill));
        }

        public static void DrawInsetPanel(Rect rect)
        {
            DrawBox(rect, new Color(0.08f, 0.105f, 0.105f, 0.96f), new Color(0.55f, 0.47f, 0.28f, 0.96f), 1f);
            Rect inner = new Rect(rect.x + 3f, rect.y + 3f, rect.width - 6f, rect.height - 6f);
            DrawBox(inner, new Color(0.12f, 0.15f, 0.15f, 0.96f), new Color(0.22f, 0.28f, 0.27f, 1f), 1f);
            GUI.DrawTexture(new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, 1f), GetTexture(new Color(1f, 0.86f, 0.45f, 0.16f)));
        }

        public static void DrawSlot(Rect rect, bool selected)
        {
            Color border = selected ? SelectedColor : new Color(0.5f, 0.43f, 0.28f, 1f);
            DrawBox(rect, new Color(0.07f, 0.08f, 0.08f, 1f), border, selected ? 2f : 1f);
            Rect bevel = new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f);
            DrawBox(bevel, SlotColor, new Color(0.27f, 0.32f, 0.31f, 1f), 1f);
            Rect shine = new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, 2f);
            GUI.DrawTexture(shine, GetTexture(new Color(1f, 0.92f, 0.58f, selected ? 0.28f : 0.12f)));
            Rect shadow = new Rect(rect.x + 4f, rect.yMax - 6f, rect.width - 8f, 2f);
            GUI.DrawTexture(shadow, GetTexture(new Color(0f, 0f, 0f, 0.18f)));
        }

        public static void DrawProgressBar(Rect rect, float normalized, Color fill)
        {
            DrawBox(rect, new Color(0.05f, 0.06f, 0.06f, 1f), new Color(0.4f, 0.45f, 0.43f, 1f), 1f);
            Rect fillRect = new Rect(rect.x + 2f, rect.y + 2f, Mathf.Max(0f, rect.width - 4f) * Mathf.Clamp01(normalized), Mathf.Max(0f, rect.height - 4f));
            GUI.DrawTexture(fillRect, GetTexture(fill));
        }

        public static void DrawInventoryBackdrop(Rect modalRect)
        {
            EnsureInitialized(GUI.skin);
            Rect screen = new Rect(0f, 0f, Screen.width, Screen.height);
            GUI.DrawTexture(screen, GetTexture(new Color(0.005f, 0.01f, 0.012f, 0.78f)));

            Rect halo = new Rect(modalRect.x - 18f, modalRect.y - 18f, modalRect.width + 36f, modalRect.height + 36f);
            DrawBox(halo, new Color(0.1f, 0.13f, 0.12f, 0.34f), new Color(0.72f, 0.55f, 0.28f, 0.4f), 1f);
            Rect shell = new Rect(modalRect.x - 6f, modalRect.y - 6f, modalRect.width + 12f, modalRect.height + 12f);
            DrawBox(shell, new Color(0.04f, 0.055f, 0.055f, 0.94f), new Color(0.68f, 0.52f, 0.28f, 0.95f), 2f);

            Rect topBand = new Rect(modalRect.x, modalRect.y, modalRect.width, 36f);
            GUI.DrawTexture(topBand, GetTexture(new Color(0.25f, 0.18f, 0.08f, 0.5f)));
            Rect bottomBand = new Rect(modalRect.x, modalRect.yMax - 18f, modalRect.width, 18f);
            GUI.DrawTexture(bottomBand, GetTexture(new Color(0.02f, 0.13f, 0.12f, 0.38f)));
        }

        private static void EnsureInitialized(GUISkin skin)
        {
            if (initialized)
            {
                return;
            }

            GUIStyle baseWindow = skin != null ? skin.window : GUI.skin.window;
            GUIStyle baseLabel = skin != null ? skin.label : GUI.skin.label;
            GUIStyle baseButton = skin != null ? skin.button : GUI.skin.button;

            Window = new GUIStyle(baseWindow);
            Window.normal.background = GetStyleTexture("window", PanelColor, new Color(0.64f, 0.52f, 0.31f, 1f), new Color(0.15f, 0.19f, 0.18f, 1f), new Color(0.95f, 0.78f, 0.38f, 0.22f));
            Window.normal.textColor = Color.white;
            Window.padding = new RectOffset(12, 12, 24, 12);
            Window.border = new RectOffset(6, 6, 6, 6);
            Window.fontSize = 14;

            InventoryWindow = new GUIStyle(Window);
            InventoryWindow.normal.background = GetStyleTexture("inventory-window", new Color(0.055f, 0.068f, 0.064f, 0.99f), new Color(0.78f, 0.58f, 0.28f, 1f), new Color(0.14f, 0.18f, 0.17f, 1f), new Color(1f, 0.78f, 0.35f, 0.3f));
            InventoryWindow.padding = new RectOffset(14, 14, 28, 14);
            InventoryWindow.border = new RectOffset(8, 8, 8, 8);
            InventoryWindow.fontSize = 15;

            InventorySurface = new GUIStyle(baseWindow);
            InventorySurface.normal.background = GetStyleTexture("inventory-surface", new Color(0.08f, 0.095f, 0.09f, 0.98f), new Color(0.36f, 0.42f, 0.34f, 1f), new Color(0.12f, 0.15f, 0.14f, 1f), new Color(0.75f, 0.58f, 0.28f, 0.18f));
            InventorySurface.normal.textColor = Color.white;
            InventorySurface.padding = new RectOffset(12, 12, 12, 12);
            InventorySurface.margin = new RectOffset(0, 0, 0, 0);
            InventorySurface.border = new RectOffset(6, 6, 6, 6);

            Panel = new GUIStyle(baseWindow);
            Panel.normal.background = GetStyleTexture("panel", PanelColor, new Color(0.5f, 0.43f, 0.27f, 1f), new Color(0.13f, 0.16f, 0.16f, 1f), new Color(0.9f, 0.72f, 0.36f, 0.16f));
            Panel.normal.textColor = Color.white;
            Panel.padding = new RectOffset(10, 10, 10, 10);
            Panel.margin = new RectOffset(0, 0, 0, 0);
            Panel.border = new RectOffset(6, 6, 6, 6);

            SubPanel = new GUIStyle(baseWindow);
            SubPanel.normal.background = GetStyleTexture("sub-panel", SubPanelColor, new Color(0.42f, 0.38f, 0.25f, 1f), new Color(0.16f, 0.19f, 0.18f, 1f), new Color(0.86f, 0.72f, 0.4f, 0.12f));
            SubPanel.normal.textColor = Color.white;
            SubPanel.padding = new RectOffset(8, 8, 8, 8);
            SubPanel.margin = new RectOffset(0, 0, 0, 0);
            SubPanel.border = new RectOffset(5, 5, 5, 5);

            Label = new GUIStyle(baseLabel);
            Label.normal.textColor = new Color(0.94f, 0.96f, 0.92f, 1f);
            Label.fontSize = 13;
            Label.wordWrap = false;

            HeaderLabel = new GUIStyle(Label);
            HeaderLabel.fontSize = 15;
            HeaderLabel.fontStyle = FontStyle.Bold;
            HeaderLabel.normal.textColor = new Color(1f, 0.92f, 0.68f, 1f);

            SmallLabel = new GUIStyle(Label);
            SmallLabel.fontSize = 11;

            MutedLabel = new GUIStyle(SmallLabel);
            MutedLabel.normal.textColor = MutedTextColor;
            MutedLabel.wordWrap = true;

            CenterLabel = new GUIStyle(Label);
            CenterLabel.alignment = TextAnchor.MiddleCenter;
            CenterLabel.clipping = TextClipping.Clip;

            Button = new GUIStyle(baseButton);
            Button.normal.background = GetStyleTexture("button", new Color(0.18f, 0.23f, 0.22f, 1f), new Color(0.48f, 0.41f, 0.25f, 1f), new Color(0.25f, 0.31f, 0.3f, 1f), new Color(1f, 0.84f, 0.42f, 0.18f));
            Button.hover.background = GetStyleTexture("button-hover", new Color(0.24f, 0.3f, 0.29f, 1f), new Color(0.7f, 0.56f, 0.3f, 1f), new Color(0.31f, 0.38f, 0.36f, 1f), new Color(1f, 0.9f, 0.5f, 0.26f));
            Button.active.background = GetStyleTexture("button-active", new Color(0.11f, 0.15f, 0.15f, 1f), new Color(0.38f, 0.32f, 0.22f, 1f), new Color(0.18f, 0.22f, 0.21f, 1f), new Color(1f, 0.78f, 0.34f, 0.1f));
            Button.normal.textColor = Color.white;
            Button.hover.textColor = Color.white;
            Button.active.textColor = Color.white;
            Button.fontSize = 12;
            Button.padding = new RectOffset(8, 8, 4, 4);
            Button.border = new RectOffset(5, 5, 5, 5);

            SelectedButton = new GUIStyle(Button);
            SelectedButton.normal.background = GetStyleTexture("button-selected", new Color(0.48f, 0.33f, 0.12f, 1f), new Color(0.95f, 0.74f, 0.32f, 1f), new Color(0.62f, 0.43f, 0.18f, 1f), new Color(1f, 0.92f, 0.54f, 0.32f));
            SelectedButton.normal.textColor = new Color(1f, 0.97f, 0.82f, 1f);

            StackLabel = new GUIStyle(SmallLabel);
            StackLabel.alignment = TextAnchor.LowerRight;
            StackLabel.normal.textColor = Color.white;
            StackLabel.fontStyle = FontStyle.Bold;

            BadgeLabel = new GUIStyle(SmallLabel);
            BadgeLabel.alignment = TextAnchor.UpperRight;
            BadgeLabel.normal.textColor = new Color(1f, 0.91f, 0.58f, 1f);
            BadgeLabel.fontStyle = FontStyle.Bold;

            FeedbackLabel = new GUIStyle(Label);
            FeedbackLabel.alignment = TextAnchor.MiddleCenter;
            FeedbackLabel.normal.background = GetTexture(new Color(0.05f, 0.06f, 0.06f, 0.82f));
            FeedbackLabel.normal.textColor = new Color(1f, 0.92f, 0.62f, 1f);
            FeedbackLabel.fontSize = 16;
            FeedbackLabel.fontStyle = FontStyle.Bold;
            FeedbackLabel.padding = new RectOffset(8, 8, 6, 6);

            initialized = true;
        }

        private static Texture2D GetStyleTexture(string key, Color fill, Color border, Color bevel, Color highlight)
        {
            Texture2D texture;
            if (StyleTextures.TryGetValue(key, out texture))
            {
                return texture;
            }

            const int size = 32;
            texture = new Texture2D(size, size);
            texture.name = "Runtime UI Style " + key;
            texture.hideFlags = HideFlags.HideAndDontSave;
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                float vertical = (float)y / (size - 1);
                for (int x = 0; x < size; x++)
                {
                    int edge = Mathf.Min(Mathf.Min(x, y), Mathf.Min(size - 1 - x, size - 1 - y));
                    Color pixel = Color.Lerp(fill, Color.black, (1f - vertical) * 0.08f);
                    if (edge <= 1)
                    {
                        pixel = border;
                    }
                    else if (edge <= 3)
                    {
                        pixel = bevel;
                    }
                    else if (y >= size - 5)
                    {
                        pixel = Color.Lerp(pixel, highlight, 0.35f);
                    }
                    else if (((x + y) % 11) == 0)
                    {
                        pixel = Color.Lerp(pixel, Color.white, 0.025f);
                    }

                    pixels[y * size + x] = pixel;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            StyleTextures[key] = texture;
            return texture;
        }
    }
}

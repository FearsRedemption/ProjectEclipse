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
        private static bool initialized;

        public static GUIStyle Window { get; private set; }
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

        public static void DrawSlot(Rect rect, bool selected)
        {
            Color border = selected ? SelectedColor : SlotBorderColor;
            DrawBox(rect, SlotColor, border, selected ? 2f : 1f);
            Rect shine = new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, 1f);
            GUI.DrawTexture(shine, GetTexture(new Color(1f, 1f, 1f, selected ? 0.22f : 0.1f)));
        }

        public static void DrawProgressBar(Rect rect, float normalized, Color fill)
        {
            DrawBox(rect, new Color(0.05f, 0.06f, 0.06f, 1f), new Color(0.4f, 0.45f, 0.43f, 1f), 1f);
            Rect fillRect = new Rect(rect.x + 2f, rect.y + 2f, Mathf.Max(0f, rect.width - 4f) * Mathf.Clamp01(normalized), Mathf.Max(0f, rect.height - 4f));
            GUI.DrawTexture(fillRect, GetTexture(fill));
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
            Window.normal.background = GetTexture(PanelColor);
            Window.normal.textColor = Color.white;
            Window.padding = new RectOffset(12, 12, 24, 12);
            Window.border = new RectOffset(1, 1, 1, 1);
            Window.fontSize = 14;

            Panel = new GUIStyle(baseWindow);
            Panel.normal.background = GetTexture(PanelColor);
            Panel.normal.textColor = Color.white;
            Panel.padding = new RectOffset(10, 10, 10, 10);
            Panel.margin = new RectOffset(0, 0, 0, 0);

            SubPanel = new GUIStyle(baseWindow);
            SubPanel.normal.background = GetTexture(SubPanelColor);
            SubPanel.normal.textColor = Color.white;
            SubPanel.padding = new RectOffset(8, 8, 8, 8);
            SubPanel.margin = new RectOffset(0, 0, 0, 0);

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
            Button.normal.background = GetTexture(new Color(0.2f, 0.25f, 0.26f, 1f));
            Button.hover.background = GetTexture(new Color(0.28f, 0.34f, 0.34f, 1f));
            Button.active.background = GetTexture(new Color(0.12f, 0.16f, 0.16f, 1f));
            Button.normal.textColor = Color.white;
            Button.hover.textColor = Color.white;
            Button.active.textColor = Color.white;
            Button.fontSize = 12;
            Button.padding = new RectOffset(8, 8, 4, 4);

            SelectedButton = new GUIStyle(Button);
            SelectedButton.normal.background = GetTexture(new Color(0.55f, 0.39f, 0.14f, 1f));
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
    }
}

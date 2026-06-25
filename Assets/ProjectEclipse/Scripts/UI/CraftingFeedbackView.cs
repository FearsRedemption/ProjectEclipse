using System.Collections.Generic;
using ProjectEclipse.Crafting;
using UnityEngine;

namespace ProjectEclipse.UI
{
    public static class CraftingFeedbackView
    {
        public static void Draw(CraftingFeedbackMessage feedback)
        {
            GameGuiStyles.ApplySkin(GUI.skin);
            if (feedback == null)
            {
                return;
            }

            Color oldColor = GUI.color;
            GUI.color = feedback.IsSuccess ? new Color(0.65f, 1f, 0.65f, 1f) : feedback.IsError ? new Color(1f, 0.65f, 0.65f, 1f) : new Color(1f, 0.92f, 0.45f, 1f);
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label(feedback.Header);
            GUI.color = oldColor;
            if (!string.IsNullOrEmpty(feedback.Detail))
            {
                GUILayout.Label(feedback.Detail);
            }
            DrawLines(feedback.Lines);
            GUILayout.EndVertical();
            GUI.color = oldColor;
        }

        public static void DrawCompact(CraftingFeedbackMessage feedback)
        {
            GameGuiStyles.ApplySkin(GUI.skin);
            if (feedback == null)
            {
                return;
            }

            Color oldColor = GUI.color;
            GUI.color = feedback.IsSuccess ? new Color(0.65f, 1f, 0.65f, 1f) : feedback.IsError ? new Color(1f, 0.65f, 0.65f, 1f) : new Color(1f, 0.92f, 0.45f, 1f);
            GUILayout.BeginVertical(GameGuiStyles.SubPanel);
            GUILayout.Label(feedback.Header, GameGuiStyles.SmallLabel);
            GUI.color = oldColor;
            if (!string.IsNullOrEmpty(feedback.Detail))
            {
                GUILayout.Label(feedback.Detail, GameGuiStyles.MutedLabel);
            }
            GUILayout.EndVertical();
            GUI.color = oldColor;
        }

        public static void DrawLines(IEnumerable<CraftingRequirementLine> lines)
        {
            if (lines == null)
            {
                return;
            }

            foreach (CraftingRequirementLine line in lines)
            {
                DrawLine(line);
            }
        }

        public static void DrawLine(CraftingRequirementLine line)
        {
            if (line == null)
            {
                return;
            }

            Color oldColor = GUI.color;
            GUI.color = ColorFor(line.Status);
            GUILayout.BeginHorizontal();
            Texture icon = ItemSlotView.GetIconTexture(line.Item);
            Rect iconRect = GUILayoutUtility.GetRect(20f, 20f, GUILayout.Width(20f), GUILayout.Height(20f));
            if (icon != null)
            {
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
            }
            else
            {
                GameGuiStyles.DrawBox(iconRect, new Color(0.13f, 0.16f, 0.16f, 1f), new Color(0.38f, 0.45f, 0.44f, 1f), 1f);
            }

            string count = line.RequiredQuantity > 0 ? " " + line.OwnedQuantity + "/" + line.RequiredQuantity : string.Empty;
            string reserved = line.ReservedQuantity > 0 ? " reserved " + line.ReservedQuantity : string.Empty;
            string detail = string.IsNullOrEmpty(line.Detail) ? string.Empty : " - " + line.Detail;
            GUILayout.Label(StatusPrefix(line.Status) + " " + line.Label + count + reserved + detail, GameGuiStyles.MutedLabel);
            GUILayout.EndHorizontal();
            GUI.color = oldColor;
        }

        private static Color ColorFor(CraftingRequirementStatus status)
        {
            switch (status)
            {
                case CraftingRequirementStatus.Satisfied:
                case CraftingRequirementStatus.Complete:
                    return new Color(0.45f, 1f, 0.45f, 1f);
                case CraftingRequirementStatus.Queueable:
                    return new Color(1f, 0.88f, 0.35f, 1f);
                case CraftingRequirementStatus.Processing:
                case CraftingRequirementStatus.Reserved:
                    return new Color(0.55f, 0.82f, 1f, 1f);
                case CraftingRequirementStatus.Missing:
                case CraftingRequirementStatus.MissingPort:
                case CraftingRequirementStatus.InsufficientPortTier:
                case CraftingRequirementStatus.RecipeLocked:
                    return new Color(1f, 0.45f, 0.45f, 1f);
                default:
                    return Color.white;
            }
        }

        private static string StatusPrefix(CraftingRequirementStatus status)
        {
            switch (status)
            {
                case CraftingRequirementStatus.Satisfied:
                case CraftingRequirementStatus.Complete:
                    return "[OK]";
                case CraftingRequirementStatus.Queueable:
                    return "[QUEUE]";
                case CraftingRequirementStatus.Processing:
                    return "[PROC]";
                case CraftingRequirementStatus.Reserved:
                    return "[RES]";
                case CraftingRequirementStatus.MissingPort:
                    return "[PORT]";
                case CraftingRequirementStatus.InsufficientPortTier:
                    return "[TIER]";
                case CraftingRequirementStatus.RecipeLocked:
                    return "[LOCK]";
                case CraftingRequirementStatus.Missing:
                    return "[MISS]";
                default:
                    return "[ ]";
            }
        }
    }
}

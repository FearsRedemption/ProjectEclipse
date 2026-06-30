using System.Collections.Generic;
using ProjectEclipse.Crafting;
using UnityEngine;

namespace ProjectEclipse.UI
{
    public static class WorkOrderTrackerPanel
    {
        private const int VisibleOrderCount = 3;
        private const float CompletionHoldSeconds = 1.4f;
        private const float CompletionFadeSeconds = 1.1f;
        private static int firstVisibleOrderIndex;

        public static void Draw(CraftingSystem crafting)
        {
            Draw(crafting, false, false);
        }

        public static void Draw(CraftingSystem crafting, bool incompleteOnly, bool showCompletion)
        {
            if (crafting == null || crafting.WorkOrders.Count == 0)
            {
                GUILayout.Label("No Work Order", GameGuiStyles.MutedLabel);
                return;
            }

            int visibleCount = Mathf.Min(VisibleOrderCount, crafting.WorkOrders.Count);
            int maxFirstIndex = Mathf.Max(0, crafting.WorkOrders.Count - visibleCount);
            firstVisibleOrderIndex = Mathf.Clamp(firstVisibleOrderIndex, 0, maxFirstIndex);
            bool canScroll = crafting.WorkOrders.Count > visibleCount;

            if (canScroll)
            {
                GUI.enabled = firstVisibleOrderIndex > 0;
                if (GUILayout.Button("^", GameGuiStyles.Button, GUILayout.Height(20f)))
                {
                    firstVisibleOrderIndex = Mathf.Max(0, firstVisibleOrderIndex - 1);
                }
                GUI.enabled = true;
            }

            int endIndex = Mathf.Min(crafting.WorkOrders.Count, firstVisibleOrderIndex + visibleCount);
            for (int i = firstVisibleOrderIndex; i < endIndex; i++)
            {
                WorkOrder order = crafting.WorkOrders[i];
                if (order == null || order.Plan == null)
                {
                    continue;
                }

                if (DrawOrder(crafting, order, i, incompleteOnly, showCompletion))
                {
                    return;
                }
            }

            if (canScroll)
            {
                GUI.enabled = firstVisibleOrderIndex < maxFirstIndex;
                if (GUILayout.Button("v", GameGuiStyles.Button, GUILayout.Height(20f)))
                {
                    firstVisibleOrderIndex = Mathf.Min(maxFirstIndex, firstVisibleOrderIndex + 1);
                }
                GUI.enabled = true;
            }
        }

        public static void DismissExpiredCompletedOrders(CraftingSystem crafting)
        {
            if (crafting == null || crafting.WorkOrders.Count == 0)
            {
                return;
            }

            for (int i = crafting.WorkOrders.Count - 1; i >= 0; i--)
            {
                WorkOrder order = crafting.WorkOrders[i];
                if (ShouldAutoDismiss(order))
                {
                    crafting.ClearWorkOrder(order);
                }
            }

            firstVisibleOrderIndex = Mathf.Clamp(firstVisibleOrderIndex, 0, Mathf.Max(0, crafting.WorkOrders.Count - VisibleOrderCount));
        }

        private static bool DrawOrder(CraftingSystem crafting, WorkOrder order, int orderIndex, bool incompleteOnly, bool showCompletion)
        {
            if (ShouldAutoDismiss(order))
            {
                return true;
            }

            string outputName = order.Plan.FinalRecipe != null && order.Plan.FinalRecipe.OutputItem != null
                ? order.Plan.FinalRecipe.OutputItem.DisplayName
                : "Item";

            Color previousColor = GUI.color;
            if (order.IsComplete)
            {
                float alpha = GetCompletionAlpha(order);
                GUI.color = new Color(previousColor.r, previousColor.g, previousColor.b, previousColor.a * alpha);
            }

            GUILayout.BeginVertical(GameGuiStyles.SubPanel);
            GUILayout.Label(order.IsComplete ? "WO" + (orderIndex + 1) + ": " + outputName + " Complete" : "WO" + (orderIndex + 1) + ": " + outputName + " x" + order.Plan.TargetQuantity, GameGuiStyles.HeaderLabel);
            if (order.IsComplete && showCompletion)
            {
                GUILayout.Label("Crafting complete", GameGuiStyles.MutedLabel);
                if (GUILayout.Button("Dismiss", GameGuiStyles.Button, GUILayout.Height(24f)))
                {
                    crafting.ClearWorkOrder(order);
                    GUILayout.EndVertical();
                    GUI.color = previousColor;
                    return true;
                }

                GUILayout.EndVertical();
                GUI.color = previousColor;
                return false;
            }

            bool drewLine = false;
            List<CraftingRequirementLine> lines = crafting.GetWorkOrderLines(order);
            for (int i = 0; i < lines.Count; i++)
            {
                CraftingRequirementLine line = lines[i];
                if (incompleteOnly && !ShouldShowIncomplete(line))
                {
                    continue;
                }

                CraftingFeedbackView.DrawLine(line);
                drewLine = true;
            }

            if (!drewLine)
            {
                GUILayout.Label("All tracked requirements are ready.", GameGuiStyles.MutedLabel);
            }

            GUILayout.Space(4f);
            if (order.IsComplete)
            {
                if (GUILayout.Button("Dismiss", GameGuiStyles.Button, GUILayout.Height(24f)))
                {
                    crafting.ClearWorkOrder(order);
                    GUILayout.EndVertical();
                    GUI.color = previousColor;
                    return true;
                }
            }
            else if (GUILayout.Button("Cancel Work Order", GameGuiStyles.Button, GUILayout.Height(24f)))
            {
                crafting.CancelWorkOrder(order);
                GUILayout.EndVertical();
                GUI.color = previousColor;
                return true;
            }

            GUILayout.EndVertical();
            GUI.color = previousColor;
            return false;
        }

        private static bool ShouldAutoDismiss(WorkOrder order)
        {
            if (order == null || !order.IsComplete || order.CompletedAt < 0f)
            {
                return false;
            }

            return Time.time - order.CompletedAt >= CompletionHoldSeconds + CompletionFadeSeconds;
        }

        private static float GetCompletionAlpha(WorkOrder order)
        {
            if (order == null || !order.IsComplete || order.CompletedAt < 0f)
            {
                return 1f;
            }

            float elapsed = Time.time - order.CompletedAt;
            if (elapsed <= CompletionHoldSeconds)
            {
                return 1f;
            }

            return Mathf.Clamp01(1f - ((elapsed - CompletionHoldSeconds) / CompletionFadeSeconds));
        }

        private static bool ShouldShowIncomplete(CraftingRequirementLine line)
        {
            if (line == null)
            {
                return false;
            }

            if (line.Status == CraftingRequirementStatus.Satisfied || line.Status == CraftingRequirementStatus.Complete)
            {
                return false;
            }

            if (line.Status == CraftingRequirementStatus.Processing
                || line.Status == CraftingRequirementStatus.Queueable
                || line.Status == CraftingRequirementStatus.Missing
                || line.Status == CraftingRequirementStatus.MissingPort
                || line.Status == CraftingRequirementStatus.InsufficientPortTier
                || line.Status == CraftingRequirementStatus.RecipeLocked)
            {
                return true;
            }

            return line.RequiredQuantity > 0 && line.OwnedQuantity < line.RequiredQuantity;
        }
    }
}

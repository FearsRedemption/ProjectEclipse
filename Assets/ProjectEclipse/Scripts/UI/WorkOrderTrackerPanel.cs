using ProjectEclipse.Crafting;
using UnityEngine;

namespace ProjectEclipse.UI
{
    public static class WorkOrderTrackerPanel
    {
        public static void Draw(CraftingSystem crafting)
        {
            Draw(crafting, false, false);
        }

        public static void Draw(CraftingSystem crafting, bool incompleteOnly, bool showCompletion)
        {
            if (crafting == null || crafting.ActiveWorkOrder == null || crafting.ActiveWorkOrder.Plan == null)
            {
                GUILayout.Label("No Work Order", GameGuiStyles.MutedLabel);
                return;
            }

            WorkOrder order = crafting.ActiveWorkOrder;
            string outputName = order.Plan.FinalRecipe != null && order.Plan.FinalRecipe.OutputItem != null
                ? order.Plan.FinalRecipe.OutputItem.DisplayName
                : "Item";

            GUILayout.Label(order.IsComplete ? outputName + " Complete" : "Work Order: " + outputName + " x" + order.Plan.TargetQuantity, GameGuiStyles.HeaderLabel);
            if (order.IsComplete && showCompletion)
            {
                GUILayout.Label("Crafting complete", GameGuiStyles.MutedLabel);
                return;
            }

            bool drewLine = false;
            foreach (CraftingRequirementLine line in crafting.GetActiveWorkOrderLines())
            {
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
                if (GUILayout.Button("Dismiss"))
                {
                    crafting.ClearActiveWorkOrder();
                }
            }
            else if (GUILayout.Button("Cancel Work Order"))
            {
                crafting.CancelActiveWorkOrder();
            }
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

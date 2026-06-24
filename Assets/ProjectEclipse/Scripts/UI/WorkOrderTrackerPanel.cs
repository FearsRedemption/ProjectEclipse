using ProjectEclipse.Crafting;
using UnityEngine;

namespace ProjectEclipse.UI
{
    public static class WorkOrderTrackerPanel
    {
        public static void Draw(CraftingSystem crafting)
        {
            if (crafting == null || crafting.ActiveWorkOrder == null || crafting.ActiveWorkOrder.Plan == null)
            {
                GUILayout.Label("No Work Order");
                return;
            }

            WorkOrder order = crafting.ActiveWorkOrder;
            string outputName = order.Plan.FinalRecipe != null && order.Plan.FinalRecipe.OutputItem != null
                ? order.Plan.FinalRecipe.OutputItem.DisplayName
                : "Item";

            GUILayout.Label(order.IsComplete ? "Work Order Complete" : "Work Order: " + outputName + " x" + order.Plan.TargetQuantity);
            CraftingFeedbackView.DrawLines(crafting.GetActiveWorkOrderLines());

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
    }
}

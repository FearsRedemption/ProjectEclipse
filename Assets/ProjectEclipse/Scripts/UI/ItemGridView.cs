using System.Collections.Generic;
using ProjectEclipse.Inventory;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.UI
{
    public class ItemGridView
    {
        private Vector2 scroll;

        public ItemDefinition Draw(
            IReadOnlyList<InventoryStack> stacks,
            ItemHoverState hover,
            System.Predicate<ItemDefinition> filter,
            float height)
        {
            ItemDefinition clicked;
            ItemSlotClick click = DrawClickable(stacks, hover, filter, height, null, out clicked);
            return click != ItemSlotClick.None ? clicked : null;
        }

        public ItemSlotClick DrawClickable(
            IReadOnlyList<InventoryStack> stacks,
            ItemHoverState hover,
            System.Predicate<ItemDefinition> filter,
            float height,
            ItemDefinition selectedItem,
            out ItemDefinition clicked)
        {
            clicked = null;
            ItemSlotClick clickedButton = ItemSlotClick.None;
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(height));
            int column = 0;
            GUILayout.BeginHorizontal();

            for (int i = 0; i < stacks.Count; i++)
            {
                InventoryStack stack = stacks[i];
                if (stack.Item == null || !filter(stack.Item))
                {
                    continue;
                }

                ItemSlotClick slotClick = ItemSlotView.DrawClick(stack.Item, stack.Quantity, hover, stack.Item == selectedItem);
                if (slotClick != ItemSlotClick.None)
                {
                    clicked = stack.Item;
                    clickedButton = slotClick;
                }

                column++;
                if (column >= 8)
                {
                    column = 0;
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
            return clickedButton;
        }
    }
}

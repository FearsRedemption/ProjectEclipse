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
            ItemDefinition clicked = null;
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

                if (ItemSlotView.Draw(stack.Item, stack.Quantity, hover))
                {
                    clicked = stack.Item;
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
            return clicked;
        }
    }
}

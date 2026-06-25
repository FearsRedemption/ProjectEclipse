using System.Collections.Generic;
using ProjectEclipse.Inventory;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.UI
{
    public class ItemGridView
    {
        public const int Columns = 9;
        private const float SlotGap = 4f;
        private const float ScrollbarWidth = 16f;
        private const int MinimumVisibleRows = 6;

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
            List<InventoryStack> visibleStacks = BuildVisibleStacks(stacks, filter);
            int rows = Mathf.Max(MinimumVisibleRows, Mathf.CeilToInt(visibleStacks.Count / (float)Columns));
            float gridWidth = Columns * ItemSlotView.SlotSize + (Columns - 1) * SlotGap;
            float contentHeight = rows * ItemSlotView.SlotSize + Mathf.Max(0, rows - 1) * SlotGap;
            float outerWidth = gridWidth + ScrollbarWidth + 14f;

            Rect outerRect = GUILayoutUtility.GetRect(outerWidth, height, GUILayout.Width(outerWidth), GUILayout.Height(height));
            GameGuiStyles.DrawBox(outerRect, GameGuiStyles.SubPanelColor, new Color(0.35f, 0.43f, 0.42f, 1f), 1f);

            Rect viewport = new Rect(outerRect.x + 6f, outerRect.y + 6f, gridWidth + ScrollbarWidth, Mathf.Max(10f, height - 12f));
            Rect content = new Rect(0f, 0f, gridWidth, contentHeight);
            scroll = GUI.BeginScrollView(viewport, scroll, content, false, true);
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    int index = row * Columns + column;
                    Rect slotRect = new Rect(
                        column * (ItemSlotView.SlotSize + SlotGap),
                        row * (ItemSlotView.SlotSize + SlotGap),
                        ItemSlotView.SlotSize,
                        ItemSlotView.SlotSize);

                    if (index >= visibleStacks.Count)
                    {
                        ItemSlotView.DrawEmpty(slotRect);
                        continue;
                    }

                    InventoryStack stack = visibleStacks[index];
                    ItemSlotClick slotClick = ItemSlotView.DrawClick(stack.Item, stack.Quantity, hover, stack.Item == selectedItem);
                    if (slotClick != ItemSlotClick.None)
                    {
                        clicked = stack.Item;
                        clickedButton = slotClick;
                    }
                }
            }

            GUI.EndScrollView();
            return clickedButton;
        }

        private static List<InventoryStack> BuildVisibleStacks(IReadOnlyList<InventoryStack> stacks, System.Predicate<ItemDefinition> filter)
        {
            List<InventoryStack> visibleStacks = new List<InventoryStack>();
            if (stacks == null)
            {
                return visibleStacks;
            }

            for (int i = 0; i < stacks.Count; i++)
            {
                InventoryStack stack = stacks[i];
                if (stack == null || stack.Item == null)
                {
                    continue;
                }

                if (filter != null && !filter(stack.Item))
                {
                    continue;
                }

                visibleStacks.Add(stack);
            }

            return visibleStacks;
        }
    }
}

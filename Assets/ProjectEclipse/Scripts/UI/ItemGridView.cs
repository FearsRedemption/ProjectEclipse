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
        private const float DragThreshold = 7f;

        private Vector2 scroll;
        private ItemDefinition dragItem;
        private int dragQuantity;
        private Vector2 dragStart;
        private bool draggingItem;

        public int LastVisibleCount { get; private set; }
        public int LastTotalCount { get; private set; }
        public bool IsDraggingItem { get { return draggingItem && dragItem != null; } }
        public ItemDefinition DraggingItem { get { return IsDraggingItem ? dragItem : null; } }

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
            LastVisibleCount = visibleStacks.Count;
            LastTotalCount = CountNonEmptyStacks(stacks);
            int rows = Mathf.Max(MinimumVisibleRows, Mathf.CeilToInt(visibleStacks.Count / (float)Columns));
            float gridWidth = Columns * ItemSlotView.SlotSize + (Columns - 1) * SlotGap;
            float contentHeight = rows * ItemSlotView.SlotSize + Mathf.Max(0, rows - 1) * SlotGap;
            float outerWidth = gridWidth + ScrollbarWidth + 14f;

            Rect outerRect = GUILayoutUtility.GetRect(outerWidth, height, GUILayout.Width(outerWidth), GUILayout.Height(height));
            GameGuiStyles.DrawInsetPanel(outerRect);

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
                    TrackDragCandidate(slotRect, stack);
                    ItemSlotClick slotClick = ItemSlotView.DrawClick(slotRect, stack.Item, stack.Quantity, hover, false);
                    if (slotClick != ItemSlotClick.None)
                    {
                        clicked = stack.Item;
                        clickedButton = slotClick;
                    }
                }
            }

            GUI.EndScrollView();
            UpdateDragState();
            if (visibleStacks.Count == 0)
            {
                Rect messageRect = new Rect(outerRect.x + 12f, outerRect.y + 18f, outerRect.width - 24f, 24f);
                GUI.Label(messageRect, LastTotalCount > 0 ? "No items in this tab" : "Inventory is empty", GameGuiStyles.CenterLabel);
            }

            return clickedButton;
        }

        public bool TryTakeDraggedItem(out ItemDefinition item, out int quantity)
        {
            item = null;
            quantity = 0;
            if (dragItem == null)
            {
                return false;
            }

            item = dragItem;
            quantity = Mathf.Max(1, dragQuantity);
            ClearDrag();
            return true;
        }

        public void CancelDrag()
        {
            ClearDrag();
        }

        private void TrackDragCandidate(Rect rect, InventoryStack stack)
        {
            Event current = Event.current;
            if (current == null || current.type != EventType.MouseDown || current.button != 0 || stack == null || stack.Item == null || !rect.Contains(current.mousePosition))
            {
                return;
            }

            dragItem = stack.Item;
            dragQuantity = stack.Quantity;
            dragStart = current.mousePosition;
            draggingItem = false;
        }

        private void UpdateDragState()
        {
            Event current = Event.current;
            if (current == null || dragItem == null)
            {
                return;
            }

            if (current.rawType == EventType.MouseDrag && !draggingItem && Vector2.Distance(current.mousePosition, dragStart) >= DragThreshold)
            {
                draggingItem = true;
                current.Use();
                return;
            }

            if (current.rawType == EventType.MouseUp && !draggingItem)
            {
                ClearDrag();
            }
        }

        private void ClearDrag()
        {
            dragItem = null;
            dragQuantity = 0;
            draggingItem = false;
            dragStart = Vector2.zero;
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

        private static int CountNonEmptyStacks(IReadOnlyList<InventoryStack> stacks)
        {
            if (stacks == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < stacks.Count; i++)
            {
                if (stacks[i] != null && stacks[i].Item != null && stacks[i].Quantity > 0)
                {
                    count++;
                }
            }

            return count;
        }
    }
}

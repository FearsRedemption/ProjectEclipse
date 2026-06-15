using System;
using System.Collections.Generic;
using ProjectEclipse.Crafting;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.Inventory
{
    public class InventoryStore : MonoBehaviour
    {
        [SerializeField] private List<InventoryStack> stacks = new List<InventoryStack>();

        public event Action Changed;

        public IReadOnlyList<InventoryStack> Stacks { get { return stacks; } }

        public bool AddItem(ItemDefinition item, int quantity)
        {
            if (item == null || quantity <= 0)
            {
                return false;
            }

            int remaining = quantity;
            for (int i = 0; i < stacks.Count && remaining > 0; i++)
            {
                if (stacks[i].CanStackWith(item))
                {
                    remaining = stacks[i].Add(remaining);
                }
            }

            while (remaining > 0)
            {
                int amount = Mathf.Min(remaining, item.StackLimit);
                stacks.Add(new InventoryStack(item, amount));
                remaining -= amount;
            }

            Changed?.Invoke();
            return true;
        }

        public int CountItem(ItemDefinition item)
        {
            if (item == null)
            {
                return 0;
            }

            int total = 0;
            for (int i = 0; i < stacks.Count; i++)
            {
                if (stacks[i].Item == item)
                {
                    total += stacks[i].Quantity;
                }
            }

            return total;
        }

        public bool HasItem(ItemDefinition item, int quantity)
        {
            return CountItem(item) >= quantity;
        }

        public bool HasIngredients(IEnumerable<CraftingIngredient> ingredients)
        {
            foreach (CraftingIngredient ingredient in ingredients)
            {
                if (ingredient == null || !HasItem(ingredient.Item, ingredient.Quantity))
                {
                    return false;
                }
            }

            return true;
        }

        public bool ConsumeIngredients(IEnumerable<CraftingIngredient> ingredients)
        {
            List<CraftingIngredient> snapshot = new List<CraftingIngredient>(ingredients);
            if (!HasIngredients(snapshot))
            {
                return false;
            }

            for (int i = 0; i < snapshot.Count; i++)
            {
                RemoveItem(snapshot[i].Item, snapshot[i].Quantity);
            }

            Changed?.Invoke();
            return true;
        }

        public bool RemoveItem(ItemDefinition item, int quantity)
        {
            if (item == null || quantity <= 0 || !HasItem(item, quantity))
            {
                return false;
            }

            int remaining = quantity;
            for (int i = stacks.Count - 1; i >= 0 && remaining > 0; i--)
            {
                InventoryStack stack = stacks[i];
                if (stack.Item != item)
                {
                    continue;
                }

                remaining -= stack.Remove(remaining);
                if (stack.Quantity <= 0)
                {
                    stacks.RemoveAt(i);
                }
            }

            Changed?.Invoke();
            return true;
        }

        public List<InventoryStack> GetSnapshot()
        {
            List<InventoryStack> snapshot = new List<InventoryStack>();
            for (int i = 0; i < stacks.Count; i++)
            {
                snapshot.Add(stacks[i].Copy());
            }

            return snapshot;
        }
    }
}


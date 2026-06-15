using ProjectEclipse.Inventory;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.Furnace
{
    public class FurnaceSystem : MonoBehaviour
    {
        [SerializeField] private int furnaceLevel = 1;
        [SerializeField] private float smeltingTime = 4f;
        [SerializeField] private FurnaceSlot fuelSlot = new FurnaceSlot(InventorySlotCategory.FurnaceFuel);
        [SerializeField] private FurnaceSlot inputSlot = new FurnaceSlot(InventorySlotCategory.FurnaceInput);
        [SerializeField] private FurnaceSlot outputSlot = new FurnaceSlot(InventorySlotCategory.FurnaceOutput);

        private float progress;
        private InventoryStore linkedInventory;

        public int FurnaceLevel { get { return Mathf.Max(1, furnaceLevel); } }
        public float SmeltingTime { get { return Mathf.Max(0.1f, smeltingTime); } }
        public float Progress01 { get { return Mathf.Clamp01(progress / SmeltingTime); } }
        public FurnaceSlot FuelSlot { get { return fuelSlot; } }
        public FurnaceSlot InputSlot { get { return inputSlot; } }
        public FurnaceSlot OutputSlot { get { return outputSlot; } }

        public void Initialize(InventoryStore inventory)
        {
            linkedInventory = inventory;
        }

        private void Update()
        {
            if (fuelSlot.IsEmpty || inputSlot.IsEmpty)
            {
                progress = 0f;
                return;
            }

            progress += Time.deltaTime;
            if (progress >= SmeltingTime)
            {
                progress = 0f;
                // TODO: Replace this placeholder transfer with smelting recipes and furnace upgrades.
                outputSlot.Add(inputSlot.Item, 1);
                inputSlot.Clear();
                fuelSlot.Clear();
            }
        }

        public bool TryMoveToRelevantSlot(ItemDefinition item, int amount)
        {
            if (linkedInventory == null || item == null || amount <= 0)
            {
                return false;
            }

            FurnaceSlot destination = item.ItemId.Contains("coal") ? fuelSlot : inputSlot;
            if (!destination.CanAccept(item) || !linkedInventory.HasItem(item, amount))
            {
                return false;
            }

            if (!destination.Add(item, amount))
            {
                return false;
            }

            linkedInventory.RemoveItem(item, amount);
            return true;
        }
    }
}


using System.Collections.Generic;
using ProjectEclipse.Items;
using UnityEngine;

namespace ProjectEclipse.Crafting
{
    public class CraftingPlan
    {
        private readonly Dictionary<ItemDefinition, int> requirements = new Dictionary<ItemDefinition, int>();
        private readonly Dictionary<ItemDefinition, int> reservations = new Dictionary<ItemDefinition, int>();
        private readonly Dictionary<ItemDefinition, int> missingMaterials = new Dictionary<ItemDefinition, int>();
        private readonly List<CraftingPlanStep> steps = new List<CraftingPlanStep>();
        private readonly List<string> blockingMessages = new List<string>();

        public CraftingRecipe FinalRecipe { get; private set; }
        public int TargetQuantity { get; private set; }
        public bool HasLoopError { get; private set; }

        public IReadOnlyList<CraftingPlanStep> Steps { get { return steps; } }
        public IReadOnlyDictionary<ItemDefinition, int> Requirements { get { return requirements; } }
        public IReadOnlyDictionary<ItemDefinition, int> Reservations { get { return reservations; } }
        public IReadOnlyDictionary<ItemDefinition, int> MissingMaterials { get { return missingMaterials; } }
        public IReadOnlyList<string> BlockingMessages { get { return blockingMessages; } }
        public bool HasBlockingProblems { get { return HasLoopError || blockingMessages.Count > 0 || missingMaterials.Count > 0; } }

        public CraftingPlan(CraftingRecipe finalRecipe, int targetQuantity)
        {
            FinalRecipe = finalRecipe;
            TargetQuantity = Mathf.Max(1, targetQuantity);
        }

        public void AddRequirement(ItemDefinition item, int quantity)
        {
            AddTo(requirements, item, quantity);
        }

        public void AddReservation(ItemDefinition item, int quantity)
        {
            AddTo(reservations, item, quantity);
        }

        public void AddMissingMaterial(ItemDefinition item, int quantity)
        {
            AddTo(missingMaterials, item, quantity);
        }

        public void AddBlockingMessage(string message)
        {
            if (!string.IsNullOrEmpty(message) && !blockingMessages.Contains(message))
            {
                blockingMessages.Add(message);
            }
        }

        public void AddLoopError(string message)
        {
            HasLoopError = true;
            AddBlockingMessage(message);
        }

        public void AddStep(CraftingPlanStep step)
        {
            if (step != null)
            {
                steps.Add(step);
            }
        }

        public int GetReserved(ItemDefinition item)
        {
            int value;
            return item != null && reservations.TryGetValue(item, out value) ? value : 0;
        }

        public bool HasProducerFor(ItemDefinition item)
        {
            for (int i = 0; i < steps.Count; i++)
            {
                if (steps[i] != null && steps[i].OutputItem == item)
                {
                    return true;
                }
            }

            return false;
        }

        private static void AddTo(Dictionary<ItemDefinition, int> dictionary, ItemDefinition item, int quantity)
        {
            if (dictionary == null || item == null || quantity <= 0)
            {
                return;
            }

            int existing;
            dictionary.TryGetValue(item, out existing);
            dictionary[item] = existing + quantity;
        }
    }
}

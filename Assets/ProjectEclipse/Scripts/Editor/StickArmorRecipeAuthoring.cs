#if UNITY_EDITOR
using ProjectEclipse.Crafting;
using ProjectEclipse.Items;
using UnityEditor;
using UnityEngine;

namespace ProjectEclipse.EditorTools
{
    [InitializeOnLoad]
    public static class StickArmorRecipeAuthoring
    {
        private const string SticksPath = "Assets/ProjectEclipse/Data/Items/TreeMaterial.asset";

        static StickArmorRecipeAuthoring()
        {
            EditorApplication.delayCall += RepairStickArmorRecipes;
        }

        [MenuItem("Project Eclipse/Repair Stick Armor Recipes")]
        public static void RepairStickArmorRecipes()
        {
            ItemDefinition sticks = AssetDatabase.LoadAssetAtPath<ItemDefinition>(SticksPath);
            if (sticks == null)
            {
                Debug.LogWarning("Project Eclipse could not repair Stick armor recipes because Sticks material is missing.");
                return;
            }

            bool changed = false;
            changed |= RepairRecipe("Assets/ProjectEclipse/Data/Recipes/StickHelmetRecipe.asset", "Assets/ProjectEclipse/Data/Equipment/StickHelmet.asset", sticks, 8);
            changed |= RepairRecipe("Assets/ProjectEclipse/Data/Recipes/StickChestRecipe.asset", "Assets/ProjectEclipse/Data/Equipment/StickChest.asset", sticks, 16);
            changed |= RepairRecipe("Assets/ProjectEclipse/Data/Recipes/StickGlovesRecipe.asset", "Assets/ProjectEclipse/Data/Equipment/StickGloves.asset", sticks, 6);
            changed |= RepairRecipe("Assets/ProjectEclipse/Data/Recipes/StickBootsRecipe.asset", "Assets/ProjectEclipse/Data/Equipment/StickBoots.asset", sticks, 6);

            if (changed)
            {
                AssetDatabase.SaveAssets();
                Debug.Log("Project Eclipse repaired Stick armor recipes to require Sticks.");
            }
        }

        private static bool RepairRecipe(string recipePath, string outputPath, ItemDefinition sticks, int stickCount)
        {
            CraftingRecipe recipe = AssetDatabase.LoadAssetAtPath<CraftingRecipe>(recipePath);
            ItemDefinition output = AssetDatabase.LoadAssetAtPath<ItemDefinition>(outputPath);
            if (recipe == null || output == null)
            {
                Debug.LogWarning("Project Eclipse could not repair Stick armor recipe at " + recipePath + ".");
                return false;
            }

            SerializedObject serializedRecipe = new SerializedObject(recipe);
            SerializedProperty ingredients = serializedRecipe.FindProperty("ingredients");
            SerializedProperty outputItem = serializedRecipe.FindProperty("outputItem");
            SerializedProperty outputQuantity = serializedRecipe.FindProperty("outputQuantity");
            bool changed = false;

            if (ingredients != null)
            {
                if (ingredients.arraySize != 1)
                {
                    ingredients.arraySize = 1;
                    changed = true;
                }

                SerializedProperty ingredient = ingredients.GetArrayElementAtIndex(0);
                SerializedProperty item = ingredient.FindPropertyRelative("item");
                SerializedProperty quantity = ingredient.FindPropertyRelative("quantity");
                if (item != null && item.objectReferenceValue != sticks)
                {
                    item.objectReferenceValue = sticks;
                    changed = true;
                }

                if (quantity != null && quantity.intValue != stickCount)
                {
                    quantity.intValue = stickCount;
                    changed = true;
                }
            }

            if (outputItem != null && outputItem.objectReferenceValue != output)
            {
                outputItem.objectReferenceValue = output;
                changed = true;
            }

            if (outputQuantity != null && outputQuantity.intValue != 1)
            {
                outputQuantity.intValue = 1;
                changed = true;
            }

            if (changed)
            {
                serializedRecipe.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(recipe);
            }

            return changed;
        }
    }
}
#endif

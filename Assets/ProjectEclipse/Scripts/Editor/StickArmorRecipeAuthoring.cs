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
        private const string EquipmentArtFolder = "Assets/ProjectEclipse/Art/Equipment";

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
            changed |= RepairEquipmentArt("Assets/ProjectEclipse/Data/Equipment/StickHelmet.asset", "stick_helmet_icon.png", "stick_helmet_equipped.png");
            changed |= RepairEquipmentArt("Assets/ProjectEclipse/Data/Equipment/StickChest.asset", "stick_chest_icon.png", "stick_chest_equipped.png");
            changed |= RepairEquipmentArt("Assets/ProjectEclipse/Data/Equipment/StickGloves.asset", "stick_gloves_icon.png", "stick_gloves_equipped.png");
            changed |= RepairEquipmentArt("Assets/ProjectEclipse/Data/Equipment/StickBoots.asset", "stick_boots_icon.png", "stick_boots_equipped.png");

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

        private static bool RepairEquipmentArt(string equipmentPath, string iconFileName, string equippedFileName)
        {
            ItemDefinition output = AssetDatabase.LoadAssetAtPath<ItemDefinition>(equipmentPath);
            if (output == null)
            {
                return false;
            }

            Sprite icon = LoadSprite(EquipmentArtFolder + "/" + iconFileName, 64f, new Vector2(0.5f, 0.5f));
            Sprite equipped = LoadSprite(EquipmentArtFolder + "/" + equippedFileName, 96f, new Vector2(0.5f, 0.08f));
            SerializedObject serialized = new SerializedObject(output);
            bool changed = false;
            if (icon != null)
            {
                changed |= SetObject(serialized, "icon", icon);
                changed |= SetObject(serialized, "worldDropSprite", icon);
            }

            if (equipped != null)
            {
                changed |= SetObject(serialized, "visualSprite", equipped);
            }

            if (changed)
            {
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(output);
            }

            return changed;
        }

        private static Sprite LoadSprite(string path, float pixelsPerUnit, Vector2 pivot)
        {
            EnsureSpriteImportSettings(path, pixelsPerUnit, pivot);
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void EnsureSpriteImportSettings(string path, float pixelsPerUnit, Vector2 pivot)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }

            if (!Mathf.Approximately(importer.spritePixelsPerUnit, pixelsPerUnit))
            {
                importer.spritePixelsPerUnit = pixelsPerUnit;
                changed = true;
            }

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            bool settingsChanged = false;
            if (settings.spriteAlignment != (int)SpriteAlignment.Custom)
            {
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settingsChanged = true;
            }

            if ((settings.spritePivot - pivot).sqrMagnitude > 0.0001f)
            {
                settings.spritePivot = pivot;
                settingsChanged = true;
            }

            if (settingsChanged)
            {
                importer.SetTextureSettings(settings);
                changed = true;
            }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            if (importer.filterMode != FilterMode.Bilinear)
            {
                importer.filterMode = FilterMode.Bilinear;
                changed = true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static bool SetObject(SerializedObject serialized, string name, Object value)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property == null || property.objectReferenceValue == value)
            {
                return false;
            }

            property.objectReferenceValue = value;
            return true;
        }
    }
}
#endif

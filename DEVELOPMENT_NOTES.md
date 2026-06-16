# Project Eclipse Development Notes

## Architecture Overview

The MVP scene now uses normal Unity scene composition. `Assets/ProjectEclipse/Scenes/ProjectEclipse_MVP.unity` contains serialized GameObjects for the camera, GameManager, player, platforms, furnace station, HUD, and placed enemies.

`MvpGameManager` is intentionally small. It wires serialized references at play time:

- equips the starter weapon
- initializes crafting with recipe assets
- connects furnace storage to the player inventory
- connects the HUD to player/storage/furnace systems
- gives placed enemies their player target and drop spawner

`PrototypeBootstrapper` is deprecated and no longer creates the world. It remains only as a compatibility stub so old scene references do not break compilation.

## Folder Map

- `Assets/ProjectEclipse/Scripts/Player`: movement, jumping, facing, input, and ground checks.
- `Assets/ProjectEclipse/Scripts/Combat`: health, damage, damageable interface, and weapon hit detection.
- `Assets/ProjectEclipse/Scripts/Enemies`: enemy definitions, AI controller, states, and drop tables.
- `Assets/ProjectEclipse/Scripts/Items`: item definitions, weapon definitions, drops, and drop spawning.
- `Assets/ProjectEclipse/Scripts/Inventory`: storage stacks and slot category enums.
- `Assets/ProjectEclipse/Scripts/Crafting`: recipe definitions and crafting execution.
- `Assets/ProjectEclipse/Scripts/Equipment`: equipped weapon and armor placeholders.
- `Assets/ProjectEclipse/Scripts/Furnace`: furnace level, fuel/input/output slots, and smelting timer foundation.
- `Assets/ProjectEclipse/Scripts/Progression`: resource tiers and dimension/boss-lock definitions.
- `Assets/ProjectEclipse/Scripts/UI`: test HUD, storage, crafting, and furnace panels.
- `Assets/ProjectEclipse/Scripts/Utilities`: sprite placeholders, sprite-sheet animation, camera follow, and runtime animation feedback.
- `Assets/ProjectEclipse/Scripts/Editor`: Unity editor-only generation helpers.
- `Assets/ProjectEclipse/Art/Creatures`: original generated creature sprite sheets.
- `Assets/ProjectEclipse/Art/Items`: original generated drop icons.
- `Assets/ProjectEclipse/Data`: committed ScriptableObject assets for items, weapons, enemies, recipes, and progression.
- `Assets/ProjectEclipse/Prefabs`: committed prefabs for player, enemies, drops, platforms, and furnace station.

## Adding Items

Add committed `ItemDefinition` assets under `Assets/ProjectEclipse/Data/Items` and reference them from recipes, drops, enemies, and equipment.

## Adding Weapons

Weapons are `WeaponDefinition` objects and are also items. Add new weapon data with:

- Archetype
- Damage
- Attack range
- Attack height
- Cooldown
- Knockback

The current combat code supports horizontal melee. Future weapon behaviors should branch from weapon archetype or a dedicated weapon behavior strategy.

Planned archetypes already exist:

- Fast claws
- Heavy hammer
- Bow
- Magic staff
- Summon

## Adding Enemies

Add a new `EnemyDefinition` asset with health, damage, speed, chase range, attack range, cooldown, lunge force, attack knockback, visual scale, collider size, sprite sheet reference, placeholder color, and drop entries.

For a new behavior family, keep the definition data-focused and add specialized behavior through an enemy controller variant or behavior module.

Current enemy behavior tuning:

- Tree Creature: smallest and slowest, with a short branch/root lunge.
- Stone Creature: slower and tankier, with stronger knockback.
- Coal Creature: fast, aggressive, and attacks more frequently.
- Copper Creature: largest current basic enemy, with stronger damage and a charge-like lunge.

## Adding Recipes

Recipes are `CraftingRecipe` objects. Add required `CraftingIngredient` entries and an output item. The crafting system already checks storage, consumes ingredients, and adds the result back to storage.

Weapon recipe outputs can auto-equip when `equipOutputIfWeapon` is true.

Recipe assets live under `Assets/ProjectEclipse/Data/Recipes` and are assigned to `MvpGameManager.availableRecipes`.

## Expanding Furnace Logic

The current `FurnaceSystem` has:

- Furnace level
- Fuel slot
- Input slot
- Output slot
- Smelting time

TODO:

- Add data-driven smelting recipes.
- Make Coal fuel value explicit.
- Add Copper processing output.
- Add upgrade slots and furnace upgrades.
- Add station interaction rules instead of always showing the furnace panel.

## Expanding Progression

Progression tiers are represented by `DimensionTierDefinition`.

Each tier can define:

- Tier ID
- Display name
- Required defeated boss ID
- Resource tier
- Available enemies
- Main boss placeholder
- Mini-boss placeholders

Current seed tiers are Earth / Forest, Stone, Coal, and Copper. Future dimensions should add Moon, Mars, cosmic tiers, elemental tiers, and god bosses.

## Animation Setup

`AnimationAssetGenerator` creates placeholder Unity Animator Controllers and AnimationClips for Player and Enemy objects. The generated state names are intentionally stable:

- Idle
- Move
- Attack
- Hurt
- Die

Runtime code drives Animator parameters named `IsMoving`, `IsGrounded`, `Attack`, `Hurt`, and `Die`. Final art can replace the generated clips without changing gameplay code.

Creature sheets use a reusable code-driven setup:

- `SpriteSheetAnimator` slices 96x96 frames from each sheet.
- Rows are ordered Idle, Move, Attack, Hurt, Die.
- Frame counts are Idle 4, Move 6, Attack 6, Hurt 2, Die 6.
- `VisualStateAnimator` forwards movement and trigger state changes to the sheet animator.
- Enemy definition assets assign sprite sheet references, visual scale, collider size, and behavior tuning.

The current sheets are original generated placeholder art. They are meant to establish readable silhouettes and animation timing, not final production art.

## Known TODOs

- Replace runtime-created ScriptableObjects with committed data assets after initial tuning.
- Add true weapon behavior modules for ranged, magic, summon, and fast melee styles.
- Add armor and upgrade stat effects.
- Add proper UI Toolkit or UGUI UI after gameplay loops settle.
- Add respawn and checkpoint handling.
- Add boss gates and tier unlock persistence.
- Add automated play-mode tests once Unity is installed in the development environment.

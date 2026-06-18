# Project Eclipse

Project Eclipse is a standalone Unity 2D side-scrolling combat and grinding prototype. The long-term fantasy is Terraria-style boss-locked progression and crafted equipment with MapleStory-like enemy grinding, pop-out drops, and weapon-driven combat.

This is not a Terraria mod, not a tModLoader project, and does not depend on any paid assets.

## Unity Version

The project targets Unity 6000.4.5f1, recorded in `ProjectSettings/ProjectVersion.txt`.

The package manifest includes the built-in Unity modules needed for 2D sprites, 2D physics, animation, audio, legacy input, IMGUI, and UI. Unity will regenerate `Packages/packages-lock.json` locally when the project opens.

## Current MVP Scope

- Normal Unity scene at `Assets/ProjectEclipse/Scenes/ProjectEclipse_MVP.unity` with serialized scene objects.
- Handcrafted visible Player, terrain, creature areas, furnace station, enemies, and HUD objects placed directly in the scene.
- Homemade prefab copies exist only as reusable references; the MVP scene does not depend on invisible runtime level generation.
- ScriptableObject data assets for items, weapons, enemies, recipes, and progression tiers.
- Warrior class foundation asset under `Assets/ProjectEclipse/Data/Classes`.
- Inventory foundation now separates Equipment, Materials, Consumables, and Key Items / Special Items.
- Inventory UI is split into focused IMGUI helper classes with icon slots and icon-backed tooltips.
- 2D side-scroller player movement, jumping, facing, ground detection, health, and simple death handling.
- Horizontal melee attack in front of the player.
- Starter Blade equipped by default.
- Stone Cleaver craftable from Stone drops.
- Tree, Stone, Coal, and Copper creatures with distinct original chibi side-scroller sprite sheets, sizes, animations, combat stats, and reusable drop table assets.
- Drops pop upward and sideways from defeated enemies, then collect into storage.
- Sticks, Stone, Coal, Copper Ore, Iron Ore, and Gold Ore have distinct homemade inventory icons and world drop sprites.
- Starter Blade, Stone Cleaver, Training Shield, Traveler Cape, Furnace Port, and Cauldron Port now have dedicated original item art instead of borrowing material icons.
- Forest, Stone, Coal, and Copper platform art has clearer area-specific surfaces and top edges.
- Infinite-style storage foundation with stack sizes up to 999.
- Small HUD by default, with storage, crafting, and furnace panels toggled open with Tab.
- Furnace model with fuel, input, output, level, and smelting timer placeholders.
- Data structures for Earth/Forest, Stone, Coal, and Copper progression tiers.
- Progression skeleton models for stages, world tiers, bosses, unlock requirements, resource tiers, crafting tiers, and recommended levels.
- Weapon data now supports separate inventory icons, world drop sprites, and equipped in-hand visual sprites.
- Equipment data models support mainhand, offhand, helmet, chest, boots, gloves, accessories, belt, and back/cape slots.
- Inventory crafting port data seeds support furnace and cauldron ports for future inventory-based crafting.
- World drops now have magnet pickup behavior after a short delay and warn when art is missing.

## Controls

- Move: A/D or Left/Right arrows
- Jump: Space, W, or Up arrow
- Attack: J, Left Ctrl, or left mouse button
- Inventory/storage: Tab
- Shift-click storage item buttons:
  - Weapons attempt to equip.
  - Coal and copper-related materials attempt to move into the furnace model.

## Current Enemies And Drops

- Tree Creature: small animated stump/sapling monster. Slowest basic enemy, simple root/branch lunge, drops Sticks.
- Stone Creature: bulkier rock golem. Slower, higher health, heavier knockback, drops Stone.
- Coal Creature: fast charcoal imp. Quicker chase and shorter attack cooldown, drops Coal with a chance for extra Stone.
- Copper Creature: largest current basic enemy. Tougher mineral beast with stronger damage, charge-like lunge, drops Copper Ore with a chance for Coal.
- Future Iron Creature drop table seed: Iron Ore.
- Future Gold Creature drop table seed: Gold Ore.

## Current Crafting Recipes

- Stone Cleaver: Stone x4.
- Basic Furnace: Stone x12, Coal x3.
- Copper Whetstone Placeholder: Copper Ore x8, Coal x2.

## Progression Direction

Project Eclipse is structured around enemy and boss-gated resource tiers instead of mining blocks. The current MVP seeds these tiers:

- Earth / Forest
- Earth / Stone Tier
- Earth / Coal Tier
- Earth / Copper Tier

The intended long-term structure expands from Earth into Moon, Mars, and deeper cosmic or elemental dimensions. Each dimension should eventually contain multiple mini-bosses before a main boss or god unlocks the next progression layer.

## Animation Notes

The project includes `Assets/ProjectEclipse/Scripts/Editor/AnimationAssetGenerator.cs`. When the project opens in Unity, it creates placeholder Player and Enemy Animator Controllers under `Assets/ProjectEclipse/Animations` if they do not already exist.

Creature visuals use original chibi fantasy sprite sheets under `Assets/ProjectEclipse/Art/Creatures/`:

- `tree_creature_sheet.png`
- `stone_creature_sheet.png`
- `coal_creature_sheet.png`
- `copper_creature_sheet.png`

Each creature sheet has five animation rows:

- Idle
- Move
- Attack
- Hurt
- Die

The player also has an original polished chibi sheet under `Assets/ProjectEclipse/Art/Player/` with Idle, Run, Jump, Attack, Hurt, and Die rows. `SpriteSheetAnimator` slices these sheets at runtime and is driven by `VisualStateAnimator`, so the player and enemies visibly transition during the current MVP loop. The generated Animator Controllers remain as a Unity-native placeholder path for future hand-authored clips.

## Scene And Data Workflow

The MVP no longer relies on `PrototypeBootstrapper` creating the entire world in `Awake`. The scene contains real editable objects:

- Main Camera
- GameManager with `MvpGameManager` and `DropSpawner`
- Player
- Ground and custom platform objects
- Tree Creature Area, Stone Creature Area, Coal Creature Area, and Copper Creature Area backdrop objects
- Furnace station
- Placed Tree, Stone, Coal, and Copper creatures
- UI HUD object

`MvpGameManager` only wires serialized references at play time: player systems, crafting recipes, furnace storage, HUD, enemy targets, and drop spawning.

Data assets live under:

- `Assets/ProjectEclipse/Data/Classes`
- `Assets/ProjectEclipse/Data/CraftingPorts`
- `Assets/ProjectEclipse/Data/DropTables`
- `Assets/ProjectEclipse/Data/Equipment`
- `Assets/ProjectEclipse/Data/Items`
- `Assets/ProjectEclipse/Data/Weapons`
- `Assets/ProjectEclipse/Data/Enemies`
- `Assets/ProjectEclipse/Data/Recipes`
- `Assets/ProjectEclipse/Data/Progression`

Prefab copies live under these folders for later reuse, but current MVP iteration should prioritize placing and tuning visible homemade scene objects first:

- `Assets/ProjectEclipse/Prefabs/Player`
- `Assets/ProjectEclipse/Prefabs/Enemies`
- `Assets/ProjectEclipse/Prefabs/Items`
- `Assets/ProjectEclipse/Prefabs/World`

## Next Work

- Open the project in Unity 6000.4.5f1.
- Let Unity regenerate `Packages/packages-lock.json` and any local Library data.
- Needs Unity machine testing: confirm C# compilation, inspect new serialized fields, and run Play Mode.
- Needs Unity machine testing: tune enemy ledge/platform probe distances per creature size.
- Needs Unity machine testing: inspect the IMGUI inventory tabs, item tooltip placement, and shift-click equip/port actions.
- Needs Unity machine testing: add/assign `CombatInputRouter`, `InventoryCraftingController`, and layered visual anchors where appropriate.
- Needs Unity machine testing: tune acceleration, deceleration, coyote time, jump buffering, fall gravity, sprint, and magnet pickup feel.
- Needs visual inspection in Unity: confirm player base sprite no longer appears to permanently include the weapon once a separate weapon anchor/renderer is added to the player prefab or scene object.
- Needs visual inspection in Unity: tune equipped weapon sprite offsets, scale, sorting order, and attack readability.
- Needs visual inspection in Unity: player body, armor, offhand, and back/cape layers are only model-supported right now; they need actual anchor setup and original art.
- Needs visual inspection in Unity: check new item icons, world drop sprites, equipment visuals, and platforms at gameplay zoom and actual UI size.
- Continue improving the homemade sheets and world sprites in-place.
- Convert code-driven sprite clips into authored Animator clips if the project moves away from runtime slicing.
- Expand the committed ScriptableObject assets instead of adding runtime-created data.
- Add proper respawn, save/load, stations, armor, upgrades, boss gates, and projectile weapon variants.

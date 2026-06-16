# Project Eclipse

Project Eclipse is a standalone Unity 2D side-scrolling combat and grinding prototype. The long-term fantasy is Terraria-style boss-locked progression and crafted equipment with MapleStory-like enemy grinding, pop-out drops, and weapon-driven combat.

This is not a Terraria mod, not a tModLoader project, and does not depend on any paid assets.

## Unity Version

The project targets Unity 6000.4.5f1, recorded in `ProjectSettings/ProjectVersion.txt`.

The package manifest includes the built-in Unity modules needed for 2D sprites, 2D physics, animation, audio, legacy input, IMGUI, and UI. Unity will regenerate `Packages/packages-lock.json` locally when the project opens.

## Current MVP Scope

- Normal Unity scene at `Assets/ProjectEclipse/Scenes/ProjectEclipse_MVP.unity` with serialized scene objects.
- Prefabs for the player, current enemies, world item drops, platforms, and furnace station.
- ScriptableObject data assets for items, weapons, enemies, recipes, and progression tiers.
- 2D side-scroller player movement, jumping, facing, ground detection, health, and simple death handling.
- Horizontal melee attack in front of the player.
- Starter Blade equipped by default.
- Stone Cleaver craftable from Stone drops.
- Tree, Stone, Coal, and Copper creatures with distinct original sprite sheets, sizes, animations, combat stats, and drops.
- Drops pop upward and sideways from defeated enemies, then collect into storage.
- Wood, stone, coal, and copper drops have distinct generated icons.
- Infinite-style storage foundation with stack sizes up to 999.
- Basic crafting panel and furnace panel through IMGUI.
- Furnace model with fuel, input, output, level, and smelting timer placeholders.
- Data structures for Earth/Forest, Stone, Coal, and Copper progression tiers.

## Controls

- Move: A/D or Left/Right arrows
- Jump: Space, W, or Up arrow
- Attack: J, Left Ctrl, or left mouse button
- Shift-click storage item buttons:
  - Weapons attempt to equip.
  - Coal and copper-related materials attempt to move into the furnace model.

## Current Enemies And Drops

- Tree Creature: small animated stump/sapling monster. Slowest basic enemy, simple root/branch lunge, drops Tree Material.
- Stone Creature: bulkier rock golem. Slower, higher health, heavier knockback, drops Stone.
- Coal Creature: fast charcoal imp. Quicker chase and shorter attack cooldown, drops Coal with a chance for extra Stone.
- Copper Creature: largest current basic enemy. Tougher mineral beast with stronger damage, charge-like lunge, drops Copper Fragments with a chance for Coal.

## Current Crafting Recipes

- Stone Cleaver: Stone x10.
- Basic Furnace: Stone x12, Coal x3.
- Copper Whetstone Placeholder: Copper Fragments x8, Coal x2.

## Progression Direction

Project Eclipse is structured around enemy and boss-gated resource tiers instead of mining blocks. The current MVP seeds these tiers:

- Earth / Forest
- Earth / Stone Tier
- Earth / Coal Tier
- Earth / Copper Tier

The intended long-term structure expands from Earth into Moon, Mars, and deeper cosmic or elemental dimensions. Each dimension should eventually contain multiple mini-bosses before a main boss or god unlocks the next progression layer.

## Animation Notes

The project includes `Assets/ProjectEclipse/Scripts/Editor/AnimationAssetGenerator.cs`. When the project opens in Unity, it creates placeholder Player and Enemy Animator Controllers under `Assets/ProjectEclipse/Animations` if they do not already exist.

Creature visuals now use original generated sprite sheets under `Assets/ProjectEclipse/Art/Creatures/`:

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

`SpriteSheetAnimator` slices these sheets at runtime and is driven by `VisualStateAnimator`, so the enemies visibly transition between idle, moving, attacking, hurt, and dying states during the current MVP loop. The generated Animator Controllers remain as a Unity-native placeholder path for future hand-authored clips.

## Scene And Data Workflow

The MVP no longer relies on `PrototypeBootstrapper` creating the entire world in `Awake`. The scene contains real editable objects:

- Main Camera
- GameManager with `MvpGameManager` and `DropSpawner`
- Player
- Ground and platform blocks
- Furnace station
- Placed Tree, Stone, Coal, and Copper creatures
- MVP HUD object

`MvpGameManager` only wires serialized references at play time: player systems, crafting recipes, furnace storage, HUD, enemy targets, and drop spawning.

Data assets live under:

- `Assets/ProjectEclipse/Data/Items`
- `Assets/ProjectEclipse/Data/Weapons`
- `Assets/ProjectEclipse/Data/Enemies`
- `Assets/ProjectEclipse/Data/Recipes`
- `Assets/ProjectEclipse/Data/Progression`

Prefabs live under:

- `Assets/ProjectEclipse/Prefabs/Player`
- `Assets/ProjectEclipse/Prefabs/Enemies`
- `Assets/ProjectEclipse/Prefabs/Items`
- `Assets/ProjectEclipse/Prefabs/World`

## Next Work

- Open the project in Unity 6000.4.5f1.
- Let Unity regenerate `Packages/packages-lock.json` and any local Library data.
- Replace generated placeholder sheets with final production sprite sheets.
- Convert code-driven sprite clips into authored Animator clips if the project moves away from runtime slicing.
- Expand the committed ScriptableObject assets instead of adding runtime-created data.
- Add proper respawn, save/load, stations, armor, upgrades, boss gates, and projectile weapon variants.

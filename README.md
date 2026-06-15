# Project Eclipse

Project Eclipse is a standalone Unity 2D side-scrolling combat and grinding prototype. The long-term fantasy is Terraria-style boss-locked progression and crafted equipment with MapleStory-like enemy grinding, pop-out drops, and weapon-driven combat.

This is not a Terraria mod, not a tModLoader project, and does not depend on any paid assets.

## Unity Version

The project targets Unity 6.4.11f1, recorded in `ProjectSettings/ProjectVersion.txt`.

This target was chosen because Unity's current Unity 6 documentation identifies Unity 6.4 as the latest Unity 6 release family, and Unity's official release archive lists 6000.4.11f1 as released on June 10, 2026. The local machine used for this scaffold did not have Unity Hub or the Unity Editor installed, so the first validation pass is source and repository validation only.

## Current MVP Scope

- Runtime-built playable Unity 2D scene at `Assets/ProjectEclipse/Scenes/ProjectEclipse_MVP.unity`.
- 2D side-scroller player movement, jumping, facing, ground detection, health, and simple death handling.
- Horizontal melee attack in front of the player.
- Starter Blade equipped by default.
- Stone Cleaver craftable from Stone drops.
- Tree, Stone, Coal, and Copper creatures with increasing health, damage, speed, and drops.
- Drops pop upward and sideways from defeated enemies, then collect into storage.
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

- Tree Creature: drops Tree Material.
- Stone Creature: drops Stone.
- Coal Creature: drops Coal, with a chance for extra Stone.
- Copper Creature: drops Copper Fragments, with a chance for Coal.

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

The generated controllers use these states:

- Idle
- Move
- Attack
- Hurt
- Die

The runtime `VisualStateAnimator` also provides visible placeholder bob, hurt, attack, and death feedback, so the scene remains readable even before final sprite sheets or hand-authored clips are added.

## Next Work

- Open the project in Unity 6.4.11f1 or newer Unity 6.4 patch release.
- Let Unity generate missing `.meta` files and placeholder animation assets.
- Replace procedural square sprites with imported 2D sprite sheets.
- Add real animation clips to the generated controllers.
- Convert runtime catalog data into committed ScriptableObject assets once the gameplay direction settles.
- Add proper respawn, save/load, stations, armor, upgrades, boss gates, and projectile weapon variants.


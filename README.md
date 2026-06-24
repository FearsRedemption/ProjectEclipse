# Project Eclipse

Project Eclipse is a standalone Unity 2D side-scrolling monster-grinding crafting RPG prototype. The long-term fantasy is readable MapleStory-like platform grinding and loot, Terraria-like movement-heavy boss fights, BDO-inspired class skill flow, and inventory-driven equipment/crafting progression.

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
- Tab now opens one unified IMGUI inventory/equipment/crafting screen instead of separate prototype windows.
- Inventory tabs now separate Equipment, Usable, Materials, Misc, and Key Items.
- Left-side character equipment area shows mainhand, offhand, armor, accessory, belt, and back slots around a paper-doll placeholder.
- Crafting ports are shown as equipment-adjacent Furnace, Cauldron, Forge, Anvil, and Utility slots.
- Inventory UI is split into focused IMGUI helper classes with icon slots, right-click interactions, normal item tooltips, equipment comparison tooltips, and crafting-port comparison tooltips.
- Integrated crafting now has a craft amount selector for 1 / 5 / 10 / 50 / 100 / Custom, with safe clamping and reset behavior.
- Crafting now creates one active Work Order with dependency-aware planning, requirement feedback, simple logical material reservations, and port-lane processing.
- Work Orders can auto-queue craftable intermediates from known recipes, wait on missing materials or ports, and continue when inventory changes make pending steps possible.
- The MVP recipe list now includes a Copper Sword Work Order chain: Copper Ore -> Copper Ingot, Birch Log -> Birch Rod, then Copper Sword at an Anvil Port.
- Furnace, Utility, and Anvil crafting ports are now craftable inventory items rather than invisible scene-only unlocks.
- Work Order tracker lines are ordered as final item, direct ingredients, and immediate raw inputs, with missing-port lines near the step they block.
- A disabled Debug/Test Copper Sword kit exists on `MvpGameManager` for Unity validation without seeding normal starter inventory.
- Different crafting station/port types can process in parallel; basic ports expose one lane by default, with lane count modeled for future upgrades.
- Crafting feedback includes Queue Started, Insufficient Materials, Missing Crafting Port, Insufficient Crafting Port Tier, Recipe Locked, and Work Order Complete states.
- 2D side-scroller player movement, jumping, facing, ground detection, health, and simple death handling.
- Mainhand and offhand attacks now resolve a Terraria-like mouse aim direction, with facing-direction fallback if no camera/mouse world point is available.
- Warrior Q/E/R/F now execute playable placeholder skills: Cleave, Guard Break, Leap Strike, and Battle Cry.
- Starter Blade is equipped by default without seeding extra starter inventory items.
- Stone Cleaver craftable from Stone drops.
- Tree, Stone, Coal, and Copper creatures with replaced original chibi side-scroller sprite sheets, sizes, animation rows, combat stats, and reusable drop table assets.
- Drops pop upward and sideways from defeated enemies, then collect into storage.
- Sticks, Stone, Coal, Copper Ore, and Birch Log drops are seeded as monster materials; Birch Log currently uses the dev missing-icon fallback.
- Infinite-style storage foundation with stack sizes up to 999.
- Small HUD by default, with the unified inventory/equipment/crafting screen toggled open with Tab.
- Furnace model with fuel, input, output, level, and smelting timer placeholders.
- Data structures for Earth/Forest, Stone, Coal, and Copper progression tiers.
- Progression skeleton models for stages, world tiers, bosses, unlock requirements, resource tiers, crafting tiers, and recommended levels.
- Weapon data now supports separate inventory icons, world drop sprites, and equipped in-hand visual sprites.
- Equipment data models support mainhand, offhand, helmet, chest, boots, gloves, accessories, belt, and back/cape slots.
- Active item icons have been replaced with cleaner original side-scroller material art for Sticks, Stone, Coal, Copper Ore, Iron Ore, Gold Ore, shield, cape, furnace port, and cauldron port.
- Starter Blade and Stone Cleaver now have separate inventory/equipped sprites, with shield and cape equipped visuals prepared for future layered anchors.
- Connector-safe platform kits live under `Assets/ProjectEclipse/Art/Platforms`; left, middle, and right pieces share flat seam edges so they can assemble into longer platforms.
- The platform kit library now includes additional art-only ore, forest, winter, elemental, and biome variants for future crafting-map stages.
- Inventory crafting port data seeds support furnace, cauldron, anvil, and utility ports as real item-like, slottable, upgrade-ready crafting ports.
- World drops now have magnet pickup behavior after a short delay and warn when art is missing.

## Controls

- Move: A/D or Left/Right arrows
- Jump: Space, W, or Up arrow
- Mainhand attack: J, Left Ctrl, or left mouse button
- Offhand action: right mouse button
- Warrior skills: Q, E, R, F
- Sprint / modifier: Shift
- Inventory/storage: Tab
- Right-click inventory equipment or crafting-port items to equip/swap them into the matching slot.
- Right-click equipped gear or equipped crafting ports to unequip them back into inventory.
- Shift-click remains a secondary shortcut for inventory equip behavior.

## Current Enemies And Drops

- Tree Creature: small animated stump/sapling monster. Slowest basic enemy, simple root/branch lunge, drops Sticks and Birch Log.
- Stone Creature: bulkier rock golem. Slower, higher health, heavier knockback, drops Stone.
- Coal Creature: fast charcoal imp. Quicker chase and shorter attack cooldown, drops Coal with a chance for extra Stone.
- Copper Creature: largest current basic enemy. Tougher mineral beast with stronger damage, charge-like lunge, drops Copper Ore with a chance for Coal.
- Future Iron Creature drop table seed: Iron Ore.
- Future Gold Creature drop table seed: Gold Ore.

## Current Crafting Recipes

- Stone Cleaver: Stone x4.
- Basic Furnace: Stone x12, Coal x3.
- Copper Whetstone Placeholder: Copper Ore x8, Coal x2.
- Furnace Port: Stone x12, Coal x3.
- Utility Port: Sticks x12, Birch Log x4.
- Anvil Port: Stone x20, Copper Ingot x5.
- Smelt Copper Ingot: Copper Ore x10 at a Furnace Port.
- Carve Birch Rod: Birch Log x10 at a Utility Port.
- Copper Sword: Copper Ingot x30, Birch Rod x10 at an Anvil Port. One sword requires 300 Copper Ore and 100 Birch Logs through the dependency chain.

Recipes craft from inventory materials. `Inventory` recipes only require ingredients; port-gated recipes require the matching equipped crafting port. Crafted output returns to inventory unless a recipe explicitly opts into auto-equipping.

The first Work Order implementation is intentionally small:

- One active Work Order at a time.
- One deterministic producer recipe per output item.
- Logical reservation display for inputs reserved by the active Work Order, using available counts in recipe previews.
- Processing jobs consume inputs when a step starts, then add outputs when the timer completes.
- The tracker keeps consumed Work Order inputs visible so raw/intermediate lines do not appear to vanish after processing starts.
- Material-change blocked steps retry automatically once required materials and ports are available again.
- Completion cue hooks support an assigned `AudioClip`; if no clip exists, text cues such as `TINK TINK TINK` are displayed/logged for smithing/anvil recipes.

## Progression Direction

Project Eclipse is structured around enemy and boss-gated resource tiers instead of mining blocks. Materials should primarily come from monster drops and processing monster drops, not mining/chopping/survival gathering. The current MVP seeds these tiers:

- Earth / Forest
- Earth / Stone Tier
- Earth / Coal Tier
- Earth / Copper Tier

The intended long-term structure expands from Earth into Moon, Mars, and deeper cosmic or elemental dimensions. Each dimension should eventually contain multiple mini-bosses before a main boss or god unlocks the next progression layer. Bosses should test movement, dodge timing, positioning, telegraph reading, arena control, projectiles, phases, and skill usage instead of becoming static unavoidable damage sponges.

Crafting-port progression direction:

- Basic/Stone ports start slow and one-lane.
- Copper ports should become faster and unlock more recipes.
- Tin plus Copper can lead into Bronze processing.
- Bronze, Iron, Silver, Gold, and gem tiers can later add extra lanes, sockets, elemental modifiers, accessories, skill modifiers, and port upgrades.

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

The player also has an original polished chibi sheet under `Assets/ProjectEclipse/Art/Player/` with Idle, Run, Jump, Attack, Hurt, and Die rows. The current active player base sheet is weapon-free so mainhand weapons can be rendered later as a separate equipped visual layer. `SpriteSheetAnimator` slices these sheets at runtime and is driven by `VisualStateAnimator`, so the player and enemies can transition during the current MVP loop. The generated Animator Controllers remain as a Unity-native placeholder path for future hand-authored clips.

Animation rows now use variable frame counts. The player sheet is `768x576` with 8 possible 96x96 cells per row. Creature sheets are `768x480`. Blank trailing cells are intentional, and `SpriteSheetAnimator` detects non-empty cells per row so animations can use the number of frames they need.

The active art no longer keeps separate idle PNG files; idle lives in row 0 of each full sheet. Rejected player and creature PNGs were archived under `ArtArchive/RejectedCharacterCreatureSprites_2026-06-19` and `ArtArchive/RejectedCharacterCreatureSprites_2026-06-19_FixedFrameRedo` outside `Assets` so Unity does not import duplicate old art. The active replacements were locally checked as 96x96 sprite sheets for dimensions, frame counts, blank trailing cells, magenta chroma residue, and basic frame-to-frame motion/contact-sheet readability. This was not Unity Play Mode validation.

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
- Needs Unity machine testing: inspect the unified IMGUI inventory/equipment/crafting screen, item tooltip placement, comparison tooltip placement, right-click equip/swap, and right-click unequip behavior.
- Needs Unity machine testing: inspect craft amount selector reset behavior, custom amount input, requirement colors, and insufficient-material feedback.
- Needs Unity machine testing: verify Work Order planning, missing-material waiting, cancellation, completion, and active HUD tracker behavior.
- Needs Unity machine testing: verify crafting-port lane processing, same-port serial jobs, different-port parallel jobs, and port tier/lock feedback.
- Needs Unity machine testing: assign/test completion `AudioClip` hooks; until then, text cue fallback/logging is the expected behavior.
- Needs Unity machine testing: verify runtime-created `InventoryCraftingController` wiring or add it to the player prefab/scene object explicitly after inspection.
- Needs Unity machine testing: verify LMB/RMB mouse aim, Warrior Q/E/R/F skill effects, cooldowns, knockback, and Shift sprint/modifier behavior.
- Needs Unity machine testing: add/assign layered visual anchors where appropriate.
- Needs Unity machine testing: tune acceleration, deceleration, coyote time, jump buffering, fall gravity, sprint, and magnet pickup feel.
- Needs visual inspection in Unity: confirm player base sprite no longer appears to permanently include the weapon once a separate weapon anchor/renderer is added to the player prefab or scene object.
- Needs visual inspection in Unity: tune equipped weapon sprite offsets, scale, sorting order, and attack readability.
- Needs visual inspection in Unity: player body, armor, offhand, and back/cape layers are only model-supported right now; they need actual anchor setup and original art.
- Needs visual inspection in Unity: verify replacement icons, equipped sprites, and connector-safe platform pieces import at the right pixels-per-unit, sorting, collision, and gameplay scale.
- Needs visual inspection in Unity: check rebuilt and expanded platform kits in left/right, left/middle/right, and repeated-middle arrangements.
- Needs visual inspection in Unity: review the replaced player and creature animation sheets at actual game scale, including player run/weaponless attack, creature walk/attack/hurt/death rows, import alpha, pixels-per-unit, sorting, and runtime slice timing.
- Continue improving the homemade sheets and world sprites in-place.
- Convert code-driven sprite clips into authored Animator clips if the project moves away from runtime slicing.
- Expand the committed ScriptableObject assets instead of adding runtime-created data.
- Add proper respawn, save/load, stations, armor, upgrades, boss gates, and projectile weapon variants.

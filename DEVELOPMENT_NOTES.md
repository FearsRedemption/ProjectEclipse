# Project Eclipse Development Notes

## Architecture Overview

The MVP scene now uses normal Unity scene composition. `Assets/ProjectEclipse/Scenes/ProjectEclipse_MVP.unity` contains serialized GameObjects for the camera, GameManager, custom player, custom platforms, creature areas, furnace station, HUD, and placed enemies.

For the MVP, the visible scene is the source of truth. Build and tune homemade objects directly in the scene first. Save a prefab only after that object already works and feels right in the handcrafted scene.

`MvpGameManager` is intentionally small. It wires serialized references at play time:

- equips the starter weapon
- can use the Warrior class asset as the default class foundation when assigned
- initializes crafting with recipe assets
- connects furnace storage to the player inventory
- connects the Tab-toggled HUD to player/storage/furnace systems
- gives placed enemies their player target and drop spawner

`PrototypeBootstrapper` is deprecated and no longer creates the world. It remains only as a compatibility stub so old scene references do not break compilation.

## Folder Map

- `Assets/ProjectEclipse/Scripts/Player`: movement, jumping, facing, input, and ground checks.
- `Assets/ProjectEclipse/Scripts/Combat`: health, damage, damageable interface, and weapon hit detection.
- `Assets/ProjectEclipse/Scripts/Combat`: also contains action/input routing foundations for mainhand, offhand, Q/E/R/F, and Shift modifiers.
- `Assets/ProjectEclipse/Scripts/Enemies`: enemy definitions, AI controller, states, and drop table definitions.
- `Assets/ProjectEclipse/Scripts/Items`: item definitions, weapon definitions, drops, and drop spawning.
- `Assets/ProjectEclipse/Scripts/Inventory`: storage stacks, inventory tabs, and slot category enums.
- `Assets/ProjectEclipse/Scripts/Crafting`: recipe definitions and crafting execution.
- `Assets/ProjectEclipse/Scripts/Equipment`: equipment slots, stats, rarity, restrictions, layered visual anchors, and equipped item state.
- `Assets/ProjectEclipse/Scripts/Furnace`: furnace level, fuel/input/output slots, and smelting timer foundation.
- `Assets/ProjectEclipse/Scripts/Progression`: resource tiers, crafting tiers, stages, world tiers, boss definitions, and unlock requirement models.
- `Assets/ProjectEclipse/Scripts/UI`: small HUD plus Tab-toggled storage, crafting, and furnace panels.
- `Assets/ProjectEclipse/Scripts/Utilities`: sprite placeholders, sprite-sheet animation, camera follow, and runtime animation feedback.
- `Assets/ProjectEclipse/Scripts/Editor`: Unity editor-only generation helpers.
- `Assets/ProjectEclipse/Art/Player`: homemade player idle/run/jump/attack/hurt/death sheet.
- `Assets/ProjectEclipse/Art/Creatures`: homemade creature sprite sheets.
- `Assets/ProjectEclipse/Art/Items`: homemade drop icons.
- `Assets/ProjectEclipse/Art/World`: homemade platform, area backdrop, and furnace sprites.
- `Assets/ProjectEclipse/Data`: committed ScriptableObject assets for classes, crafting ports, drop tables, items, weapons, enemies, recipes, and progression.

## Inventory Direction

Inventory tabs are:

- Equipment
- Materials
- Consumables
- Key Items / Special Items

Monster drops are crafting materials and belong in Materials. Current material drops are `Sticks`, `Stone`, `Coal`, `Copper Ore`, future `Iron Ore`, and future `Gold Ore`.

The MVP HUD is still IMGUI and still needs Unity inspection, but it now follows the intended tab structure and exposes tooltip data hooks.

The HUD code is split into focused IMGUI helper classes so it can later move to UGUI/UI Toolkit without growing `MvpHud` into a catch-all:

- `InventoryPanel`
- `EquipmentPanel`
- `ItemGridView`
- `ItemSlotView`
- `ItemTooltipView`
- `CraftingPanel`
- `CraftingPortPanel`
- `Assets/ProjectEclipse/Prefabs`: reusable copies for later, not the primary MVP authoring path.

## Adding Items

Add committed `ItemDefinition` assets under `Assets/ProjectEclipse/Data/Items` and reference them from recipes, drop tables, enemies, and equipment.

Item definitions support:

- Item icon
- World drop sprite
- Resource tier
- Stack size
- Item category
- Tooltip description
- Dropped-by text
- Crafting usage text

Current material names:

- Tree Creature drops `Sticks`.
- Stone/Rock Creature drops `Stone`.
- Coal Creature drops `Coal`.
- Copper Creature drops `Copper Ore`.
- Future Iron Creature drops `Iron Ore`.
- Future Gold Creature drops `Gold Ore`.

## Adding Weapons

Weapons are `WeaponDefinition` objects and are also items. Add new weapon data with:

- Archetype
- Damage
- Attack range
- Attack height
- Cooldown
- Knockback
- Equipped visual sprite, offset, rotation, and scale
- Equipment slot, rarity, stats, class restriction, level requirement, and visual layer

The current combat code supports horizontal melee. Future weapon behaviors should branch from weapon archetype or a dedicated weapon behavior strategy. The player base sprite should remain weapon-free long term; equipped weapons should be rendered through a separate `WeaponVisualAnchor` layer/anchor.

Equipment slots now include:

- Mainhand
- Offhand
- Helmet
- Chest
- Boots
- Gloves
- Necklace
- Ring 1
- Ring 2
- Earring 1
- Earring 2
- Belt
- Back

The Back slot is reserved for capes, cloaks, wings, gliders, jetpacks, or other movement gear. Early game can use simple cape-style bonuses; later tiers can introduce flight or air-control mechanics.

Character visual layering model:

- Base body
- Hair/face
- Helmet
- Chest
- Gloves
- Boots
- Mainhand
- Offhand
- Back
- Accessories when visually relevant

This pass only adds model/controller support. Actual anchors, sorting, offsets, and art still need Unity setup.

Planned archetypes already exist:

- Fast claws
- Heavy hammer
- Bow
- Magic staff
- Summon

## Adding Enemies

Add a new `EnemyDefinition` asset with health, damage, speed, chase range, attack range, cooldown, lunge force, attack knockback, visual scale, collider size, sprite sheet reference, placeholder color, and a `DropTableDefinition`.

Enemy ranks are modeled as:

- Normal
- Enhanced
- Elite
- MiniBoss
- Boss

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

## Inventory Crafting Ports

Crafting should move toward inventory-equipped ports instead of requiring world-placed stations. Current seed models:

- `CraftingPortDefinition`
- `CraftingPortSlot`
- `CraftingStationType`
- `InventoryCraftingController`
- Furnace Port data asset
- Cauldron Port data asset
- Training Shield offhand seed asset
- Traveler Cape back-slot seed asset

Examples:

- Furnace Port: future Copper Ore smelting from inventory.
- Cauldron Port: future potion crafting from inventory.
- Forge/Anvil Port: future weapon crafting and upgrades.

The old world furnace stays for MVP compatibility until the inventory-port flow is tested.

## Art Style Guide

Project Eclipse should converge around one original, readable side-scroller fantasy style. Keep the newer player and creature direction as the quality target, while using official MapleStory media only as a high-level reference for readability, charm, layered 2D environments, silhouette discipline, contrast hierarchy, and animation staging. Do not copy, trace, kitbash, recolor, or closely mimic MapleStory, Nexon, Terraria, Re-Logic, Black Desert, Pearl Abyss, Calamity, or any other copyrighted assets.

Rejected art approaches:

- Do not make art by combining old generated ProjectEclipse assets with pieces, colors, or texture artifacts from the approved character/monster sprites.
- Do not keep malformed swords, props, or platforms because they are already committed. Bad active art should be fully replaced by clean original drawings.
- Do not use simple triangle/pentagon programmer-art shapes as final item, equipment, platform, foreground, or background art.
- Do not generate broad batches of filler props and call that an art pass. A smaller set of good, style-matched assets is better than a large folder of crude pieces.
- Do not use texture sampling from character/monster faces or bodies as a substitute for drawing matching environment art.

Required replacement process:

- Build a visual reference board from official MapleStory screenshots/media and the current ProjectEclipse player/monster sheets.
- Extract style rules from those references: clean contour language, readable clustered shapes, soft but crisp shading, prop density, foreground/background separation, tile seams, and animation pose staging.
- Redraw ProjectEclipse assets from scratch in an original style following those rules.
- Check every candidate asset beside the player and creature sprites at gameplay scale before it becomes active art.
- For weapons, verify the silhouette reads as a real fantasy weapon: blade, guard, grip, pommel/handle, and attachment points must align and connect cleanly.

Target proportions:

- Chibi-readable bodies with clear silhouettes.
- Slightly oversized heads/hands/weapons only where it improves readability.
- Materials and equipment should read clearly at inventory-slot size and on the ground.

Outline and shading:

- Soft dark outlines, not harsh programmer-art blocks.
- Simple directional highlights and shaded undersides.
- Avoid flat single-color squares, gradients-only blobs, and generic placeholder rectangles.

Palette consistency:

- Forest materials use warm bark and leaf-adjacent browns/greens.
- Stone and iron use cool grays with distinct highlight ranges.
- Coal uses charcoal blacks with ember accents.
- Copper uses orange ore with a teal oxidation accent.
- Gold uses warm yellow highlights without becoming neon.

Icon/drop readability:

- Each material needs a matching inventory icon and world drop sprite.
- Missing icon/drop art should be treated as a development warning, not acceptable MVP presentation.
- Icons should remain legible at small UI slot sizes.

Platform kit requirements:

- Platforms that are meant to extend horizontally must be built from separate left, middle, and right pieces.
- Left pieces should only have the outside cap on their left edge; right pieces should only have the outside cap on their right edge.
- The right edge of a left piece, both edges of a middle piece, and the left edge of a right piece should use clean sheer-cut connector seams so pieces can join as `(___|_____________|______________|___)`.
- Left and right pieces must also connect cleanly without a middle tile between them.
- Middle pieces must tile repeatedly without visible end caps, large height jumps, or mismatched underside chunks.
- Middle pieces must be horizontally self-seamless: the left and right boundary columns should match so `middle + middle + middle` forms one continuous platform without a visible splice at the join.
- Keep small standalone platforms separate from connector pieces so they can have both outside caps without breaking the modular kit.

Animation expectations:

- No duplicate fake animation frames.
- Player run needs distinct leg/body motion.
- Attacks need anticipation, swing, and follow-through.
- Creature idle/move/attack/hurt/death rows need visibly different silhouettes.
- Animation quality requires Unity/visual review before claiming it is fixed.

Art cleanup performed:

- Replaced the old tiny material icons with larger stylized material sprites for Sticks, Stone, Coal, Copper Ore, Iron Ore, and Gold Ore.
- Added matching Shield, Cape, Furnace Port, and Cauldron Port icons.
- Added separate equipment art under `Assets/ProjectEclipse/Art/Equipment` for Starter Blade, Stone Cleaver, Training Shield, and Traveler Cape equipped visuals.
- Updated Starter Blade and Stone Cleaver data to use separate inventory icon sprites and in-hand equipped sprites.
- Added connector-safe platform tiles under `Assets/ProjectEclipse/Art/Platforms` for forest, stone cave, coal cave, and copper cave themes.
- Refreshed the existing world platform sprites from the connector-safe pieces without hand-editing scene YAML.
- Replaced the first connector-safe platform pass because it still read as sliced/programmer-style bars and simple shapes. The active platform kit now comes from a fresh original painted platform sheet, then is normalized into the existing left/middle/right/small/underside asset slots with chroma despill cleanup.
- Rebuilt the connectable platform pieces again as vertical sheer-cut slices from one continuous painted master per theme, so left, middle, and right share the same top/underside height and middle tiles can repeat between caps.
- Connector fixes should stay edge-only: do not paint broad vertical seam patches over the platform art. The join sides need exact one-column profiles while the surrounding platform art remains natural.
- Rebuilt the forest, stone, coal, and copper connector pieces from newly generated modular proof sheets using the approved forest direction as the style target. Coal now reads as dark charcoal rock instead of lava/magma, and copper reads as gray-brown stone with copper ore inclusions instead of solid copper.
- Kept the small standalone platform silhouettes in place for now, with only stray chroma residue pixels removed where the scan found them.
- Added 28 additional art-only modular platform kits under `Assets/ProjectEclipse/Art/Platforms`: iron, gold, platinum, silver, tin, lead, cobalt, titanium, mithril, adamantite, obsidian, crystal geode, oak forest, birch forest, pine forest, fir forest, snowy grass, snowy stone, snow, snowy ice, ice, magma, cloud, desert sandstone, swamp moss, mushroom forest, ancient ruins, and cosmic stone.
- These expanded platform kits are not wired into scenes or prefabs yet; treat them as candidate art assets until they are inspected in Unity at actual gameplay scale.
- Normalized the oak, birch, pine, and fir forest kits back to the approved base forest platform shape with grass color changes only. Avoid adding tree/root clutter or bark chunks to the walkable top surface, since creatures need to read clearly while walking across it.
- Removed the unreferenced `Art/Placeholders/solid_square.png` programmer-art placeholder.
- Reverted the later generated/collage art passes that produced janky swords, muddy texture artifacts, placeholder-like modular kits, and malformed props.
- This replacement pass used official MapleStory imagery only as high-level style direction and used the approved ProjectEclipse player/creature sprites as the in-project style anchor. Assets should remain original ProjectEclipse art, not copied, traced, recolored, kitbashed, or texture-sampled from copyrighted sources.
- Archived the rejected pre-redo player and creature PNGs under `ArtArchive/RejectedCharacterCreatureSprites_2026-06-19`, outside `Assets`, so Unity will not import duplicate stale sprites.
- Replaced the active player sheet and Tree, Stone, Coal, and Copper creature sheets at their existing `Assets/ProjectEclipse/Art/Player` and `Assets/ProjectEclipse/Art/Creatures` paths.
- Reworked the player base sheet to stay weapon-free; the attack row swings an empty hand for future weapon overlay following.
- Reworked creature rows to avoid simple still-image duplication: player run and creature move rows have alternating steps, attacks have anticipation/extension/recovery, and stable idle/move rows were normalized to a consistent baseline and height.
- Locally checked replacement sheets outside Unity for required dimensions, 96x96 frame slicing, magenta chroma residue, detached non-death sprite fragments, and contact-sheet motion readability. This is not Play Mode validation or final animation-quality approval.
- Archived the fixed-frame replacement pass under `ArtArchive/RejectedCharacterCreatureSprites_2026-06-19_FixedFrameRedo` after it was rejected for still being too tied to forced six-frame rows.
- Removed the active standalone idle PNG assets and their initial `SpriteRenderer` references. Idle frames now live only in row 0 of each full animation sheet.
- Rebuilt the active sheets as variable-frame sheets with 8 possible cells per row and blank trailing cells: player rows use Idle 4, Run 8, Jump 4, Attack 7, Hurt 3, Die 6; creature rows use Idle 4, Move 8, Attack 7, Hurt 3, Die 6.
- Updated `SpriteSheetAnimator` to detect non-empty cells per row so future animations do not need to share one fixed frame count.
- Locally checked the variable-frame sheets outside Unity for dimensions, expected non-empty frame counts, blank trailing cells, magenta chroma residue, stable idle/move/run heights, and duplicate-frame risk. This is still not Play Mode validation.

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

Newer full-game progression skeleton assets live alongside the original MVP tiers:

- `ProgressionStageDefinition`
- `WorldTierDefinition`
- `BossDefinition`
- `StageUnlockRequirement`
- `ResourceTier`
- `CraftingTier`
- `RecommendedLevel` fields on stage, world tier, boss, and unlock requirement data

Keep boss implementations lightweight until the stage loop, equipment, and drops are tested in Unity.

Boss direction: use original planet/celestial/godlike themes later, but avoid copying existing boss names, patterns, sprites, music, or effects from other games. The eventual endgame can be chaotic, high-energy, pattern-heavy, and cosmic, with final stages around escaping the solar system and later universal escape.

## Combat Input Direction

Current action model foundations:

- LMB / J: mainhand attack
- RMB: offhand action placeholder
- Q / E / R / F: class or weapon actions
- Shift: sprint and attack modifier

Warrior remains the only implemented starting class. Future class models can include Rogue, Mage/Wizard, Archer, and Gunslinger, but do not implement those until Warrior and equipment loops work.

## Unity Testing Required

This pass was made without opening Unity. Do not treat these items as visually or behaviorally validated until tested on a Unity machine:

- Unity compile/import of the new ScriptableObject types and `.asset` files.
- Play Mode movement, jump buffering, attack input timing, and enemy pursuit.
- Mainhand/offhand action routing, Shift sprint, and Q/E/R/F input hooks.
- IMGUI inventory tabs and tooltip placement.
- Equipment slot display and shift-click equip behavior.
- Inventory crafting port equip behavior and port-gated recipe rules.
- Magnet pickup radius/speed, item landing behavior, and collection timing.
- Enemy ledge/platform awareness probe distances on each current creature.
- Drop spawning/collection using the new `DropTableDefinition` references.
- Warrior class assignment on the player/GameManager, if wired in the scene.
- Separate weapon visual layer setup, sprite sorting, hand offset, and whether the player base sprite still appears to include a weapon.
- Offhand and back/cape visual anchor setup.
- Armor visual layers and original wearable art.
- New item/equipment/crafting-port icons at actual UI size and in-world scale.
- Rebuilt modular platform pieces in left/right, left/middle/right, repeated-middle, and long-platform arrangements at actual import pixels-per-unit, sorting, collision, and gameplay scale.
- Expanded ore, forest, winter, elemental, and biome platform kits at actual import pixels-per-unit, sorting, collision, and gameplay scale.
- Replaced player and creature sheets at actual import pixels-per-unit, sorting, alpha edges, runtime row slicing, and gameplay scale.
- Variable frame-count detection from transparent trailing cells on the player and creature sheets.
- Player run, jump, weaponless attack, hurt, and death timing in Play Mode.
- Tree, Stone, Coal, and Copper idle/move/attack/hurt/death timing in Play Mode.
- Animation readability and combat feel.

## Animation Setup

`AnimationAssetGenerator` creates placeholder Unity Animator Controllers and AnimationClips for Player and Enemy objects. The generated state names are intentionally stable:

- Idle
- Move
- Attack
- Hurt
- Die

Runtime code drives Animator parameters named `IsMoving`, `IsGrounded`, `Attack`, `Hurt`, and `Die`. Final art can replace the generated clips without changing gameplay code.

Player and creature sheets use a reusable code-driven setup:

- `SpriteSheetAnimator` slices 96x96 frames from each sheet.
- Creature rows are ordered Idle, Move, Attack, Hurt, Die.
- Player rows are ordered Idle, Run, Jump, Attack, Hurt, Die.
- Creature frame counts currently resolve from non-empty sheet cells: Idle 4, Move 8, Attack 7, Hurt 3, Die 6.
- Player frame counts currently resolve from non-empty sheet cells: Idle 4, Run 8, Jump 4, Attack 7, Hurt 3, Die 6.
- Keep unused trailing cells transparent; `SpriteSheetAnimator` auto-detects the final non-empty 96x96 cell in each row.
- `VisualStateAnimator` forwards movement and trigger state changes to the sheet animator.
- Enemy definition assets assign sprite sheet references, visual scale, collider size, and behavior tuning.

The current sheets are original homemade-style game art. They are meant to establish readable silhouettes, animation timing, and charm before final production polish. Local image QA checked dimensions, chroma cleanup, stable baselines, and obvious duplicate-frame problems, but Unity still needs to validate import settings, runtime timing, and visual scale.

## Known TODOs

- Keep important MVP objects visible and editable in `ProjectEclipse_MVP.unity`; avoid returning to invisible runtime world generation.
- Add a child weapon anchor/renderer to the player prefab or scene object and assign it to `EquipmentController`.
- Replace placeholder weapon visual sprites with original in-hand weapon art.
- Add true weapon behavior modules for ranged, magic, summon, and fast melee styles.
- Add armor and upgrade stat effects.
- Add proper UI Toolkit or UGUI UI after gameplay loops settle.
- Add respawn and checkpoint handling.
- Add boss gates and tier unlock persistence.
- Add automated play-mode tests once Unity is installed in the development environment.

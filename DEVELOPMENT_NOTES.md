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
- `Assets/ProjectEclipse/Art/Player`: homemade player idle/run/jump/attack/hurt/death sheet and edit-time idle sprite.
- `Assets/ProjectEclipse/Art/Creatures`: homemade creature sprite sheets and edit-time idle sprites.
- `Assets/ProjectEclipse/Art/Items`: homemade inventory/ground icons for materials, equipment seeds, and crafting ports.
- `Assets/ProjectEclipse/Art/Equipment`: dedicated equipment inventory icons and held/equipped visual sprites.
- `Assets/ProjectEclipse/Art/Platforms`: modular platform kits, split by Forest, CaveStone, CaveCoal, and CaveCopper.
- `Assets/ProjectEclipse/Art/Foreground`: walk-behind decorative props, split by Forest, CaveStone, CaveCoal, and CaveCopper.
- `Assets/ProjectEclipse/Art/Backgrounds`: distant silhouettes, cave wall patches, and area mood pieces.
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

Project Eclipse should converge around one original, readable side-scroller fantasy style. The current player, Tree Creature, Stone Creature, Coal Creature, and Copper Creature sheets are the style anchor and should not be downgraded to match rougher older assets. The broad target is charming, crisp, readable MapleStory-esque fantasy side-scroller art, but all assets must remain original and must not copy MapleStory, Nexon, Terraria, Re-Logic, Black Desert, Pearl Abyss, Calamity, or any other copyrighted game.

Current art audit:

- Keep: the player and creature designs as style anchors, with future polish focused on cleaner edges and stronger authored animation.
- Clean Up: active item/equipment icons, player/creature animation sheets, and old one-off world platforms/backdrops.
- Replace/Add: modular platform pieces, foreground props, background dressing, and any active item that borrowed unrelated material art.
- Quarantine rule: debug-only art should live under a clearly named `DebugOnly` folder and must not be referenced by active MVP item/equipment/world data.

Art production process:

- New MVP art should be checked beside `player_adventurer_idle.png` and the current creature idle sprites before it is treated as active.
- Material items and biome props should sample palette/texture language from their source creature line: Tree for Sticks/forest, Stone for cave stone, Coal for coal seams, Copper for copper ore.
- Avoid broad procedural filler passes that create many assets without a style-anchor comparison sheet.
- Do not sample creature face/eye regions for environmental rock or ore textures; props should not accidentally inherit creature facial details.
- Contact sheets are useful for review, but they are temporary validation artifacts and should not be committed.

Target proportions:

- Chibi-readable bodies with clear silhouettes.
- Slightly oversized heads/hands/weapons only where it improves readability.
- Materials and equipment should read clearly at inventory-slot size and on the ground.

Outline and shading:

- Soft dark outlines, not harsh programmer-art blocks.
- Controlled dark outlines with simple readable cel-shaded highlights.
- Avoid blurry painterly smears, noisy texture, and generic AI-smudged shapes.
- Use small cast shadows only where they help inventory/drop readability.
- Clean silhouettes first; small internal detail should support the shape instead of making the asset noisy.
- Avoid flat single-color squares, gradients-only blobs, and generic placeholder rectangles.

Palette consistency:

- Forest materials use warm bark and leaf-adjacent browns/greens.
- Stone and iron use cool grays with distinct highlight ranges.
- Coal uses charcoal blacks with ember accents.
- Copper uses orange ore with a teal oxidation accent.
- Gold uses warm yellow highlights without becoming neon.

Icon/drop readability:

- Each material needs a strong inventory icon that can also read as its ground drop sprite.
- Equipment should normally need only an inventory icon and, when worn/held, an equipped visual sprite.
- Do not create extra item variants unless there is a clear gameplay or readability reason.
- Crafting ports are compact magical/tool-port items, not full world stations, unless a separate world prop asset is explicitly being authored.
- Missing icon/drop art should be treated as a development warning, not acceptable MVP presentation.
- Icons should remain legible at small UI slot sizes.

Platform/world rules:

- Platforms need a clean readable top edge because that edge communicates the collider.
- Modular platform sets should include left, middle, right, small standalone, and underside pieces before making more one-off long platforms.
- Forest platforms should read as grass/wood/earth with roots and undergrowth.
- Cave Stone is the structural base for cave progression.
- Coal platforms should remain cave stone with coal seams, dark deposits, soot, and occasional ember accents. Do not make the whole platform pure coal.
- Copper platforms should remain cave stone with copper ore veins, deposits, and occasional teal oxidation accents. Do not make the whole platform pure copper.
- Keep lava/magma styling reserved for a future fire or magma biome, not Coal or Copper.
- Tiling/repeating surfaces should avoid loud detail that fights player, enemy, or drop readability.
- World art may be simple, but it should not read as crude rectangles or debug blocks.

Foreground/background rules:

- Foreground props are non-blocking walk-behind visuals for depth and clutter.
- Background props should stay lower contrast than gameplay objects and communicate wall detail, distant silhouettes, or mood.
- Forest progression should feel denser as the player goes deeper: roots, vines, logs, undergrowth, trunks, and canopy silhouettes.
- Cave progression should feel deeper underground: boulders, protrusions, stalactites, stalagmites, cracked stone, supports, and ore seams.
- Coal, Copper, future Iron, and future Gold should feel like increasingly rich cave layers, not floors made entirely of the resource.

Animation expectations:

- No duplicate fake animation frames.
- Player run needs distinct leg/body motion, not whole-body stretch/squeeze.
- Attacks need anticipation, strike, and recovery/follow-through poses.
- Creature idle/move/attack/hurt/death rows need visibly different silhouettes.
- Current sheets have an intermediate cutout-style staging pass, but final animation quality still requires hand-authored review and Unity playback inspection before calling it fixed.

Art cleanup performed:

- Reworked material icons for Sticks, Stone, Coal, Copper Ore, Iron Ore, and Gold Ore using palette and texture cues from the current creature art.
- Removed the redundant `Art/Drops` direction; active item data now reuses the inventory icon as the ground sprite for normal drops.
- Reworked Starter Blade from the player's existing weapon language and Stone Cleaver from non-face stone texture regions so it reads as a fantasy stone weapon.
- Reworked Shield, Cape, Furnace Port, Cauldron Port, Basic Furnace, and Copper Whetstone to reduce borrowed-material-icon presentation.
- Rebuilt modular platform kits for Forest, CaveStone, CaveCoal, and CaveCopper with source-creature palette/texture cues.
- Rebuilt foreground decorative kits for Forest, CaveStone, CaveCoal, and CaveCopper, removing accidental creature-eye artifacts from cave rock props.
- Added background dressing kits for Forest, CaveStone, CaveCoal, and CaveCopper.
- Rebuilt active one-off `Art/World` platform PNGs from the corrected modular platform pieces while preserving their asset paths for existing scene references.
- Rebuilt player and creature sprite sheets with clearer staged idle/move/attack/hurt/death poses to reduce the copy-pasted-still feeling.
- Reworked Forest, Stone, Coal, and Copper platform/backdrop art so the top surfaces are clearer and more area-specific.
- Removed the unreferenced `Art/Placeholders/solid_square.png` programmer-art placeholder.

Assets that still need visual review in Unity:

- Player and creature sprite sheets were audited and adjusted, but animation frame quality still needs Unity review and a future hand-authored animator pass before calling it final.
- Equipment visual offsets, rotations, sorting layers, and hand/back/offhand anchors need Unity inspection.
- Platform scale, repetition, and collider readability need scene inspection.
- Item icons need inspection at actual inventory slot size and in-world drop scale, since normal item drops now reuse the icon sprite.
- Modular platform tiles need Unity scene assembly testing for seams, pivot expectations, sorting, and collider placement.
- Foreground and background props need sorting-layer, parallax, and camera-scale inspection.
- The old world furnace sprite remains for MVP scene compatibility and should receive a future dedicated world-prop pass if the world station stays.

Future professional/manual art pass candidates:

- Player wearables and armor layers.
- Final authored attack/hurt/death animation polish.
- Boss, mini-boss, and progression gate art.
- Full biome background sets beyond compact platform/backdrop MVP assets.

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
- New material drop sprites, platform art, and equipment visual sprites at actual camera zoom.
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
- Creature frame counts are Idle 4, Move 6, Attack 6, Hurt 2, Die 6.
- Player frame counts are Idle 4, Run 6, Jump 2, Attack 6, Hurt 2, Die 6.
- `VisualStateAnimator` forwards movement and trigger state changes to the sheet animator.
- Enemy definition assets assign sprite sheet references, visual scale, collider size, and behavior tuning.

The current sheets are original homemade-style game art. They are meant to establish readable silhouettes, animation timing, and charm before final production polish.

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

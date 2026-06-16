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
- Enemy ledge/platform awareness probe distances on each current creature.
- Drop spawning/collection using the new `DropTableDefinition` references.
- Warrior class assignment on the player/GameManager, if wired in the scene.
- Separate weapon visual layer setup, sprite sorting, hand offset, and whether the player base sprite still appears to include a weapon.
- Offhand and back/cape visual anchor setup.
- Armor visual layers and original wearable art.
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

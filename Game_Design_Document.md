# Game Design Document: Conquer The Plane

**Based on the Nuclino Game Design Document Format**

---

## 1. Executive Summary

### 1.1 Game Concept
**Conquer The Plane** is a 3D tactical, physics-based arena survival game featuring a persistent world exploration mechanic. Players control a robotic sphere navigating through interconnected arena maps (planes) using dynamic wormholes. The goal is to survive waves of intelligent enemies while managing powerful tactical skills (such as dashes, shockwaves, and energy bolts).

### 1.2 Genre
Tactical Arena Survival / Physics-based Action

### 1.3 Target Audience
Players who enjoy fast-paced reflex challenges, tactical positioning, and high-replayability survival loops. The game appeals to fans of both physics-roller games (like *Marble Madness* or *Super Monkey Ball*) and tactical arena shooters (like *Valorant*), blending movement mechanics with strategic cooldown-based combat.

### 1.4 Project Scope
An indie-developed Unity 3D project. The current scope includes:
- A playable core loop with 4 distinct interconnected maps plus a Bootstrap scene.
- A functional persistent singleton manager tracking player state and enemies across scenes.
- Tactical skills with cooldown UI.
- Intelligent enemy AI featuring Patrol and Chase states.
- High-quality visual processing using Unity's Universal Render Pipeline (URP).

---

## 2. Gameplay

### 2.1 Objectives
The primary objective is to **survive as long as possible** while exploring the environment. The player must:
- Avoid or neutralize the Black Cubes (Enemies) that drain health over time upon contact.
- Constantly relocate via Magenta Sphere Wormholes to manage enemy aggro.
- Master movement and skill cooldowns to maintain control over the danger zones.

### 2.2 Core Gameplay Loop
1. **Spawn & Explore:** The player spawns in a map. They use WASD/Arrows to roll their Robot Sphere. 
2. **Engage & Survive:** Enemies aggressively chase the player. The player uses tactical skills (e.g., Dash Strike to knock back enemies and traverse quickly) to evade.
3. **Teleport & Reset:** The player locates a "Wormhole" (which shifts its location dynamically every 5-60 seconds) to teleport to another map.
4. **Persist:** Because enemy counts and states are preserved across maps, teleporting helps balance danger, turning survival into a dynamic puzzle.
5. **Auto-Regenerate:** By avoiding damage for 2 seconds, the player's HP begins to regenerate, incentivizing hit-and-run tactical movement.

### 2.3 UI / UX Design
- **Top-Left (Ranking Dashboard):** Displays real-time danger levels of each map, sorting maps by the number of active enemies. Clean and minimalist.
- **Top-Right (Health Bar):** A visual indicator of the player's 100 HP.
- **Bottom Center (Cooldowns):** Icons tracking cooldown states for the player's tactical skills (Dash, Energy Bolt, Spike Trap, Shockwave).
- **Game Over & Victory UI:** Displays standard interactive menus seamlessly managing run resets or exits.

---

## 3. Mechanics

### 3.1 Combat & Skills
The player utilizes cooldown-based skills to survive:
- **Dash Strike (`DashStrikeSkill.cs`):** 
  - *Mechanic:* An instant impulse force driving the player forward. Leaves a glowing orange trail.
  - *Combat Use:* Pushes back any enemies within a 1.5-unit radius, dealing impact force and granting temporary breathing room.
- **Energy Bolt (`EnergyBoltSkill.cs`):** A fast-moving tactical projectile used for ranged engagements.
- **Shockwave (`ShockwaveSkill.cs`):** An Area-of-Effect (AoE) blast that knocks enemies back, useful when swarmed.
- **Spike Trap (`SpikeSkill.cs`):** A defensive deployable that punishes enemies following the player too closely.

### 3.2 Enemy AI (`EnemyAI.cs`, `EnemyManager.cs`)
- **State Machine:** Enemies transition between **Patrol** (roaming randomly based on navmesh or physics constraints) and **Chase** (relentlessly tracking the player when in line-of-sight/aggro radius).
- **Persistent Threat:** Enemies are globally tracked. If Map 2 has 5 enemies, those 5 enemies remain in Map 2 even if the player teleports to Map 3. Exactly 9 enemies are distributed across the maps on initialization.

### 3.3 Movement & Physics Rules
- Movement relies purely on Unity's Rigidbody `AddForce` and `linearVelocity`.
- **Wormhole Persistence:** When entering a wormhole, the player's current physical momentum (velocity) and spatial position relative to the wormhole are preserved upon exiting the destination wormhole, allowing for seamless momentum-based traversal.

---

## 4. Game Elements

### 4.1 Worldbuilding & Theme
The aesthetic is heavily inspired by modern tactical shooters like *Valorant*. The world uses clean lines, dramatic lighting, and distinct color blocking:
- The arenas feel like high-tech holographic training facilities or minimalist brutalist structures.
- Important gameplay elements are heavily color-coded:
  - **Player:** High-tech Robot Sphere.
  - **Wormholes:** Bright Magenta Spheres (high contrast, easily spotted).
  - **Enemies:** Ominous Black Cubes.
  - **Abilities:** Vivid emissive effects (e.g., Bright Orange for Dash Strike).

### 4.2 Characters
- **The Protagonist (RobotSphere):** A rigged and animated robotic sphere. It embodies fluid rolling dynamics combined with tactical capabilities.
- **The Antagonists (Black Cubes):** Faceless, relentless geometry. They represent a pure, systemic threat rather than narrative-driven villains.

### 4.3 Level Design
- **Map 1, Map 2, Map 3, Map 4:** 
  - Each map represents a distinct tactical arena generated using ProBuilder.
  - They feature structured plant sites, strategic cover, elevated vantage points, and chokepoints.
- **Bootstrap Scene:** Initializes the core singletons (`EnemyManager`, `PersistentCamera`) before pushing the player into the physical arenas.

---

## 5. Assets

### 5.1 Art & Textures
- **Materials:** Tactical color palettes using `Valorant_DarkGrey.mat` and `Valorant_DeepBlue.mat`.
- **Textures:** High-resolution realistic ground layers (Seamless Concrete Floor, Plaster Wall, Wood Crates) mixed with stylized, glowing visual effects.
- **Lighting/Shaders:** Extensive use of URP post-processing, emission materials (Dash Trail), and custom shaders (`GlassRefraction.shader`).

### 5.2 3D Models
- Customized ProBuilder geometry allowing for rapid, physics-accurate level iterations.
- Animated `RobotSphere.fbx` featuring Open/Close/Roll/Idle animations for player state feedback.

### 5.3 Audio (TBD)
- Currently supported by `AudioManager.asset`, planned to include high-impact electronic/synth soundtracks matching the futuristic tactical aesthetic, alongside clear, distinct audio cues for skill cooldowns and wormhole relocations.

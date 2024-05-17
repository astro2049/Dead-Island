# Dead Island

> Scenario 2: Zombie Apocalypse
>
> Environment: Ocean Islands

The scene is set on some connected ocean islands. A squad of survivors begins on one island and must navigate through zones populated with wandering zombies to reach a safe zone.

*Playthrough Video* https://youtu.be/tfhC5Rpoukg

## 0. Scene Overview (Unity)

- *Canvas*. Where the buttons at.
- *Map*. A container for tiles, ocean background etc.
- *Game Manager*. Holds *LevelGenerator*, *GameManager*, and the *NavMeshSurface*.
- *UI*. Legends and indicators.
- *Music Player*. Plays the background music. *I added music only because it makes the gunfire sounds a lot less 'out of nowhere'...*

## 1. Map Generation

**Map Overview**

- Playable area is a 64 * 64 tile grid, with each tile = 1.5 * 1.5m.
- Three types of tiles: *Ocean* (won't be rendered to scene), *Beach*, and *Forest*.

- Ocean islands generated with a binary space trees (BSP), a cellular automata (CA), and a post processor (I'll call it PP).


**Generation Overview**

The map starts out with all *Ocean* tiles `public TileType[,] grid;`.

> Terminology: As we know the partition unit in BSP is "room", while in code "room" is `class Island`, so I might refer to "room" as "island" as well.

1. **Partitioning.** BSP partitions the map into rooms. Leaf rooms are recorded in the process.
2. **Generate individual islands.** For every leaf room,
   1. CA evolves the island landscape, starting with an initial 3 * 3 *Forest* tile square.
   2. PP generates the coastlines, by turning some *Forest* tiles on land edges (adjacent to the ocean) into *Beach* tiles.
3. **Connect the islands.** PP connects the islands, creating pathways between islands by writing the tiles along a direct line connecting the centers of each island 3 tiles wide, all to *Beach/Forest* tiles.
4. Render the tile grid to scene.
5. Build the nav mesh on the map.
6. Place survivors and zombies.

### 1.0 Generator Design

So, as mentioned before, there is a

- Binary space trees (BSP), `class BinarySpaceTrees`
  - And room/island, `class Island`
- Cellular automata (CA), `class CellularAutomata`
- Post processor (PP), `class PostProcessor`
- and, a level generator, which organizes the generation process, `class LevelGenerator`

#### 1.0.0 Level Generator

`public void Generate()` fires up the generation.

#### 1.0.1 Binary Space Trees (BSP)

**Splits the map into rooms.**

`public void Partition(Island island)`

Recursively partition the rooms.

- A leaf island is formed when *width <= maxWidth || height <= maxHeight*.
- If both width and height are bigger than max, there's a 50/50 chance of splitting horizontally/vertically.

`private void SplitHorizontally(Island island)`

`private void SplitVertically(Island island)`

Split an island horizontally/vertically. Initial split at the central line, and there's a *m_splitWiggle = 12*, which shifts the divider a bit.

#### 1.0.2 Cellular Automata

**Creates the actual landscape of an island.**

`private void InitializeIsland(Island island)`

Set the center 3 * 3 tiles of an island to *Forest* tiles.

`public void GenerateIsland(Island island)`

`private TileType GetNewState(int x, int y)`

Evolve the initial tiles square: gradually expand the island landscape.

I made up the evolution rule: What's the new state for an *Ocean* cell? - If it has >= 3 land Moore neighbors, it has a 50% chance of becoming a *Forest* cell.

#### 1.0.3 Post Processor

**Generates coastlines for an island, and connects the islands.**

`public void GenerateCoastlines(Island island)`

Given an island, generate its coastlines, by transforming some of the *Forest* tiles adjacent (or close) to water to *Beach* (Again, the algorithm is made-up, by me).

`public void ConnectIslands(Island island)`

Given the root island, connect the islands' centers. The direction is top-down, so we're connecting the largest rooms first and the leaf rooms last, as we are doing a depth-first search just like `void BinarySpaceTrees::Partition(Island island)`.

## 2. Agents

**Agents Overview**

Agents,

- There are *survivors* and *zombies*
- have perceptions: *visual*, and *aural*
- act with behavior trees, using NPBehave
- all steering through *NavMeshAgent*
- in code, both derived from `class IndividualAgent`
- *Survivors* are in a squad, manager is `class SquadAgent`
- Prefabs next to scene (under Assets/Main.unity)

### 2.0 Individuals

#### 2.0.0 Perceptions,  `class AIPerception`

**Visual**

- Forward sight
- angle: 180 degrees
- distance: 10m
- Collision present: A collision rectangle (square)

**Aural**

- All-directional hearing
- angle: 360 degrees
- radius = 15m (*zombie*s) / 7m (*survivors*)
- Collision present: A collision sphere (circle)

#### 2.0.1 Individual Agents

**Individual Agent, `class IndividualAgent`**

`m_targets, and m_target`

An individual always maintains a list of enemies observed, in `m_targets`, and selects the closest enemy, as `m_target`.

(`m_target` won't be updated unless `m_target` cannot be sensed anymore/is dead. So while this is cool for *zombies*, it's indeed not that perfect for *survivors*)

**Zombie, `class ZombieAI : IndividualAgent`**

A *zombie*

- Roams at 2m/s. Picks a random location within a radius, goes there, waits, and repeats.
- When sees a survivor, chases at 8m/s.
  - If the survivor is within attack range = 3m, attacks.

![2](https://github.com/astro2049/Dead-Island/assets/45759373/38b15b33-b3a2-46d8-838c-a610fb7ff796)

**Survivor, `class SurvivorAI : IndividualAgent`**

A *survivor*

- Navigates at 5m/s if everything's fine
  - if is *Leader*, to the safe zone
  - or else, follow the leader
- If *senses* a zombie,
  - turn to face the zombie with angular speed = 360 deg/s
  - if is facing the target (<= 10 deg) and rifle is loaded (currentFireCooldown <= 0s), it shoots (cooldown = 1.2s)
  - else, and if the zombie is too close (<= 7m), back up to create some distance

### 2.1 The Squad

![1](https://github.com/astro2049/Dead-Island/assets/45759373/7d41d1fa-1508-462f-b859-3f04b6e08690)

**Squad Overview**

- *Survivors* are organized in a squad.
- All *survivors* belong to a squad.
- Squad assigns roles.
- Individual *survivors* perform those roles.
- Roles are behaviors.
- There are two roles: *Leader*, and *Follower*.
- *Leader*'s direction is squad's direction.
- Squad advances in formation.
- *Leader* navigates to the safe zone, while *followers* follow and maintain the formation.
- A squad member engages *zombies* upon spotting any and returns to formation if all targets are cleared.

#### More on Squad Coordination

**New Leader**

If the *leader* is dead, the last *follower* becomes the *leader* (if there are still *followers*).

**Killing a Zombie**

When a squad member kills a *zombie*, it is notified across the squad, other squad members will remove the *zombie* from `m_targets`, saving shots.

## References

**Cellular Automata**

1. White Box Dev (2020) 'Cellular Automata | Procedural Generation | Game Development Tutorial', *YouTube*, 19 September. Available at: https://www.youtube.com/watch?v=slTEz6555Ts (Accessed 27 March 2024).

**Binary Space Trees/Partitioning**

2. 'Basic BSP Dungeon generation' (2023) *RogueBasin*. Available at: https://www.roguebasin.com/index.php/Basic_BSP_Dungeon_generation (Accessed 31 March 2024).

3. Sunny Valley Studio (2020) 'Binary Space Partitioning Algorithm - P13 - Unity Procedural Generation of a 2D Dungeon', *YouTube*, 19 December. Available at: https://www.youtube.com/watch?v=nbi88hY9hcw (Accessed 31 March 2024).

4. Uribe, G. (2019) 'Dungeon Generation using Binary Space Trees', *Medium*, 2 June. Available at: https://medium.com/@guribemontero/dungeon-generation-using-binary-space-trees-47d4a668e2d0 (Accessed 31 March 2024).

**Ocean Texture**

5. 'Shallow Ocean Terrain Texture' (2019) *Don't Starve Wiki*. Available at: https://dontstarve.fandom.com/wiki/Ocean?file=Shallow_Ocean_Terrain_Texture.png#Shipwrecked (Accessed 1 April 2024).

**Unity Packages**

6. Nilspferd (2019) *NPBehave - Event Driven Code Based Behaviour Trees, v. 1.2.3*. Available at: https://assetstore.unity.com/packages/tools/behavior-ai/npbehave-event-driven-code-based-behaviour-trees-75884 (Accessed 30 Jan 2024).

7. Alstra Infinite (2021) *Planes & Choppers - PolyPack, v. 1.2*. Available at: https://assetstore.unity.com/packages/3d/vehicles/air/planes-choppers-polypack-194946 (Accessed 9 April 2024).

8. Sound Earth Game Audio (2023) *Post Apocalypse Guns Demo, v. 1.1.1*. Available at: https://assetstore.unity.com/packages/audio/sound-fx/weapons/post-apocalypse-guns-demo-33515 (Accessed 11 April 2024).

9. Muz Station Productions (2017) *FREE Energy Hard Rock Music Pack, v. 1.2*. Available at: https://assetstore.unity.com/packages/audio/music/rock/free-energy-hard-rock-music-pack-54012 (Accessed 11 April 2024).

**Squad Coordination**

10. Karlsson, T. (2022) 'Squad Coordination in "Days Gone"', *YouTube*, 7 February. Available at: https://www.youtube.com/watch?v=7TQ-WS3MPlE (Accessed: 20 March 2024).

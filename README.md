# Dead Island

> Scenario 2: Zombie Apocalypse
>
> Environment: Ocean Islands

The scene is set on a group of connected ocean islands. Survivors start on one island and will try to navigate through zones populated with wandering zombies to reach the safe zone (basically, the goal island) and win the game.

*Video* [https://youtu.be/RyaxD4ZxN04](https://youtu.be/RyaxD4ZxN04)

## 0. Map Generation

**Map Overview**

- Playable area is a 64 * 64 tile grid, with each tile = 1.5 * 1.5m.
- Three types of tiles: *Ocean* (won't be rendered to scene), *Beach*, and *Forest*.

- Ocean islands generated with binary space partitioning (BSP), cellular automata (CA), and a post processor (I'll call it PP).


**Generation Overview**

The map starts out with all *Ocean* tiles.

> Terminology: I will refer to the "room" concept in BSP as "island" in our scenario here, as it fits our scene well.

1. **Partitioning.** BSP partitions the map into islands, recording leaf islands in the process.
2. **Generate individual islands.** For every leaf island,
   1. CA evolves the island landscape around an initial 3 * 3 *Forest* tile square.
   2. PP generates coastlines, turning some *Forest* tiles on land edges (adjacent to the ocean) into *Beach* tiles.
3. **Connect the islands.** PP connects the islands, creating pathways between islands by writing the tiles along a direct line connecting the centers of each island 3 tiles wide, to land tiles.
4. Render the tile grid to scene.
5. Build the nav mesh on the map.
6. Place survivors and zombies.

### 0.0 Generator Design

So, as mentioned before, there is a

- Binary space trees (BSP), *class BinarySpaceTrees*. With room/island, *class Island*
- Cellular automata (CA), *class CellularAutomata*
- Post processor (PP), *class PostProcessor*
- and, a level generator, which organizes the generation process, *class LevelGenerator*

#### 0.0.0 *class LevelGenerator*

`public void Generate()` generates the tile map.

#### 0.0.1 *class BinarySpaceTrees*

**Splits the map into rooms.**

`public void Partition(Island island)`

Recursively partition the islands.

- A leaf island is formed when *width <= maxWidth || height <= maxHeight*.
- If both width and height are bigger than max, there's a 50/50 chance of splitting horizontally/vertically.

`private void SplitHorizontally(Island island)`

`private void SplitVertically(Island island)`

Split an island horizontally/vertically. Initial split at the central line, and there's a *m_splitWiggle = 8*, which shifts the divider a bit.

#### 0.0.2 *class CellularAutomata*

**Creates actual landscape of an island.**

`private void InitializeIsland(Island island)`

Set the center 3 * 3 tiles of an island to *Forest* tiles.

`public void GenerateIsland(Island island)`

`private TileType GetNewState(int x, int y)`

Evolve the initial tiles square: gradually expand the island landscape.

I made up the evolution rule: What's the new state for an *Ocean* cell? - If it has >= 3 land Moore neighbors, it has a 50% chance of becoming a *Forest* cell.

#### 0.0.3 *class PostProcessor*

**Generates coastlines for an island, or connect the islands.**

`public void GenerateCoastlines(Island island)`

Given an island, generate its coastlines, by transforming some of the *Forest* tiles adjacent to water to *Beach*.

`public void ConnectIslands(Island island)`

Given the root island, connect the islands' (rooms) centers. The direction is top-down, so we're connecting the largest rooms first and the leaf rooms last, as we are doing a depth-first search just like *void BinarySpaceTrees::Partition(Island island)*.

## 1. Agents

**Agents Overview**

Agents,

- There are *survivors* and *zombies*.
- have perceptions: *visual*, and *aural*.
- act with behavior trees using NPBehave.
- all steering through NavMeshAgent.
- in code, both derived from *class IndividualAgent*.
- Prefabs next to scene (under Assets/Main.unity)

### 1.0 Perceptions, *class AIPerception*

#### 1.0.0 Visual

Forward sight, angle = 180 degrees, distance = 10m.

Collision presents: A collision rectangle (square), in front of agent.

#### 1.0.1 Aural

360 degrees, radius = 3m (*Zombie*s) / 7m (Survivors)

Collision presents: A collision sphere (circle), centered at agent.

***Now, behaviors...*** (behavior trees below)

### 1.1 Behaviors

#### 1.1.0 Individual Agent, class *IndividualAgent*

`m_targets, and m_target`

An agent always maintains a list of enemy agents observed, in `m_targets`, and the closest enemy, as `m_target`.

(`m_target` won't be updated unless `m_target` cannot be sensed anymore/is dead. While this is cool for *zombies*, it's indeed not that perfect for *survivors*)

#### 1.1.1 Zombie, *class ZombieAI : IndividualAgent*

A *zombie*

- Roams with speed = 2m/s, where, it picks a random location within a radius, goes there, waits, and repeats.
- When sees a survivor, it chases with speed = 7m/s.
- When the survivor is within attack range = 3m, it attacks.

![BTs](https://github.com/astro2049/Dead-Island/assets/45759373/ffcd397c-caa1-4bf0-af7e-89710bed84e2)

#### 1.1.2 Survivor, *class SurvivorAI : IndividualAgent*

A *survivor*

- Navigates with speed = 5m/s, to the safe zone if everything's fine
- If a zombie is nearby:
  - will always turn with angular speed = 360 deg/s, to face the zombie
  - if it's facing the target (<= 10 deg) and the rifle is loaded (currentFireCooldown <= 0s), it shoots (cooldown = 1s)
  - else, if the zombie is too close (<= 7m), back up to create some distance

### 2. The Scene

The ***Game Manager*** game object has the *LevelGenerator*, *GameManager* scripts, and the *NavMeshSurface*.

*I added the music only because it makes the gunfire sounds less startling...

## References

**Cellular Automata**

1. White Box Dev (2020) 'Cellular Automata | Procedural Generation | Game Development Tutorial', *YouTube*. Available at: https://www.youtube.com/watch?v=slTEz6555Ts [Accessed 27 Mar 2024].

**Binary Space Trees/Partitioning**

2. 'Basic BSP Dungeon generation' (n.d.) *RogueBasin*. Available at: https://www.roguebasin.com/index.php/Basic_BSP_Dungeon_generation [Accessed 31 March 2024].

3. Sunny Valley Studio (2020) 'Binary Space Partitioning Algorithm - P13 - Unity Procedural Generation of a 2D Dungeon', *YouTube*. Available at: https://www.youtube.com/watch?v=nbi88hY9hcw&list=PLcRSafycjWFenI87z7uZHFv6cUG2Tzu9v&index=14 [Accessed 31 March 2024].

4. Uribe, G. (2019) 'Dungeon Generation using Binary Space Trees'. *Medium*. Available at: https://medium.com/@guribemontero/dungeon-generation-using-binary-space-trees-47d4a668e2d0 [Accessed 31 March 2024].

**Ocean Texture**

5. 'Shallow Ocean Terrain Texture' (2019) *Don't Starve Wiki*. Available at: https://dontstarve.fandom.com/wiki/Ocean?file=Shallow_Ocean_Terrain_Texture.png#Shipwrecked [Accessed 1 April 2024].

**Unity Packages**

6. Nilspferd (2019) *NPBehave - Event Driven Code Based Behaviour Trees*, v. 1.2.3. Available at: https://assetstore.unity.com/packages/tools/behavior-ai/npbehave-event-driven-code-based-behaviour-trees-75884 [Accessed 30 Jan 2024].

7. Alstra Infinite (2021) *Planes & Choppers - PolyPack*, v. 1.2. Available at: https://assetstore.unity.com/packages/3d/vehicles/air/planes-choppers-polypack-194946 [Accessed 9 April 2024].

8. Sound Earth Game Audio (2023) *Post Apocalypse Guns Demo*, v. 1.1.1. Available at: https://assetstore.unity.com/packages/audio/sound-fx/weapons/post-apocalypse-guns-demo-33515 [Accessed 11 April 2024].

9. Muz Station Productions (2017) *FREE Energy Hard Rock Music Pack*, v. 1.2. Available at: https://assetstore.unity.com/packages/audio/music/rock/free-energy-hard-rock-music-pack-54012 [Accessed 11 April 2024].

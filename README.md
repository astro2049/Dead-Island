# Dead Island

> Scenario 2: Zombie Apocalypse
>
> Environment: Ocean Islands

The scene is set on a group of connected ocean islands. Survivors start on one island and will try to navigate through zones populated with wandering zombies to reach the safe zone (basically, the goal island) and win the game.

## 0. Map Generation

### 0.0 The Map

64 * 64 tile map, with each tile = 1.5 * 1.5m.

Island has two types of tiles: *Beach*, and *Forest*.

Generated with binary space partitioning (BSP), cellular automata (CA), and a post processor (I'll call it PP).

**Generation Overview**

The map starts out with an ocean (which is, nothing).

> *Terminology*: I will refer to the "room" concept in BSP as "island" in our scenario here, as it fits our scene well.

1. BSP partitions the map into islands.
2. When a leaf island is determined
   1. CA evolves the land tiles from an initial 3 * 3 *Forest* tile square.
   2. PP generates the island's coastline, turning some tiles on the land edge into *Beach* tiles.
3. PP connects the islands, connecting their center 3 * 3 tiles in a line all to *Beach* tiles, forming straight land bridges.
4. Build the nav mesh on the map.
5. Place survivors and zombies.

### 0.1 Generator Design



## 1. Agents

There are survivors and zombies.

Agents are controlled by behavior trees using NPBehave.



### 1.1 Zombie



### 1.2 Survivor



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

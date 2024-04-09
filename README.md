# Dead Island

> Scenario 2: Zombie Apocalypse
>
> Environment: Ocean Islands

The scene is set on a group of connected ocean islands. Survivors start on one island and will try to navigate through zones populated with wandering zombies to reach the safe zone (basically, the goal island) and win the game.

## 0. Map Generation

### 0. 0 The Map

64 * 64 tile map, with each tile = 1.5 * 1.5m.

Island has two types of tiles: *Beach*, and *Forest*.

Generated with binary space partitioning (BSP) and cellular automata (CA), and a post processor (I'll call it PP).

#### Generation Overview

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

There are *survivor* and *zombie*.

Agents are controlled by behavior trees using NPBehave.



### 1.1 Survivor



### 1.2 Zombie



## References

Cellular Automata

# Cellular Automata | Procedural Generation | Game Development Tutorial



[![img](https://yt3.ggpht.com/1zP2ilLZW0rWnM_80RWGQ57XuJdpCYnedYJ32DElDU_DkwX7hVXQMX3tZToL5te8189920LH=s48-c-k-c0x00ffffff-no-rj)](https://www.youtube.com/@WhiteBoxDev)

[White Box Dev](https://www.youtube.com/@WhiteBoxDev)

57,001 views  19 Sept 2020

https://www.youtube.com/watch?v=slTEz6555Ts

Binary Space Trees/Partitioning

https://www.roguebasin.com/index.php/Basic_BSP_Dungeon_generation

https://www.youtube.com/watch?v=nbi88hY9hcw&list=PLcRSafycjWFenI87z7uZHFv6cUG2Tzu9v&index=14

https://medium.com/@guribemontero/dungeon-generation-using-binary-space-trees-47d4a668e2d0

Ocean Texture

https://dontstarve.fandom.com/wiki/Ocean?file=Shallow_Ocean_Terrain_Texture.png#Shipwrecked



Unity Packages:

https://assetstore.unity.com/packages/tools/behavior-ai/npbehave-event-driven-code-based-behaviour-trees-75884

https://assetstore.unity.com/packages/3d/vehicles/air/planes-choppers-polypack-194946


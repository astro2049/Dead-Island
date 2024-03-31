using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CellularAutomata
{
    readonly int[][] neighborOffsets = new int[][] {
        new int[] { -1, -1 }, new int[] { -1, 0 }, new int[] { -1, 1 },
        new int[] { 0, -1 }, new int[] { 0, 1 },
        new int[] { 1, -1 }, new int[] { 1, 0 }, new int[] { 1, 1 }
    };
    int initialLandDensity;
    int iterations;

    public CellularAutomata(int initialLandDensity, int iterations)
    {
        this.initialLandDensity = initialLandDensity;
        this.iterations = iterations;
    }

    public Island generateSimpleIsland(ref Island island, ref TileType[,] grid)
    {
        int padding = island.getPadding();
        for (int i = island.getXLeft() + padding; i < island.getXRight() - padding; i++) {
            for (int j = island.getYLeft() + padding; j < island.getYRight() - padding; j++) {
                grid[i, j] = TileType.Land;
            }
        }
        return island;
    }

    public Island generateIsland(ref Island island, ref TileType[,] grid)
    {
        initializeIsland(ref island, ref grid);

        int padding = island.getPadding();
        int xTopLeft = island.getXLeft(), yTopLeft = island.getYLeft(), xDownRight = island.getXRight(), yDownRight = island.getYRight();
        // Apply cellular automata
        for (int round = 0; round < iterations; round++) {
            var newGrid = grid;
            for (int i = xTopLeft + padding; i < xDownRight - padding; i++) {
                for (int j = yTopLeft + padding; j < yDownRight - padding; j++) {
                    newGrid[i, j] = getNewState(i, j, ref grid);
                }
            }
            grid = newGrid;
        }
        return island;
    }

    /*
    void initializeIsland()
    {
        int padding = island.getPadding();
        int xTopLeft = island.getXLeft(), yTopLeft = island.getYLeft(), xDownRight = island.getXRight(), yDownRight = island.getYRight();
        for (int i = xTopLeft + padding; i < xDownRight - padding; i++) {
            for (int j = yTopLeft + padding; j < yDownRight - padding; j++) {
                if (Random.Range(0, 100) < initialLandDensity) {
                    grid[i, j] = TileType.Ocean;
                } else {
                    grid[i, j] = TileType.Land;
                }
            }
        }
    }
    */

    void initializeIsland(ref Island island, ref TileType[,] grid)
    {
        int[] islandCenter = island.getCenter();
        grid[islandCenter[0], islandCenter[1]] = TileType.Land;
    }

    TileType getNewState(int x, int y, ref TileType[,] grid)
    {
        var neighbors = getMooreNeighbors(x, y, ref grid);
        int oceanNeighbors = neighbors[TileType.Ocean];
        if (oceanNeighbors >= 5) {
            return TileType.Ocean;
        } else if (oceanNeighbors <= 2) {
            return TileType.Land;
        } else {
            return grid[x, y];
        }
    }

    Dictionary<TileType, int> getMooreNeighbors(int x, int y, ref TileType[,] grid)
    {
        var mooreNeighbors = new Dictionary<TileType, int> {
            { TileType.Ocean, 0 },
            { TileType.Land, 0 }
        };
        for (int k = 0; k < neighborOffsets.GetLength(0); k++) {
            int xx = x + neighborOffsets[k][0], yy = y + neighborOffsets[k][1];
            if (xx < 0 || xx >= grid.GetLength(0) || yy < 0 || yy >= grid.GetLength(1)) {
                mooreNeighbors[TileType.Ocean]++;
            }
            mooreNeighbors[grid[xx, yy]]++;
        }
        return mooreNeighbors;
    }
}

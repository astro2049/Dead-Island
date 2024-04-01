using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CellularAutomata
{
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
                grid[i, j] = TileType.Beach;
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
        // Island minimum width = 3 + 2 * padding
        int[] islandCenter = island.getCenter();
        int x = islandCenter[0], y = islandCenter[1];
        for (int i = -1; i <= 1; i++) {
            int xx = x + i;
            for (int j = -1; j <= 1; j++) {
                int yy = y + j;
                if (xx < 0 || xx >= grid.GetLength(0) || yy < 0 || yy >= grid.GetLength(1)) {
                    continue;
                }
                grid[xx, yy] = TileType.Forest;
            }
        }
    }

    TileType getNewState(int x, int y, ref TileType[,] grid)
    {
        if (grid[x, y] != TileType.Ocean) {
            return grid[x, y];
        }
        var neighbors = Utils.getMooreNeighbors(x, y, ref grid);
        int landNeighbors = neighbors[TileType.Forest].Count;
        if (landNeighbors >= 3) {
            return Random.Range(0, 100) > 50 ? TileType.Forest : TileType.Ocean;
        } else {
            return TileType.Ocean;
        }
    }
}

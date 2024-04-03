using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class CellularAutomata
{
    public static int iterations;

    public static Island generateSimpleIsland(ref Island island, ref TileType[,] grid)
    {
        Vector2Int topLeft = island.getTopLeft(), downRight = island.getDownRight();
        int padding = island.getPadding();
        for (int i = topLeft.x + padding; i < downRight.x - padding; i++) {
            for (int j = topLeft.y + padding; j < downRight.y - padding; j++) {
                grid[i, j] = TileType.Beach;
            }
        }
        return island;
    }

    static void initializeIsland(ref Island island, ref TileType[,] grid)
    {
        // Island minimum width = 3 + 2 * padding
        var islandCenter = island.getCenter();
        int x = islandCenter.x, y = islandCenter.y;
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

    public static void generateIsland(ref Island island, ref TileType[,] grid)
    {
        initializeIsland(ref island, ref grid);

        Vector2Int topLeft = island.getTopLeft(), downRight = island.getDownRight();
        int padding = island.getPadding();
        // Apply cellular automata
        for (int round = 0; round < iterations; round++) {
            var newGrid = grid;
            for (int i = topLeft.x + padding; i < downRight.x - padding; i++) {
                for (int j = topLeft.y + padding; j < downRight.y - padding; j++) {
                    newGrid[i, j] = getNewState(i, j, ref grid);
                }
            }
            grid = newGrid;
        }
    }

    static TileType getNewState(int x, int y, ref TileType[,] grid)
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

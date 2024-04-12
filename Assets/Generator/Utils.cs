using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static Dictionary<TileType, List<int[]>> GetMooreNeighbors(int x, int y, TileType[,] grid)
    {
        var mooreNeighbors = new Dictionary<TileType, List<int[]>> {
            { TileType.Ocean, new List<int[]>() },
            { TileType.Beach, new List<int[]>() },
            { TileType.Forest, new List<int[]>() }
        };
        for (int i = -1; i <= 1; i++) {
            int xx = x + i;
            for (int j = -1; j <= 1; j++) {
                int yy = y + j;
                if (xx < 0 || xx >= grid.GetLength(0) || yy < 0 || yy >= grid.GetLength(1)) {
                    mooreNeighbors[TileType.Ocean].Add(new int[] { xx, yy });
                }
                mooreNeighbors[grid[xx, yy]].Add(new int[] { xx, yy });
            }
        }
        return mooreNeighbors;
    }
}

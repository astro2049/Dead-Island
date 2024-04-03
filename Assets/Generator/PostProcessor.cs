using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PostProcessor
{
    public static void connectIslands(ref Island island, ref TileType[,] grid)
    {
        if (island.getIsLeaf()) {
            return;
        }
        var center1 = island.getLeftIsland().getCenter();
        var center2 = island.getRightIsland().getCenter();
        int x1 = center1.x, y1 = center1.y, x2 = center2.x, y2 = center2.y;
        if (x1 == x2) {
            for (int j = y1 + 1; j < y2; j++) {
                if (grid[x1, j] != TileType.Ocean) {
                    continue;
                }
                grid[x1, j] = TileType.Beach;
            }
        } else {
            for (int i = x1 + 1; i < x2; i++) {
                if (grid[i, y1] != TileType.Ocean) {
                    continue;
                }
                grid[i, y1] = TileType.Beach;
            }
        }
        connectIslands(ref island.getLeftIsland(), ref grid);
        connectIslands(ref island.getRightIsland(), ref grid);
    }

    public static void generateCoastlines(ref Island island, ref TileType[,] grid)
    {
        Vector2Int topLeft = island.getTopLeft(), downRight = island.getDownRight();
        int padding = island.getPadding();
        for (int i = topLeft.x + padding; i < downRight.x - padding; i++) {
            for (int j = topLeft.y + padding; j < downRight.y - padding; j++) {
                if (grid[i, j] != TileType.Forest) {
                    continue;
                }
                var neighbors = Utils.getMooreNeighbors(i, j, ref grid);
                if (neighbors[TileType.Ocean].Count != 0) {
                    // if on coastline
                    grid[i, j] = TileType.Beach;
                    foreach (int[] coordinate in neighbors[TileType.Forest]) {
                        if (Random.Range(0, 100) > 25) {
                            continue;
                        }
                        grid[coordinate[0], coordinate[1]] = TileType.Beach;
                    }
                }
            }
        }
    }
}

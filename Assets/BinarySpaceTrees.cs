using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum TileType
{
    Ocean = 0,
    Beach = 1,
    Forest = 2
}

public class BinarySpaceTrees : MonoBehaviour
{
    TileType[,] grid;

    public int width, height;
    public int maxIslandWidth, maxIslandHeight;
    public int wiggle;
    public int islandPadding;
    public GameObject oceanTile, beachTile, forestTile;
    public GameObject tiles;
    public int iterations;

    int tileSize;
    int widthCenterOffset, heightCenterOffset;
    Island initialIsland;
    CellularAutomata ca;

    void Start()
    {
        // Initializations
        tileSize = 1;
        widthCenterOffset = (tileSize * width - tileSize) / 2;
        heightCenterOffset = (tileSize * height - tileSize) / 2;
        Camera.main.orthographicSize = tileSize * height / 2; // Camera's half-size of the vertical viewing volume when in orthographic mode. https://docs.unity3d.com/ScriptReference/Camera-orthographicSize.html
        ca = new CellularAutomata(45, iterations);

        // Apply binary space partitioning
        generate();
    }

    public void generate()
    {
        grid = new TileType[width, height];
        initialIsland = new Island(0, 0, width, height, islandPadding);
        partition(ref initialIsland);
        connectIslands(ref initialIsland);

        // Draw the tile grid
        drawGrid(tiles);
    }

    void partition(ref Island island)
    {
        if (island.getWidth() <= maxIslandWidth && island.getHeight() <= maxIslandHeight) {
            island.setIsLeaf();
            island = ca.generateIsland(ref island, ref grid);
            generateCoastlines(ref island);
            return;
        }

        if (island.getWidth() > maxIslandHeight && island.getHeight() > maxIslandHeight) {
            if (Random.Range(0, 100) < 50) {
                // X - horizontal split
                splitHorizontally(ref island);
            } else {
                // Y - vertical split
                splitVertically(ref island);
            }
        } else if (island.getWidth() > maxIslandHeight) {
            // X - horizontal split
            splitHorizontally(ref island);
        } else {
            // Y - vertical split
            splitVertically(ref island);
        }
        partition(ref island.getLeftIsland());
        partition(ref island.getRightIsland());
    }

    void splitHorizontally(ref Island island)
    {
        int xTopLeft = island.getXLeft(), yTopLeft = island.getYLeft(), xDownRight = island.getXRight(), yDownRight = island.getYRight();
        int xMid = (xTopLeft + xDownRight) / 2;
        xMid = Random.Range(xMid - wiggle, xMid + wiggle);
        island.setLeftIsland(new Island(xTopLeft, yTopLeft, xMid, yDownRight, islandPadding));
        island.setRightIsland(new Island(xMid, yTopLeft, xDownRight, yDownRight, islandPadding));
    }

    void splitVertically(ref Island island)
    {
        int xTopLeft = island.getXLeft(), yTopLeft = island.getYLeft(), xDownRight = island.getXRight(), yDownRight = island.getYRight();
        int yMid = (yTopLeft + yDownRight) / 2;
        yMid = Random.Range(yMid - wiggle, yMid + wiggle);
        island.setLeftIsland(new Island(xTopLeft, yTopLeft, xDownRight, yMid, islandPadding));
        island.setRightIsland(new Island(xTopLeft, yMid, xDownRight, yDownRight, islandPadding));
    }

    void connectIslands(ref Island island)
    {
        if (island.getIsLeaf()) {
            return;
        }
        int[] center1 = island.getLeftIsland().getCenter();
        int[] center2 = island.getRightIsland().getCenter();
        int x1 = center1[0], y1 = center1[1], x2 = center2[0], y2 = center2[1];
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
        connectIslands(ref island.getLeftIsland());
        connectIslands(ref island.getRightIsland());
    }

    void generateCoastlines(ref Island island)
    {
        int padding = island.getPadding();
        int xTopLeft = island.getXLeft(), yTopLeft = island.getYLeft(), xDownRight = island.getXRight(), yDownRight = island.getYRight();
        for (int i = xTopLeft + padding; i < xDownRight - padding; i++) {
            for (int j = yTopLeft + padding; j < yDownRight - padding; j++) {
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

    void drawGrid(GameObject tileCollection)
    {
        foreach (Transform childTransform in tileCollection.transform) {
            Destroy(childTransform.gameObject);
        }
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                GameObject tile;
                switch (grid[i, j]) {
                    case TileType.Beach:
                        tile = Instantiate(beachTile, tileCollection.transform);
                        tile.transform.localPosition =
                            new Vector3(j * tileSize - widthCenterOffset, 0, -i * tileSize + heightCenterOffset);
                        break;
                    case TileType.Forest:
                        tile = Instantiate(forestTile, tileCollection.transform);
                        tile.transform.localPosition =
                            new Vector3(j * tileSize - widthCenterOffset, 0, -i * tileSize + heightCenterOffset);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}

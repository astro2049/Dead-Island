using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Ocean = 0,
    Land = 1
}

public class BinarySpaceTrees : MonoBehaviour
{
    TileType[,] grid;

    public int width, height;
    public int minIslandWidth, minIslandHeight;
    public int islandPadding;
    public GameObject oceanTile, landTile;
    public GameObject tiles;

    int tileSize;
    int widthCenterOffset, heightCenterOffset;
    Island initialIsland;
    CellularAutomata ca;

    void Start()
    {
        // Initializations
        tileSize = 10;
        widthCenterOffset = (tileSize * width - tileSize) / 2;
        heightCenterOffset = (tileSize * height - tileSize) / 2;
        grid = new TileType[width, height];
        Camera.main.orthographicSize = tileSize * height / 2; // Camera's half-size of the vertical viewing volume when in orthographic mode. https://docs.unity3d.com/ScriptReference/Camera-orthographicSize.html
        ca = new CellularAutomata(45, 3);

        // Apply binary space partitioning
        initialIsland = new Island(0, 0, width, height, islandPadding);
        generate();

        // Draw the tile grid
        drawGrid(tiles);
    }

    public void generate() { partition(ref initialIsland); }

    void partition(ref Island island)
    {
        if (island.getWidth() <= minIslandWidth && island.getHeight() <= minIslandHeight) {
            island.setIsLeaf();
            island = ca.generateSimpleIsland(ref island, ref grid);
            return;
        }

        if (island.getWidth() > minIslandHeight && island.getHeight() > minIslandHeight) {
            if (Random.Range(0, 100) < 50) {
                // X - horizontal split
                splitHorizontally(ref island);
            } else {
                // Y - vertical split
                splitVertically(ref island);
            }
        } else if (island.getWidth() > minIslandHeight) {
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
        island.setLeftIsland(new Island(xTopLeft, yTopLeft, xMid, yDownRight, islandPadding));
        island.setRightIsland(new Island(xMid, yTopLeft, xDownRight, yDownRight, islandPadding));
    }

    void splitVertically(ref Island island)
    {
        int xTopLeft = island.getXLeft(), yTopLeft = island.getYLeft(), xDownRight = island.getXRight(), yDownRight = island.getYRight();
        int yMid = (yTopLeft + yDownRight) / 2;
        island.setLeftIsland(new Island(xTopLeft, yTopLeft, xDownRight, yMid, islandPadding));
        island.setRightIsland(new Island(xTopLeft, yMid, xDownRight, yDownRight, islandPadding));
    }

    void drawGrid(GameObject tileCollection)
    {
        foreach (Transform childTransform in tileCollection.transform) {
            Destroy(childTransform.gameObject);
        }
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                var tile = Instantiate(grid[i, j] == TileType.Ocean ? oceanTile : landTile, tileCollection.transform);
                tile.transform.localPosition =
                    new Vector3(j * tileSize - widthCenterOffset, 0, -i * tileSize + heightCenterOffset);
            }
        }
    }
}

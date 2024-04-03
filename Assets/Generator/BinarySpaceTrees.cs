using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public enum TileType
{
    Ocean = 0,
    Beach = 1,
    Forest = 2
}

public class BinarySpaceTrees : MonoBehaviour
{
    private TileType[,] grid;

    public int width, height;
    public int maxIslandWidth, maxIslandHeight;
    public int wiggle;
    public int islandPadding;
    public GameObject beachTile, forestTile;
    public GameObject tiles;
    public int iterations;

    private readonly int tileSize = 1;
    private int widthCenterOffset, heightCenterOffset;
    private Island initialIsland;

    private List<Island> leafIslands = new List<Island>();

    private void Start()
    {
        // Initializations
        widthCenterOffset = (tileSize * width - tileSize) / 2;
        heightCenterOffset = (tileSize * height - tileSize) / 2;
        Camera.main.orthographicSize = tileSize * height / 2; // Camera's half-size of the vertical viewing volume when in orthographic mode. https://docs.unity3d.com/ScriptReference/Camera-orthographicSize.html
        CellularAutomata.iterations = iterations;

        // Apply binary space partitioning
        generate();

        // Build nav mesh
        NavMeshSurface navMeshSurface = GetComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();
    }

    public void generate()
    {
        grid = new TileType[width, height];
        initialIsland = new Island(0, 0, width, height, islandPadding);
        partition(ref initialIsland);
        PostProcessor.connectIslands(ref initialIsland, ref grid);

        // Draw the tile grid
        drawGrid(tiles);
    }

    private void partition(ref Island island)
    {
        if (island.getWidth() <= maxIslandWidth && island.getHeight() <= maxIslandHeight) {
            island.setIsLeaf(ref grid);
            leafIslands.Add(island);
            CellularAutomata.generateIsland(ref island, ref grid); // writes grid
            PostProcessor.generateCoastlines(ref island, ref grid);
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

    private void splitHorizontally(ref Island island)
    {
        Vector2Int topLeft = island.getTopLeft(), downRight = island.getDownRight();
        int xMid = (topLeft.x + downRight.x) / 2;
        xMid = Random.Range(xMid - wiggle, xMid + wiggle);
        island.setLeftIsland(new Island(topLeft.x, topLeft.y, xMid, downRight.y, islandPadding));
        island.setRightIsland(new Island(xMid, topLeft.y, downRight.x, downRight.y, islandPadding));
    }

    private void splitVertically(ref Island island)
    {
        Vector2Int topLeft = island.getTopLeft(), downRight = island.getDownRight();
        int yMid = (topLeft.y + downRight.y) / 2;
        yMid = Random.Range(yMid - wiggle, yMid + wiggle);
        island.setLeftIsland(new Island(topLeft.x, topLeft.y, downRight.x, yMid, islandPadding));
        island.setRightIsland(new Island(topLeft.x, yMid, downRight.x, downRight.y, islandPadding));
    }

    private void drawGrid(GameObject tileCollection)
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

using UnityEngine;
using Random = UnityEngine.Random;

public class CellularAutomata : MonoBehaviour
{
    enum TileType
    {
        Ocean = 0,
        Land = 1
    }

    int[,] neighbors = {
        { -1, -1 }, { -1, 0 }, { -1, 1 },
        { 0, -1 }, { 0, 1 },
        { 1, -1 }, { 1, 0 }, { 1, 1 }
    };

    public int width = 32;
    int height;
    public int density = 45; // Land area = 45%
    public int iterations = 3;
    public GameObject oceanTile, landTile;
    public GameObject originalTiles, tiles;

    TileType[,] grid;
    int tileSize;
    int centerOffset;

    void logGrid()
    {
        for (int i = 0; i < width; i++) {
            string row = "";
            for (int j = 0; j < height; j++) {
                row += (int)grid[i, j] + " ";
            }
            Debug.Log(row);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Initialize fields
        height = width;
        grid = new TileType[width, height];
        tileSize = 10;
        centerOffset = (tileSize * width - tileSize) / 2;
        // Camera's half-size of the vertical viewing volume when in orthographic mode. https://docs.unity3d.com/ScriptReference/Camera-orthographicSize.html
        Camera.main.orthographicSize = tileSize * width / 2;

        // Cellular Automata
        generate();
    }

    public void generate()
    {
        // Cellular Automata
        makeNoiseGrid();
        drawGrid(originalTiles);
        applyCellularAutomation();
        drawGrid(tiles);
    }

    void makeNoiseGrid()
    {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (Random.Range(1, 100) > density) {
                    grid[i, j] = TileType.Ocean;
                } else {
                    grid[i, j] = TileType.Land;
                }
            }
        }
    }

    void applyCellularAutomation()
    {
        for (int round = 0; round < iterations; round++) {
            var newGrid = grid;
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    newGrid[i, j] = getNewState(i, j);
                }
            }
            grid = newGrid;
        }
    }

    TileType getNewState(int x, int y)
    {
        int oceanNeighbors = 0;
        for (int k = 0; k < neighbors.GetLength(0); k++) {
            int xx = x + neighbors[k, 0], yy = y + neighbors[k, 1];
            TileType neighborType;
            if (xx < 0 || xx >= width || yy < 0 || yy >= height) {
                neighborType = TileType.Ocean;
            } else {
                neighborType = grid[xx, yy];
            }

            if (neighborType == TileType.Ocean) {
                oceanNeighbors++;
            }
        }
        if (oceanNeighbors > 4) {
            return TileType.Ocean;
        } else {
            return TileType.Land;
        }
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
                    new Vector3(j * tileSize - centerOffset, 0, -i * tileSize + centerOffset);
            }
        }
    }
}

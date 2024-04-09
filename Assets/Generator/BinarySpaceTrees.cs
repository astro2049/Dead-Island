using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

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
    private NavMeshSurface navMeshSurface;

    public GameObject survivorsGameObject, zombiesGameObject;
    public GameObject survivorPrefab, zombiePrefab;
    private List<GameObject> survivors = new List<GameObject>();
    private List<GameObject> zombies = new List<GameObject>();

    public GameObject safeZoneTextPrefab;

    private void Start()
    {
        // Initializations
        widthCenterOffset = (tileSize * width - tileSize) / 2;
        heightCenterOffset = (tileSize * height - tileSize) / 2;
        Camera.main.orthographicSize = tileSize * height / 2; // Camera's half-size of the vertical viewing volume when in orthographic mode. https://docs.unity3d.com/ScriptReference/Camera-orthographicSize.html
        CellularAutomata.iterations = iterations;

        navMeshSurface = GetComponent<NavMeshSurface>();

        // Apply binary space partitioning
        generate();
    }

    public void generate()
    {
        grid = new TileType[width, height];
        initialIsland = new Island(0, 0, width, height, islandPadding);
        partition(ref initialIsland);

        // Post process
        PostProcessor.connectIslands(ref initialIsland, ref grid);

        // Draw the tile grid
        drawGrid(tiles);

        // Build nav mesh
        navMeshSurface.BuildNavMesh();

        // Place survivors and zombies, and safe zone text
        placeActors();
        placeSafeZoneText();
    }

    private void partition(ref Island island)
    {
        if (island.getWidth() <= maxIslandWidth && island.getHeight() <= maxIslandHeight) {
            island.setIsLeaf();
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
                        tile.transform.localPosition = girdCoordToWorld(i, j);
                        break;
                    case TileType.Forest:
                        tile = Instantiate(forestTile, tileCollection.transform);
                        tile.transform.localPosition = girdCoordToWorld(i, j);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private Vector3Int girdCoordToWorld(int x, int y)
    {
        return new Vector3Int(y * tileSize - widthCenterOffset, 0, -x * tileSize + heightCenterOffset);
    }

    private void placeActors()
    {
        int distance = 4;
        for (int i = 0; i < leafIslands.Count - 1; i++) {
            var island = leafIslands[i];
            var center = island.getCenter();
            var worldCoord = girdCoordToWorld(center.x, center.y);
            Debug.DrawLine(new Vector3(0, 20, 0), new Vector3(worldCoord.x, 0, worldCoord.z), Color.green, 5);
            int actorCount = Random.Range(1, 4);
            for (int j = 0; j < actorCount; j++) {
                var direction = Random.insideUnitCircle;
                NavMeshHit hit;
                NavMesh.SamplePosition(new Vector3(worldCoord.x + distance * direction.x, 0, worldCoord.z + distance * direction.y), out hit, distance, NavMesh.AllAreas);
                if (i == 0) {
                    var survivor = Instantiate(survivorPrefab, hit.position, Quaternion.identity, survivorsGameObject.transform);
                    survivor.transform.position = hit.position;
                    survivors.Add(survivor);
                } else {
                    var zombie = Instantiate(zombiePrefab, hit.position, Quaternion.identity, zombiesGameObject.transform);
                    zombie.transform.position = hit.position;
                    zombies.Add(zombie);
                }
            }
        }
    }

    private void placeSafeZoneText()
    {
        var center = leafIslands.Last().getCenter();
        var safeZone = Instantiate(safeZoneTextPrefab, girdCoordToWorld(center.x, center.y), Quaternion.identity);
        foreach (var survivor in survivors) {
            survivor.GetComponent<SurvivorAI>().safeZoneLocation = safeZone.transform;
        }
    }

    public void ActivateActors()
    {
        foreach (var survivor in survivors) {
            var survivorAI = survivor.GetComponent<SurvivorAI>();
            survivorAI.ActivateBT();
        }
        foreach (var zombie in zombies) {
            var zombieAI = zombie.GetComponent<ZombieAI>();
            zombieAI.ActivateBT();
        }
    }
}

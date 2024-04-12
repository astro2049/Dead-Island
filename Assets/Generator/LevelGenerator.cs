using System.Collections.Generic;
using System.Linq;
using Agents;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace Generator
{
    public enum TileType
    {
        Ocean = 0,
        Beach = 1,
        Forest = 2
    }

    public class LevelGenerator : MonoBehaviour
    {
        private GameManager gameManager;

        public TileType[,] grid;
        private BinarySpaceTrees BSP;
        private CellularAutomata CA;
        private PostProcessor PP;

        public int m_width, m_height;
        public int m_maxIslandWidth, m_maxIslandHeight;
        public int m_splitWiggle;
        public int m_islandPadding;
        public GameObject m_beachTile, m_forestTile;
        public GameObject m_tiles;
        public int m_iterations;

        public readonly float tileSize = 1.5f;
        [HideInInspector] public float m_widthCenterOffset, m_heightCenterOffset;
        public Island initialIsland;

        public readonly List<Island> leafIslands = new List<Island>();
        private NavMeshSurface m_navMeshSurface;

        public GameObject m_survivorsGameObject, m_zombiesGameObject;
        public GameObject m_survivorPrefab, m_zombiePrefab;

        public GameObject m_safeZoneTextPrefab;

        private void Start()
        {
            // Initializations
            gameManager = GetComponent<GameManager>();

            grid = new TileType[m_width, m_height];
            BSP = new BinarySpaceTrees(this);
            CA = new CellularAutomata(m_iterations, grid);
            PP = new PostProcessor(grid);

            m_widthCenterOffset = (tileSize * m_width - tileSize) / 2;
            m_heightCenterOffset = (tileSize * m_height - tileSize) / 2;
            Camera.main.orthographicSize = tileSize * m_height / 2; // Camera's half-size of the vertical viewing volume when in orthographic mode. https://docs.unity3d.com/ScriptReference/Camera-orthographicSize.html

            m_navMeshSurface = GetComponent<NavMeshSurface>();

            Generate();
        }

        public void Generate()
        {
            initialIsland = new Island(0, 0, m_width, m_height, m_islandPadding);

            // Apply binary space partitioning
            BSP.Partition(initialIsland);
            GenerateLeafIslands();

            // Post process
            PP.ConnectIslands(initialIsland);

            // Draw the tile grid
            DrawGrid(m_tiles);

            // Build nav mesh
            m_navMeshSurface.BuildNavMesh();

            // Place survivors and zombies, and safe zone text
            PlaceActors();
            PlaceSafeZoneText();
        }

        private void GenerateLeafIslands()
        {
            foreach (var leafIsland in leafIslands) {
                CA.GenerateIsland(leafIsland);
                PP.GenerateCoastlines(leafIsland);
            }
        }

        private void DrawGrid(GameObject tileCollection)
        {
            foreach (Transform childTransform in tileCollection.transform) {
                Destroy(childTransform.gameObject);
            }
            for (int i = 0; i < m_width; i++) {
                for (int j = 0; j < m_height; j++) {
                    GameObject tile;
                    switch (grid[i, j]) {
                        case TileType.Beach:
                            tile = Instantiate(m_beachTile, tileCollection.transform);
                            tile.transform.localPosition = GirdCoordToWorld(i, j);
                            break;
                        case TileType.Forest:
                            tile = Instantiate(m_forestTile, tileCollection.transform);
                            tile.transform.localPosition = GirdCoordToWorld(i, j);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private Vector3 GirdCoordToWorld(int x, int y)
        {
            return new Vector3(y * tileSize - m_widthCenterOffset, 0, -x * tileSize + m_heightCenterOffset);
        }

        private void PlaceActors()
        {
            int distance = 10;
            for (int i = 0; i < leafIslands.Count - 1; i++) {
                var island = leafIslands[i];
                var center = island.getCenter();
                var worldCoord = GirdCoordToWorld(center.x, center.y);
                Debug.DrawLine(new Vector3(0, 20, 0), new Vector3(worldCoord.x, 0, worldCoord.z), Color.green, 5);
                int actorCount = Random.Range(5, 10);
                for (int j = 0; j < actorCount; j++) {
                    var direction = Random.insideUnitCircle;
                    NavMeshHit hit;
                    NavMesh.SamplePosition(new Vector3(worldCoord.x + distance * direction.x, 0, worldCoord.z + distance * direction.y), out hit, distance, NavMesh.AllAreas);
                    if (i == 0) {
                        var survivor = Instantiate(m_survivorPrefab, hit.position, Quaternion.identity, m_survivorsGameObject.transform);
                        survivor.transform.position = hit.position;
                        survivor.GetComponent<IndividualAgent>().m_gameManager = gameManager;
                        gameManager.m_survivors.Add(survivor);
                        gameManager.m_survivorCount++;
                        gameManager.UpdateSurvivorCountText();
                        break;
                    } else {
                        var zombie = Instantiate(m_zombiePrefab, hit.position, Quaternion.identity, m_zombiesGameObject.transform);
                        zombie.transform.position = hit.position;
                        zombie.GetComponent<IndividualAgent>().m_gameManager = gameManager;
                        gameManager.m_zombies.Add(zombie);
                        gameManager.m_zombieCount++;
                        gameManager.UpdateZombieCountText();
                    }
                }
            }
        }

        private void PlaceSafeZoneText()
        {
            var center = leafIslands.Last().getCenter();
            var safeZone = Instantiate(m_safeZoneTextPrefab, GirdCoordToWorld(center.x, center.y), Quaternion.identity);
            foreach (var survivor in gameManager.m_survivors) {
                survivor.GetComponent<SurvivorAI>().m_safeZoneTransform = safeZone.transform;
            }
        }
    }
}

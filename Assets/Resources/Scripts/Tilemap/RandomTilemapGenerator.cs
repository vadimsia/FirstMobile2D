using System.Collections.Generic;
using UnityEngine;
using UT = UnityEngine.Tilemaps;

namespace Resources.Scripts.Tilemap
{
    /// <summary>
    /// Generates a random tilemap by placing grass tiles while avoiding placing the same tile type in adjacent cells.
    /// </summary>
    public class RandomTilemapGenerator : MonoBehaviour
    {
        [Header("Tilemap Settings")]
        [Tooltip("Reference to the Tilemap component.")]
        public UT.Tilemap tilemapComponent;
        [Tooltip("Array of grass tile variants (e.g., 8 different textures).")]
        public UT.TileBase[] grassTiles;
        [Tooltip("Width of the generated map.")]
        public int width = 10;
        [Tooltip("Height of the generated map.")]
        public int height = 10;

        // Directions for checking all 8 neighboring cells.
        private readonly Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int( 1,  0, 0),
            new Vector3Int( 1,  1, 0),
            new Vector3Int( 0,  1, 0),
            new Vector3Int(-1,  1, 0),
            new Vector3Int(-1,  0, 0),
            new Vector3Int(-1, -1, 0),
            new Vector3Int( 0, -1, 0),
            new Vector3Int( 1, -1, 0)
        };

        /// <summary>
        /// Called before the first frame update.
        /// Generates the random tilemap.
        /// </summary>
        private void Start()
        {
            GenerateRandomMap();
        }

        /// <summary>
        /// Generates a random map by setting tiles while ensuring that adjacent cells do not have the same grass tile.
        /// </summary>
        private void GenerateRandomMap()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Start with all available grass tiles.
                    List<UT.TileBase> availableTiles = new List<UT.TileBase>(grassTiles);
                    Vector3Int currentPos = new Vector3Int(x, y, 0);

                    // Check all 8 neighboring positions.
                    foreach (Vector3Int direction in directions)
                    {
                        Vector3Int neighborPos = currentPos + direction;
                        UT.TileBase neighborTile = tilemapComponent.GetTile(neighborPos);

                        // If a neighbor exists, remove that tile type from the available options.
                        if (neighborTile != null)
                        {
                            availableTiles.Remove(neighborTile);
                        }
                    }

                    // Choose a tile from the remaining options; if none remain, choose any random tile.
                    UT.TileBase chosenTile = availableTiles.Count > 0
                        ? availableTiles[Random.Range(0, availableTiles.Count)]
                        : grassTiles[Random.Range(0, grassTiles.Length)];

                    // Place the chosen tile at the current position.
                    tilemapComponent.SetTile(currentPos, chosenTile);
                }
            }
        }
    }
}

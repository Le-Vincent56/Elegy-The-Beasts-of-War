using Elegy.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elegy.Characters;

namespace Elegy.Grid
{
    public class Grid : MonoBehaviour
    {
        private Node[,] grid;
        [SerializeField] private int width;
        [SerializeField] private int length;
        [SerializeField] private float cellSize;
        [SerializeField] private LayerMask obstacleLayer;

        private void Awake()
        {
            GenerateGrid();
        }

        /// <summary>
        /// Create a new grid and check the passable terrain
        /// </summary>
        private void GenerateGrid()
        {
            // Create grid
            grid = new Node[length, width];

            // Initialize nodes
            for(int x = 0; x < length; x++)
            {
                for(int y =0; y < width; y++)
                {
                    grid[x, y] = new Node();
                }
            }

            // Check terrain
            CheckPassableTerrain();
        }

        private bool CheckBounds(Vector2Int positionOnGrid)
        {
            if (positionOnGrid.x < 0 || positionOnGrid.x >= length)
            {
                return false;
            }

            if(positionOnGrid.y < 0 || positionOnGrid.y >= width)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Set the passable nodes of the grid
        /// </summary>
        private void CheckPassableTerrain()
        {
            // Loop through the grid
            for(int x = 0; x < length; x++)
            {
                for(int y = 0; y < width; y++)
                {
                    // Get the world position of each cell
                    Vector3 worldPosition = GetWorldPosition(x, y);

                    // Check if the cell collides with anything
                    bool passable = !Physics.CheckBox(worldPosition, Vector3.one / 2 * cellSize, Quaternion.identity, obstacleLayer);

                    // Set the passability of the ndoe
                    grid[x, y].passable = passable;
                }
            }
        }

        /// <summary>
        /// Get the world position of a grid coordinate
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        /// <returns>A Vector3 that represents the world position of the (x, y) coordinate on the grid</returns>
        private Vector3 GetWorldPosition(int x, int y)
        {
            // Return the world position according to cell size
            // Uses z instead of y because of top-down
            return new Vector3(transform.position.x + (x * cellSize), 0f, transform.position.z + (y * cellSize));
        }

        #region EVENT FUNCTIONS
        /// <summary>
        /// Get the grid position of an object
        /// </summary>
        /// <param name="sender">The component raising the event</param>
        /// <param name="data">The position of the object in world space</param>
        public void GetGridPosition(Component sender, object data)
        {
            // Check if data is the correct type
            if(data is Vector3)
            {
                // Calculate grid position
                Vector3 worldPosition = (Vector3)data;
                worldPosition -= transform.position;

                // Send data depending on the sender type
                if (sender is GridObject)
                {
                    sender.gameObject.GetComponent<GridObject>().gridPos = new Vector2Int((int)(worldPosition.x / cellSize), (int)(worldPosition.z / cellSize));
                }

                if (sender is GridControl)
                {
                    sender.gameObject.GetComponent<GridControl>().gridPos = new Vector2Int((int)(worldPosition.x / cellSize), (int)(worldPosition.z / cellSize));
                }
            }
        }

        /// <summary>
        /// Place an object onto the grid
        /// </summary>
        /// <param name="sender">The component raising the event</param>
        /// <param name="data">The object to be placed</param>
        public void PlaceObject(Component sender, object data)
        {
            if(data is GridObject)
            {
                // Cast the data
                GridObject objToPlace = (GridObject)data;

                // Only place the object if within the bounds
                if(CheckBounds(objToPlace.gridPos))
                {
                    grid[objToPlace.gridPos.x, objToPlace.gridPos.y].gridObject = objToPlace;
                } else
                {
                    Debug.Log($"Attempted to place {objToPlace.gameObject.GetComponent<Character>().characterName} out of grid bounds");
                }
            }
        }

        public void GetPlacedObject(Component sender, object data)
        {
            if(data is Vector2Int)
            {
                Vector2Int objPos = (Vector2Int)data;
                if(CheckBounds(objPos))
                {
                    GridObject gridObject = grid[objPos.x, objPos.y].gridObject;

                    if (sender is GridControl)
                    {
                        sender.gameObject.GetComponent<GridControl>().highlightedObject = gridObject;
                    }
                } else
                {
                    if (sender is GridControl)
                    {
                        sender.gameObject.GetComponent<GridControl>().highlightedObject = null;
                    }
                }
                
            }
        }
        #endregion

        private void OnDrawGizmos()
        {
            if(grid == null)
            {
                return;
            }

            // Loop through the grid
            for (int x = 0; x < length; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    // Get the world position of each cell
                    Vector3 worldPosition = GetWorldPosition(x, y);

                    Gizmos.color = grid[x, y].passable ? Color.white : Color.red;
                    Gizmos.DrawCube(worldPosition, Vector3.one / 4);
                }
            }
        }
    }
}

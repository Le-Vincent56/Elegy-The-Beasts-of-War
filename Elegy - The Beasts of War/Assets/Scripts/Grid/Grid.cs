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
        public int width;
        public int length;
        [SerializeField] private float cellSize;
        [SerializeField] private LayerMask terrainLayer;
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
            grid = new Node[width, length];

            // Initialize nodes
            for(int x = 0; x < width; x++)
            {
                for(int y = 0; y < length; y++)
                {
                    grid[x, y] = new Node();
                }
            }

            // Calculate elevation
            CalculateElevation();

            // Check terrain
            CheckPassableTerrain();
        }

        /// <summary>
        /// Check if a position is within bounds
        /// </summary>
        /// <param name="positionOnGrid">The position to check</param>
        /// <returns>True if the position is within the grid, false otherwise</returns>
        public bool CheckBounds(Vector2Int positionOnGrid)
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
        /// Calculate the highest elevation of the node based on terrain
        /// </summary>
        private void CalculateElevation()
        {
            // Loop through the grid
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < length; y++)
                {
                    // Create rays above the terrain
                    Ray elevationRay = new Ray(GetWorldPosition(x, y) + (Vector3.up * 100f), Vector3.down);
                    RaycastHit hit;

                    // Check for raycast hits
                    if(Physics.Raycast(elevationRay, out hit, float.MaxValue, terrainLayer))
                    {
                        grid[x, y].elevation = hit.point.y;
                    }
                }
            }
        }

        /// <summary>
        /// Set the passable nodes of the grid
        /// </summary>
        private void CheckPassableTerrain()
        {
            // Loop through the grid
            for(int x = 0; x < width; x++)
            {
                for(int y = 0; y < length; y++)
                {
                    // Get the world position of each cell
                    Vector3 worldPosition = GetWorldPosition(x, y, true);

                    // Check if the cell collides with anything
                    bool passable = !Physics.CheckBox(worldPosition, Vector3.one / 2 * cellSize, Quaternion.identity, obstacleLayer);

                    // Set the passability of the ndoe
                    grid[x, y].passable = passable;
                }
            }
        }

        /// <summary>
        /// Check if a node is passable
        /// </summary>
        /// <param name="x">The x position of the node within the grid</param>
        /// <param name="y">The y position of the node within the grid</param>
        /// <returns>True if passable, false otherwise</returns>
        public bool CheckPassable(int x, int y)
        {
            return grid[x, y].passable;
        }

        /// <summary>
        /// Get the world position of a grid coordinate
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        /// <returns>A Vector3 that represents the world position of the (x, y) coordinate on the grid</returns>
        public Vector3 GetWorldPosition(int x, int y, bool elevation = false)
        {
            // Return the world position according to cell size
            // If an elevation is given, set the elevation, default to 0
            return new Vector3(x * cellSize + (cellSize / 2f), elevation == true ? grid[x, y].elevation : 0f, y * cellSize + (cellSize / 2f));
        }

        public Vector2Int GetGridPosition(Vector3 worldPosition)
        {
            Vector2Int returnedPosition = new Vector2Int((int)(worldPosition.x / cellSize), (int)(worldPosition.z / cellSize));

            // If the returned position is out of bounds, then return Vector2Int.zero
            // Otherwise, return the calculated position
            if (!CheckBounds(returnedPosition))
            {
                return Vector2Int.zero;
            }
            else return returnedPosition;
        }

        /// <summary>
        /// Retrieve a placed object on the grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        public GridObject GetPlacedObject(Vector2Int objPos)
        {
            // Check if the attempted retrieval is within the grid
            if (CheckBounds(objPos))
            {
                // Retrieve the object
                return grid[objPos.x, objPos.y].gridObject;
            }
            else return null;
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
                Vector2Int returnedPosition = new Vector2Int((int)(worldPosition.x / cellSize), (int)(worldPosition.z / cellSize));

                // If the returned position is out of bounds, then return
                if (!CheckBounds(returnedPosition))
                {
                    return;
                }

                // Send data depending on the sender type
                if (sender is GridObject)
                {
                    sender.gameObject.GetComponent<GridObject>().gridPos = returnedPosition;
                }

                if (sender is MoveCharacter)
                {
                    sender.gameObject.GetComponent<MoveCharacter>().clickedGridPosition = returnedPosition;
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

        /// <summary>
        /// Get the world position of an object
        /// </summary>
        /// <param name="sender">The component raising the event</param>
        /// <param name="data">The object to find the world position of</param>
        public void GetWorldPosition(Component sender, object data)
        {
            // Check if the data type is correct
            if(data is Vector2Int)
            {
                // Cast the data
                Vector2Int gridPos = (Vector2Int)data;

                // Find the world position
                Vector3 returnedPos = new Vector3(gridPos.x * cellSize + (cellSize / 2f), grid[gridPos.x, gridPos.y].elevation, gridPos.y * cellSize + (cellSize / 2f));
                
                // If the sender is a GridObject, place them
                if(sender is GridObject)
                {
                    sender.gameObject.GetComponent<GridObject>().worldPos = returnedPos;
                    sender.gameObject.transform.position = returnedPos;
                }
            }
        }

        /// <summary>
        /// Convert a List of PathNodes into a List of Vector3 world positions
        /// </summary>
        /// <param name="sender">The component raising the event</param>
        /// <param name="data">The List of Pathnodes to convert</param>
        public void PathNodesToWorldPositions(Component sender, object data)
        {
            // Check if data is the correc type
            if(data is List<PathNode>)
            {
                // Cast data
                List<PathNode> path = (List<PathNode>)data;
                List<Vector3> worldPositions = new List<Vector3>();

                // Calculate all the world positions for each path node
                for(int i = 0; i < path.Count; i++)
                {
                    worldPositions.Add(GetWorldPosition(path[i].xPos, path[i].yPos, true));
                }

                // Send data depending on sender type
                if(sender is Movement)
                {
                    sender.gameObject.GetComponent<Movement>().pathWorldPositions = worldPositions;
                }
            }
        }

        /// <summary>
        /// Highlight the grid on hover
        /// </summary>
        /// <param name="sender">The component raising the event</param>
        /// <param name="data">The grid position of the node to highlight</param>
        public void HighlightOnHover(Component sender, object data)
        {
            // Check if the data is the correct type
            if(data is Vector2Int)
            {
                // Cast the data
                Vector2Int hoverData = (Vector2Int)data;

                // Loop through the array
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < length; y++)
                    {
                        // Set the hovered node to selected and the others to unselected
                        if(x == hoverData.x && y == hoverData.y)
                        {
                            grid[x, y].selected = true;
                        } else
                        {
                            grid[x, y].selected = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update the grid objects within the grid
        /// </summary>
        /// <param name="sender">The component raising the event</param>
        /// <param name="data">The MoveData to update to</param>
        public void UpdateGridObjectPosition(Component sender, object data)
        {
            // Check if the data is the correct type
            if(data is MoveData)
            {
                // Cast the data
                MoveData moveData = (MoveData)data;

                // Loop through the grid
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < length; y++)
                    {
                        // Remove the old grid object
                        if (grid[x, y].gridObject == moveData.targetCharacter)
                        {
                            grid[x, y].gridObject = null;
                        }

                        // Place the new grid object
                        if(x == moveData.targetCharacter.gridPos.x && y == moveData.targetCharacter.gridPos.y)
                        {
                            grid[x, y].gridObject = moveData.targetCharacter;
                        }
                    }
                }
            }
        }
        #endregion

        #region GIZMOS
        private void OnDrawGizmos()
        {
            if(grid == null)
            {
                // Loop through the grid
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < length; y++)
                    {
                        GenerateGrid();

                        // Get the world position of each cell
                        Vector3 worldPosition = GetWorldPosition(x, y, true);

                        Gizmos.color = grid[x, y].passable ? Color.white : Color.red;
                        Gizmos.DrawCube(worldPosition, Vector3.one / 4);
                    }
                }
            }

            // Loop through the grid
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < length; y++)
                {
                    // Get the world position of each cell
                    Vector3 worldPosition = GetWorldPosition(x, y, true);

                    Gizmos.color = grid[x, y].passable ? Color.white : Color.red;
                    Gizmos.DrawCube(worldPosition, Vector3.one / 4);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Loop through the grid
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < length; y++)
                {
                    // Get the world position of each cell
                    Vector3 worldPosition = GetWorldPosition(x, y, true);

                    Gizmos.color = grid[x, y].selected ? Color.green : Color.white;
                    Gizmos.DrawCube(worldPosition, Vector3.one / 4);
                }
            }
        }
        #endregion
    }
}

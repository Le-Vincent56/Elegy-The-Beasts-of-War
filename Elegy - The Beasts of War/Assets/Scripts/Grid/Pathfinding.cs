using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elegy.Events;
using System;

namespace Elegy.Grid
{
    public class PathNode
    {
        public PathNode parentNode;

        public int xPos;
        public int yPos;

        public float gValue;
        public float hValue;

        public float fValue { get { return gValue + hValue; } }

        public PathNode(int xPos, int yPos)
        {
            this.xPos = xPos;
            this.yPos = yPos;
        }
    }

    [RequireComponent(typeof(Grid))]
    public class Pathfinding : MonoBehaviour
    {
        private Grid grid;
        private PathNode[,] pathNodes;

        // Start is called before the first frame update
        void Start()
        {
            Init();
        }

        /// <summary>
        /// Initialize the component
        /// </summary>
        private void Init()
        {
            // Get the grid
            if(grid == null)
            {
                grid = GetComponent<Grid>();
            }

            // Create a new pathnode grid the same size as the normal grid
            pathNodes = new PathNode[grid.width, grid.length];

            // Loop through the grid
            for(int x = 0; x < grid.width; x++)
            {
                for (int y = 0; y < grid.length; y++)
                {
                    // Create pathnode at each point
                    pathNodes[x, y] = new PathNode(x, y);
                }
            }
        }

        public void CalculateWalkableNodes(int startX, int startY, float range, ref List<PathNode> toHighlight)
        {
            PathNode startNode = pathNodes[startX, startY];

            List<PathNode> openList = new List<PathNode>();
            List<PathNode> closedList = new List<PathNode>();

            openList.Add(startNode);

            while(openList.Count > 0)
            {
                PathNode currentNode = openList[0];

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                // Find neighbor nodes in a 3x3 around the current node
                List<PathNode> neighborNodes = new List<PathNode>();
                for (int x = -1; x < 2; x++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        // Ignore the current node at (0, 0)
                        if (x == 0 && y == 0)
                        {
                            continue;
                        }

                        // Ignore any nodes outside of the grid bounds
                        if (!grid.CheckBounds(new Vector2Int(currentNode.xPos + x, currentNode.yPos + y)))
                        {
                            continue;
                        }

                        // State the node as a neighbor
                        neighborNodes.Add(pathNodes[currentNode.xPos + x, currentNode.yPos + y]);
                    }
                }

                for (int i = 0; i < neighborNodes.Count; i++)
                {
                    // If the closed list already contains the neighbor node, ignore it
                    if (closedList.Contains(neighborNodes[i]))
                    {
                        continue;
                    }

                    // If the node is unpassable, then ignore it
                    if (!grid.CheckPassable(neighborNodes[i].xPos, neighborNodes[i].yPos))
                    {
                        continue;
                    }

                    float movementCost = currentNode.gValue + CalculateDistance(currentNode, neighborNodes[i]);

                    // If the movement cost is greater than the range, then continue
                    if(movementCost > range) {
                        continue;
                    }

                    if (!openList.Contains(neighborNodes[i]) || movementCost < neighborNodes[i].gValue)
                    {
                        // Set the neighbor nodes gValue to the movement cost
                        neighborNodes[i].gValue = movementCost;

                        // Set the neighbor node's parentNode to the current node
                        neighborNodes[i].parentNode = currentNode;

                        // If the open list doesn't contain the neighbor node, then add it
                        if (!openList.Contains(neighborNodes[i]))
                        {
                            openList.Add(neighborNodes[i]);
                        }
                    }
                }
            }

            // Add the closedd list to toHighlight
            if(toHighlight != null)
            {
                toHighlight.AddRange(closedList);
            }
        }

        /// <summary>
        /// Find the path from a start (x, y) position to an end (x, y) position
        /// </summary>
        /// <param name="startX">The x position of the starting node</param>
        /// <param name="startY">The y position of the starting node</param>
        /// <param name="endX">The x position of the ending node</param>
        /// <param name="endY">The y position of the ending node</param>
        /// <returns>A List of PathNodes from the start to the end if possible, null if not</returns>
        public List<PathNode> FindPath(int startX, int startY, int endX, int endY)
        {
            PathNode startNode = pathNodes[startX, startY];
            PathNode endNode = pathNodes[endX, endY];

            List<PathNode> openList = new List<PathNode>();
            List<PathNode> closedList = new List<PathNode>();

            openList.Add(startNode);
            while(openList.Count > 0)
            {
                // Set the current node
                PathNode currentNode = openList[0];

                // Loop through the open list
                for(int i = 0; i < openList.Count; i++)
                {
                    // If the current node has a higher fValue, replace the current node with the next one
                    if (currentNode.fValue > openList[i].fValue)
                    {
                        currentNode = openList[i];
                    }

                    // If the current node's hValue equals another node's fValue, and the it is also greater than their hValue,
                    // set the currentNode to the other node
                    if(currentNode.hValue == openList[i].fValue && currentNode.hValue > openList[i].hValue)
                    {
                        currentNode = openList[i];
                    }
                }

                // Move the current node to the closed list
                openList.Remove(currentNode);
                closedList.Add(currentNode);

                // If the current node is the end node, retrace the path
                if(currentNode == endNode)
                {
                    return RetracePath(startNode, endNode);
                }

                // Find neighbor nodes in a 3x3 around the current node
                List<PathNode> neighborNodes = new List<PathNode>();
                for (int x = -1; x < 2; x++)
                {
                    for (int y = -1; y < 2; y++)
                    {
                        // Ignore the current node at (0, 0)
                        if (x == 0 && y == 0)
                        {
                            continue;
                        }

                        // Ignore any nodes outside of the grid bounds
                        if (!grid.CheckBounds(new Vector2Int(currentNode.xPos + x, currentNode.yPos + y)))
                        {
                            continue;
                        }

                        // State the node as a neighbor
                        neighborNodes.Add(pathNodes[currentNode.xPos + x, currentNode.yPos + y]);
                    }
                }

                for(int i = 0; i < neighborNodes.Count; i++)
                {
                    // If the closed list already contains the neighbor node, ignore it
                    if (closedList.Contains(neighborNodes[i]))
                    {
                        continue;
                    }

                    // If the node is unpassable, then ignore it
                    if(!grid.CheckPassable(neighborNodes[i].xPos, neighborNodes[i].yPos))
                    {
                        continue;
                    }

                    float movementCost = currentNode.gValue + CalculateDistance(currentNode, neighborNodes[i]);
                    if (!openList.Contains(neighborNodes[i]) || movementCost < neighborNodes[i].gValue)
                    {
                        // Set the neighbor nodes gValue to the movement cost and the hValue to the distance between
                        // the neighbor node and the endnode
                        neighborNodes[i].gValue = movementCost;
                        neighborNodes[i].hValue = CalculateDistance(neighborNodes[i], endNode);

                        // Set the neighbor node's parentNode to the current node
                        neighborNodes[i].parentNode = currentNode;

                        // If the open list doesn't contain the neighbor node, then add it
                        if (!openList.Contains(neighborNodes[i]))
                        {
                            openList.Add(neighborNodes[i]);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Calculate the distance between two PathNodes
        /// </summary>
        /// <param name="startNode">The node to start at</param>
        /// <param name="endNode">The node to end at</param>
        /// <returns>The distance between the nodes</returns>
        private float CalculateDistance(PathNode startNode, PathNode endNode)
        {
            int distanceX = Math.Abs(startNode.xPos - endNode.xPos);
            int distanceY = Math.Abs(startNode.yPos - endNode.yPos);

            if(distanceX > distanceY)
            {
                return 14 * distanceY + 10 * (distanceX - distanceY);
            } else return 14 * distanceX + 10 * (distanceY - distanceX);
        }

        /// <summary>
        /// Retrace the path from the start node to the end node
        /// </summary>
        /// <param name="startNode">The PathNode to start at</param>
        /// <param name="endNode">The PathNode to end at</param>
        /// <returns>A path of nodes from the start node to the end node</returns>
        private List<PathNode> RetracePath(PathNode startNode, PathNode endNode)
        {
            // Create a path and set the current node to the end node
            List<PathNode> path = new List<PathNode>();
            PathNode currentNode = endNode;

            // Trace backwards to the start node and add the path
            while(currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parentNode;
            }

            // Reverse the path to get it in the correct order
            path.Reverse();

            // Return the path
            return path;
        }
    }
}

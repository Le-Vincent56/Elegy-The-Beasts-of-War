using Elegy.Events;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Elegy.Grid
{
    public class Movement : MonoBehaviour
    {
        [SerializeField] private GameEvent onUpdateGridPos;
        [SerializeField] private GridObject gridObject;
        [SerializeField] private GameEvent onGetWorldPath;
        [SerializeField] private float movementSpeed;

        public List<Vector3> pathWorldPositions = new List<Vector3>();

        private void Start()
        {
            gridObject = GetComponent<GridObject>();
        }

        // Update is called once per frame
        void Update()
        {
            // If the path is not set, then return
            if(pathWorldPositions == null || pathWorldPositions.Count == 0)
            {
                return;
            }

            // Move towards the next path node
            transform.position = Vector3.MoveTowards(transform.position, pathWorldPositions[0], movementSpeed * Time.deltaTime);

            // Remove the path node once close enough
            if(Vector3.Distance(transform.position, pathWorldPositions[0]) < 0.05f)
            {
                pathWorldPositions.RemoveAt(0);
            }
        }

        /// <summary>
        /// Move the character
        /// </summary>
        /// <param name="sender">The component raising the event</param>
        /// <param name="data">The MoveData to move accordingly with</param>
        public void Move(Component sender, object data)
        {
            // Check if the data is the correct type
            if(data is MoveData)
            {
                // Cast the data
                MoveData moveData = (MoveData)data;

                // If the target character is not this grid object, return
                if(moveData.targetCharacter != gridObject)
                {
                    return;
                }

                // Get the world position paths
                onGetWorldPath.Raise(this, moveData.path);

                // Set the grid object's position to the last position in the list
                gridObject.gridPos.x = moveData.path[moveData.path.Count - 1].xPos;
                gridObject.gridPos.y = moveData.path[moveData.path.Count - 1].yPos;

                // Update the gridObject's grid position
                onUpdateGridPos.Raise(this, moveData);
            }
        }
    }
}

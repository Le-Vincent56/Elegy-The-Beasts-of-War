using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Elegy.Events;
using Elegy.Characters;

namespace Elegy.Grid
{
    public struct MoveData
    {
        public GridObject targetCharacter;
        public List<PathNode> path;
        public MoveData(GridObject targetCharacter, List<PathNode> path)
        {
            this.targetCharacter = targetCharacter;
            this.path = path;
        }
    }

    public class MoveCharacter : MonoBehaviour
    {
        [SerializeField] private GameEvent onMoveCharacter;
        [SerializeField] private GameEvent onGridClicked;
        [SerializeField] private GameEvent onCheckWalkableTerrain;
        [SerializeField] private LayerMask terrainLayerMask;

        [SerializeField] private GridObject targetCharacter;
        private Pathfinding pathfinding;

        public Vector2Int clickedGridPosition;
        public List<PathNode> path;

        private void Start()
        {
            pathfinding = GetComponent<Pathfinding>();
        }

        /// <summary>
        /// Pathfind from the current object position to the clicked grid position and move
        /// </summary>
        /// <param name="context"></param>
        public void OnClick(InputAction.CallbackContext context)
        {
            if (context.started && targetCharacter != null)
            {
                // Get a ray from the mouse position
                Ray mouseRay = Camera.main.ScreenPointToRay(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y));
                RaycastHit hit;

                // Check if the ray hits terrain
                if (Physics.Raycast(mouseRay, out hit, float.MaxValue, terrainLayerMask))
                {
                    // Get a grid position from the hit point
                    onGridClicked.Raise(this, hit.point);

                    // Find the path between the current position to the new position
                    path = pathfinding.FindPath(targetCharacter.gridPos.x, targetCharacter.gridPos.y, clickedGridPosition.x, clickedGridPosition.y);

                    // If the path is null or there are no nodes, return
                    if(path == null || path.Count == 0)
                    {
                        return;
                    }

                    // Move the character
                    onMoveCharacter.Raise(this, new MoveData(targetCharacter, path));
                }
            }
        }

        /// <summary>
        /// Set the target character
        /// </summary>
        /// <param name="sender">The object raising the event</param>
        /// <param name="data">The target character to set</param>
        public void SetTargetCharacter(Component sender, object data)
        {
            // Check if the data type is correct
            if(data is GridObject)
            {
                // Set the target character
                targetCharacter = (GridObject)data;
            }

            // Set null if sent null
            if(data is null)
            {
                targetCharacter = null;
            }
        }
    }
}

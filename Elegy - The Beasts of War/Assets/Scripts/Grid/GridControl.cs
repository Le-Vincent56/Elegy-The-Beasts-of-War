using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Elegy.Events;
using Elegy.Characters;


namespace Elegy.Grid
{
    [RequireComponent(typeof(Grid))]
    [RequireComponent(typeof(Pathfinding))]
    public class GridControl : MonoBehaviour
    {
        [SerializeField] private GameEvent onGridHover;
        [SerializeField] private GameEvent onSetTargetCharacter;

        [SerializeField] private Grid targetGrid;
        [SerializeField] private Pathfinding pathfinding;
        [SerializeField] private LayerMask terrainLayerMask;
        [SerializeField] private GridObject hoveringOver;
        [SerializeField] private SelectableGridObject selectedObj;

        private void Start()
        {
            targetGrid = GetComponent<Grid>();
            pathfinding = GetComponent<Pathfinding>();
        }

        private void Update()
        {
            HoverOverGrid();
        }

        /// <summary>
        /// Hover over the grid
        /// </summary>
        public void HoverOverGrid()
        {
            // Get a ray from the mouse position
            Ray mouseRay = Camera.main.ScreenPointToRay(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y));
            RaycastHit hit;

            // Check if the ray hits terrain
            if (Physics.Raycast(mouseRay, out hit, float.MaxValue, terrainLayerMask))
            {
                // Get a grid position from the hit point
                Vector2Int clickedGridPosition = targetGrid.GetGridPosition(hit.point);
                onGridHover.Raise(this, clickedGridPosition);


                GridObject retrievedObject = targetGrid.GetPlacedObject(clickedGridPosition);
                hoveringOver = retrievedObject;
            }
        }

        public void CheckWalkableTerrain()
        {
            List<PathNode> walkableNodes = new List<PathNode>();
            pathfinding.CalculateWalkableNodes(
                selectedObj.GetComponent<GridObject>().gridPos.x,
                selectedObj.GetComponent<GridObject>().gridPos.y,
                selectedObj.GetComponent<Character>().movementPoints,
                ref walkableNodes);

            List<Vector2Int> walkableTiles = new List<Vector2Int>();
            for (int x = 0; x < targetGrid.width; x++)
            {
                for (int y = 0; y < targetGrid.length; y++)
                {
                    for (int i = 0; i < walkableNodes.Count; i++)
                    {
                        if (walkableNodes[i].xPos == x && walkableNodes[i].yPos == y)
                        {
                            walkableTiles.Add(new Vector2Int(x, y));
                        }
                    }
                }
            }


            targetGrid.UpdateGridHighlight(walkableTiles);
        }

        /// <summary>
        /// Select a selectable object when clicked
        /// </summary>
        /// <param name="context"></param>
        public void OnClick(InputAction.CallbackContext context)
        {
            if(context.started)
            {
                if(hoveringOver == null)
                {
                    return;
                }

                selectedObj = hoveringOver.GetComponent<SelectableGridObject>();
                CheckWalkableTerrain();


                // If the object can be selected, send this character to be selected
                if (selectedObj != null)
                {
                    onSetTargetCharacter.Raise(this, hoveringOver);

                    // Activate the grid mesh
                    targetGrid.ToggleGridHighlight(true);
                }
            }
        }

        /// <summary>
        /// Clear any selected objects
        /// </summary>
        /// <param name="context"></param>
        public void OnCancel(InputAction.CallbackContext context)
        {
            if(context.canceled)
            {
                if(selectedObj == null)
                {
                    return;
                }

                // Deselect the object
                selectedObj = null;
                onSetTargetCharacter.Raise(this, null);

                // Set the grid mesh to be invisible
                targetGrid.ToggleGridHighlight(false);
            }
        }
    }
}

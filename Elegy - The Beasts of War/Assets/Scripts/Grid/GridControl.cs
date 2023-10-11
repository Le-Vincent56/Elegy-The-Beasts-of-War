using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using Elegy.Events;
using Elegy.Characters;


namespace Elegy.Grid
{
    [RequireComponent(typeof(Grid))]
    public class GridControl : MonoBehaviour
    {
        [SerializeField] private GameEvent onGridHover;
        [SerializeField] private GameEvent onSetTargetCharacter;
        [SerializeField] private Grid targetGrid;
        [SerializeField] private LayerMask terrainLayerMask;
        [SerializeField] private GridObject hoveringOver;
        [SerializeField] private SelectableGridObject selectedObj;

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

                // If the object can be selected, send this character to be selected
                if(selectedObj != null)
                {
                    onSetTargetCharacter.Raise(this, hoveringOver);
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

                selectedObj = null;
                onSetTargetCharacter.Raise(this, null);
            }
        }
    }
}

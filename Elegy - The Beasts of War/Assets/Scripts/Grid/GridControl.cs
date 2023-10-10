using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using Elegy.Events;
using Elegy.Characters;

namespace Elegy.Grid
{
    public class GridControl : MonoBehaviour
    {
        [SerializeField] private GameEvent onGetGridPosition;
        [SerializeField] private GameEvent onRetrieveGridObject;
        [SerializeField] private Grid targetGrid;
        [SerializeField] private LayerMask terrainLayerMask;
        public Vector2Int gridPos;
        public GridObject highlightedObject;

        public void OnClick(InputAction.CallbackContext context)
        {
            if(context.started)
            {
                // Get a ray from the mouse position
                Ray mouseRay = Camera.main.ScreenPointToRay(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y));
                RaycastHit hit;

                // Check if the ray hits terrain
                if (Physics.Raycast(mouseRay, out hit, float.MaxValue, terrainLayerMask))
                {
                    // Get a grid position from the hit point
                    onGetGridPosition.Raise(this, hit.point);
                    onRetrieveGridObject.Raise(this, gridPos);
                    if (highlightedObject == null)
                    {
                        Debug.Log($"({gridPos.x}, {gridPos.y}) is empty");
                    }
                    else
                    {
                        Debug.Log($"({gridPos.x}, {gridPos.y}) contains {highlightedObject.GetComponent<Character>().characterName}");
                    }
                }
            }
        }
    }
}

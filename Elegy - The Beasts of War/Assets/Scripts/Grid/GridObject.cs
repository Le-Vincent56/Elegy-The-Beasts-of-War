using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elegy.Events;

public class GridObject : MonoBehaviour
{
    [SerializeField] private GameEvent onGetGridPosition;
    [SerializeField] private GameEvent onPlaceObject;
    [SerializeField] private GameEvent onGetWorldPosition;
    public Vector2Int gridPos;
    public Vector3 worldPos;

    private void Start()
    {
        Init();
    }

    /// <summary>
    /// Initialize the grid object
    /// </summary>
    private void Init()
    {
        onGetGridPosition.Raise(this, transform.position);
        onPlaceObject.Raise(this, this);
        onGetWorldPosition.Raise(this, gridPos);
    }
}

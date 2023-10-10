using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Elegy.Events;

public class GridObject : MonoBehaviour
{
    [SerializeField] private GameEvent onGetGridPosition;
    [SerializeField] private GameEvent onPlaceObject;
    public Vector2Int gridPos;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        onGetGridPosition.Raise(this, transform.position);
        onPlaceObject.Raise(this, this);
    }
}

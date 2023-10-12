using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Elegy.Grid
{
    public class Node
    {
        public bool passable;
        public GridObject gridObject;
        public float elevation;
        public bool selected;
        public bool walkable = false;
        public GameObject gridNode;
    }
}

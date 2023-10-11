using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAtStart : MonoBehaviour
{
    void Start()
    {
        // Destroy the gameobject
        Destroy(gameObject);   
    }
}

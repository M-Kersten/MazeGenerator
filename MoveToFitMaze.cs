using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToFitMaze : MonoBehaviour {

    private static MoveToFitMaze instance;
    public static MoveToFitMaze Instance { get { return instance; } }

    private void Awake()
    {
        // singleton pattern implementation
        // Check if another version of this static class is active in the scene
        if (instance != null && instance != this)
        {
            // If so then destroy it, THERE CAN BE ONLY ONE.
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public void Move(Vector3 moveToPosition)
    {
        transform.position = moveToPosition;
    }
}

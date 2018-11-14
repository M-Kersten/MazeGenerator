using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Script for moving a desktop or smartphone player
/// </summary>
public class PlayerMovement : MonoBehaviour {
    
    public Platform platform;
    public TrailRenderer trail;
    public float speed;

    // 0 = none, 1 = up, 2 = right, 3 = down, 4 = left
    [HideInInspector]
    public int direction;
    private Rigidbody rb;
    private Vector3 forceToAdd;
    [HideInInspector]
    public float scale;

    void Start ()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        direction = 0;
        float cellSize = MazeGenerator.Instance.settings.cellSize;
        if (trail != null)
        {
            trail.Clear();
            trail.widthMultiplier = cellSize * .1f;
        }
        PlacePlayerInMaze(cellSize);
    }

    private void PlacePlayerInMaze(float cellSize)
    {
        // the random position inside the maze
        Vector3 setPosition = new Vector3(
            Random.Range(0, MazeGenerator.Instance.settings.mazeWidth) * cellSize,
            1,
            Random.Range(0, MazeGenerator.Instance.settings.mazeLength) * cellSize) * scale;

        // the relative position the maze has changed in order to center it on the vuforia marker
        Vector3 minPosition = new Vector3(
            -(MazeGenerator.Instance.settings.mazeWidth * MazeGenerator.Instance.settings.cellSize) / 2,
            0,
            -(MazeGenerator.Instance.settings.mazeLength * MazeGenerator.Instance.settings.cellSize) / 2) * scale;
        
        transform.localPosition = setPosition + minPosition;
    }

    void FixedUpdate()
    {
        if (platform == Platform.desktop)
        {
            forceToAdd.x = Input.GetAxis("Horizontal");
            forceToAdd.z = Input.GetAxis("Vertical");
            rb.AddForce(forceToAdd);
        }
        else
        {
            switch (direction)
            {
                // move up
                case 1:
                    forceToAdd.x = 0;
                    forceToAdd.z = speed;
                    break;
                // move right
                case 2:
                    forceToAdd.z = 0;
                    forceToAdd.x = speed;
                    break;
                // move down
                case 3:
                    forceToAdd.x = 0;
                    forceToAdd.z = -speed;
                    break;
                // move left
                case 4:
                    forceToAdd.z = 0;
                    forceToAdd.x = -speed;
                        break;
                default:
                    break;
            }
            rb.AddForce(forceToAdd);
        }
    }
}

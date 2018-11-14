using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

[System.Serializable]
public enum Platform
{
    desktop,
    smartphone
}

/// <summary>
/// Script that manages desktop or smartphone player
/// </summary>
public class Player : MonoBehaviour, ITrackableEventHandler {

    public delegate void PlatformChanged();
    public PlatformChanged OnPlatformChanged;

    private Platform platform;
    public Platform Platform
    {
        get
        {
            return platform;
        }
        set
        {
            OnPlatformChanged();
            platform = value;
        }
    }

    public GameObject desktopPlayer;
    public GameObject phonePlayer;
        
    private GameObject currentPlayer;

    [Header("AR variables")]
    public GameObject ARMarker;
    public GameObject ARPlayer;    
    public TrackableBehaviour vuforiaTarget;
    public float markerScale;

    [Header("AR UI")]
    public GameObject lookingForMarker;
    public GameObject markerFound;

    private void Awake()
    {
        // set the platform to the current platform
#if UNITY_STANDALONE
        platform = Platform.desktop;
#endif
#if UNITY_IOS
            platform = Platform.smartphone;
#endif
#if UNITY_ANDROID
        platform = Platform.smartphone;
#endif
    }

    private void OnEnable()
    {        
        OnPlatformChanged += SetPlatform;

        // Connect to the vuforia marker to handle marker found and lost functionality
        vuforiaTarget.RegisterTrackableEventHandler(this);
        lookingForMarker.SetActive(true);
        markerFound.SetActive(false);
    }
    
    private void OnDisable()
    {
        OnPlatformChanged -= SetPlatform;
        if (platform == Platform.smartphone)
        {
            MazeGenerator.Instance.mazeHolder.SetActive(true);
        }        
    }

    public void EnablePlayer(bool enable)
    {
        gameObject.SetActive(enable);
        SetPlatform();
    }

    /// <summary>
    /// Spawn the maze according to the current working platform
    /// </summary>
    private void SetPlatform()
    {
        desktopPlayer.SetActive(false);
        phonePlayer.SetActive(false);

        switch (platform)
        {
            case Platform.desktop:
                desktopPlayer.SetActive(true);                
                break;
            case Platform.smartphone:
                SetToSmartPhone();
                phonePlayer.SetActive(true);
                break;
            default:
                break;
        }
    }
    
    private void SetToSmartPhone()
    {
        // put the maze on the vuforia marker
        MazeGenerator.Instance.mazeHolder.transform.SetParent(ARMarker.transform);
        if (currentPlayer)
        {
            Destroy(currentPlayer);
        }
        // spawn a new player inside the vuforia marker
        GameObject newARPlayer = Instantiate(ARPlayer, MazeGenerator.Instance.mazeHolder.transform);
        // make a reference to the newly spawned player
        currentPlayer = newARPlayer;
        currentPlayer.GetComponent<PlayerMovement>().scale = markerScale;
        // set to inactive until the vuforia marker has been found (this is done to pause physics calculations since vuforia ontrackerlost disables shperecolliders of children of the marker)
        currentPlayer.SetActive(false);
        MazeGenerator.Instance.mazeHolder.transform.localScale = new Vector3(markerScale, markerScale, markerScale);
        MazeGenerator.Instance.mazeHolder.transform.localPosition = new Vector3(-(MazeGenerator.Instance.settings.mazeWidth * MazeGenerator.Instance.settings.cellSize) / 2, 0, -(MazeGenerator.Instance.settings.mazeLength * MazeGenerator.Instance.settings.cellSize) / 2) * markerScale;
    }

    /// <summary>
    /// Move the player in a certain direction
    /// </summary>
    /// <param name="_direction">0 = none, 1 = up, 2 = right, 3 = down, 4 = left</param>
    public void SetDirection(int _direction)
    {
        if (currentPlayer != null)
        {
            currentPlayer.GetComponent<PlayerMovement>().direction = _direction;
        }       
    }

    #region Vuforia tracking

    public void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus)
    {
        if (newStatus == TrackableBehaviour.Status.TRACKED || newStatus == TrackableBehaviour.Status.DETECTED || newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
        {
            OnTrackingFound();
        }
        else
        {
            OnTrackingLost();
        }
    }

    private void OnTrackingFound()
    {
        if (currentPlayer != null)
        {
            currentPlayer.SetActive(true);
            lookingForMarker.SetActive(false);
            markerFound.SetActive(true);
        }
    }

    private void OnTrackingLost()
    {
        if (currentPlayer != null)
        {
            currentPlayer.SetActive(false);
            lookingForMarker.SetActive(true);
            markerFound.SetActive(false);
        }
    }
    #endregion
}

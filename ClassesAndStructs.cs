using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Collection of classes, structs and enums used in this project
/// </summary>
/// <remarks>
/// I prefer having these in a seperate namespace to avoid confusion with classnames 
/// and have a bit more freedom in choosing my own names.
/// </remarks>
namespace MazeGeneratorCollection
{
    [System.Serializable]
    public enum ActiveAlgorithm
    {
        RecursiveBacktracker,
        PrimsAlgorithm
    }

    /// <summary>
    /// sliders for maze settings in options menu
    /// </summary>
    [System.Serializable]
    public struct SettingSlider
    {
        public Slider slider;
        public InputField field;
    }

    // maze cell class
    public class Cell
    {
        public GameObject northWall, eastWall, southWall, westWall, floor;
        public bool visited = false;
    }

    /// <summary>
    /// cellposition in ints, similar to vector2int
    /// </summary>
    public struct CellPosition
    {
        public int x;
        public int y;
    }

    /// <summary>
    /// Page is used in the screenmanager to go through menuslides
    /// </summary>
    [Serializable]
    public struct Page
    {
        public string pageName;
        public List<GameObject> pageComponents;
    }
}
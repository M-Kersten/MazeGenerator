using UnityEngine;
using System.Reflection;
using System;
using MazeGeneratorCollection;

/// <summary>
/// Scriptable objects are more organised and persist through opening and exiting the application.
/// </summary>
[CreateAssetMenu(fileName = "GameSettings", menuName = "MazeGenerator/Game settings", order = 1)]
public class GameSettings : ScriptableObject
{
    #region Copy&Paste

    private static GameSettings copyingFrom;
    private static FieldInfo[] fields;

    /// <summary>
    /// The reflection library makes it easy to copy and paste certain values in unity editor
    /// </summary>
    [ContextMenu("Copy values")]
    public void CopyWithReflection()
    {
        copyingFrom = this;
        Type sphereType = typeof(GameSettings);
        FieldInfo[] sphereFields = sphereType.GetFields(BindingFlags.Public | BindingFlags.Instance);
        fields = sphereFields;
    }

    [ContextMenu("Paste values")]
    public void PasteWithReflection()
    {
        foreach (FieldInfo field in fields)
        {
            object value = field.GetValue(copyingFrom);
            field.SetValue(this, value);
        }
    }
    #endregion

    [Header("Maze settings")]
    public int mazeWidth;
    public int mazeLength;
    public float cellSize;
    public bool randomColors;
    // truly random colors tend to be ugly, a list to choose from is better
    public Color[] mazeColors;
    [Space(20)]

    // the algorithm building the maze
    public ActiveAlgorithm algorithm;
    
    [Header("Prefabs")]
    public GameObject wall;
    public GameObject floor;    
}

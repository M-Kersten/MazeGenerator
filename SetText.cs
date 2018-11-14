using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Set displaytext to starting algorithm
/// </summary>
/// <remarks>
/// Could be done with an event instead in a future update
/// </remarks>
public class SetText : MonoBehaviour {

    [SerializeField]
    private string recursiveName;
    [SerializeField]
    private string primName;

    private void Start()
    {
        SetTextNow();
    }

    public void SetTextNow()
    {
        switch (MazeGenerator.Instance.settings.algorithm)
        {
            case MazeGeneratorCollection.ActiveAlgorithm.RecursiveBacktracker:
                GetComponent<Text>().text = "Generate " + recursiveName + " maze";
                break;
            case MazeGeneratorCollection.ActiveAlgorithm.PrimsAlgorithm:
                GetComponent<Text>().text = "Generate " + primName + " maze";
                break;
            default:
                break;
        }
    }

}

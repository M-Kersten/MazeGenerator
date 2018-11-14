using MazeGeneratorCollection;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Since you can't delegate enums in unity buttons, this class will do it with the ActiveAlgorithm enum
/// </summary>
public class SetAlgorithmButton : MonoBehaviour {

    public ActiveAlgorithm algorithm;
    public SetText displayText;

    private string startingDisplayText;
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => SetAlgorithm());
    }

    private void OnDestroy()
    {
        button.onClick.RemoveListener(() => SetAlgorithm());
    }

    private void SetAlgorithm()
    {
        MazeOptions.Instance.SetAlgorithm(algorithm);
        displayText.SetTextNow();
    }

}

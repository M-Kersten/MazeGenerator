using MazeGeneratorCollection;
using UnityEngine;
using UnityEngine.UI;

public class MazeOptions : MonoBehaviour {
    private static MazeOptions instance;
    public static MazeOptions Instance { get { return instance; } }

    public SettingSlider widthSlider;
    public SettingSlider lengthSlider;
    public SettingSlider sizeSlider;
    public Toggle randomToggle;
    public GameObject playButton;

    private void Awake()
    {
        // singleton pattern implementation
        // Check if another version of this static class is active in the scene
        if (instance != null && instance != this)
        {
            // If so then destroy it
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }        
    }

    private void Start()
    {
        widthSlider.field.text = MazeGenerator.Instance.settings.mazeWidth.ToString();
        if (MazeGenerator.Instance.settings.mazeWidth <= widthSlider.slider.maxValue && MazeGenerator.Instance.settings.mazeWidth >= 0)
        {
            widthSlider.slider.value = MazeGenerator.Instance.settings.mazeWidth;
        }
        lengthSlider.field.text = MazeGenerator.Instance.settings.mazeLength.ToString();
        if (MazeGenerator.Instance.settings.mazeLength <= lengthSlider.slider.maxValue && MazeGenerator.Instance.settings.mazeLength >= 0)
        {
            lengthSlider.slider.value = MazeGenerator.Instance.settings.mazeLength;
        }
        sizeSlider.field.text = MazeGenerator.Instance.settings.cellSize.ToString();
        if (MazeGenerator.Instance.settings.cellSize <= sizeSlider.slider.maxValue && MazeGenerator.Instance.settings.cellSize >= 0)
        {
            sizeSlider.slider.value = MazeGenerator.Instance.settings.cellSize;
        }

        randomToggle.isOn = MazeGenerator.Instance.settings.randomColors;

        // connect sliders to variables
        lengthSlider.slider.onValueChanged.AddListener(delegate { SetMazeLength((int)lengthSlider.slider.value); });
        widthSlider.slider.onValueChanged.AddListener(delegate { SetMazeWidth((int)widthSlider.slider.value); });
        sizeSlider.slider.onValueChanged.AddListener(delegate { SetCellSize((int)sizeSlider.slider.value); });
        
        lengthSlider.field.onValueChanged.AddListener(delegate { SetMazeLength(int.Parse(lengthSlider.field.text)); });
        widthSlider.field.onValueChanged.AddListener(delegate { SetMazeWidth(int.Parse(widthSlider.field.text)); });
        sizeSlider.field.onValueChanged.AddListener(delegate { SetCellSize(int.Parse(sizeSlider.field.text)); });

        randomToggle.onValueChanged.AddListener(delegate { SetRandomColor(); });
    }

    #region setting of variables
    public void SetMazeWidth(int width)
    {
        playButton.SetActive(false);
        MazeGenerator.Instance.settings.mazeWidth = width;
        widthSlider.field.text = width.ToString();
        if (width < widthSlider.slider.maxValue && width >= 0)
        {
            widthSlider.slider.value = width;
        }
    }  

    public void SetMazeLength(int length)
    {
        playButton.SetActive(false);
        MazeGenerator.Instance.settings.mazeLength = length;
        lengthSlider.field.text = length.ToString();
        if (length < lengthSlider.slider.maxValue && length >= 0)
        {
            lengthSlider.slider.value = length;
        }
    }

    public void SetAlgorithm(ActiveAlgorithm algorithm)
    {
        MazeGenerator.Instance.settings.algorithm = algorithm;
    }

    public void SetCellSize(int cellSize)
    {
        playButton.SetActive(false);
        MazeGenerator.Instance.settings.cellSize = cellSize;
        sizeSlider.field.text = cellSize.ToString();
        if (cellSize < sizeSlider.slider.maxValue && cellSize >= 0)
        {
            sizeSlider.slider.value = cellSize;
        }
    }

    public void SetRandomColor()
    {
        MazeGenerator.Instance.settings.randomColors = randomToggle.isOn;
    }

    #endregion
}

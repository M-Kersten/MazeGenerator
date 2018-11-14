using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Place this class on a rect transform you want to slide in and out of a position
/// </summary>
public class SlideUI : MonoBehaviour {

    [Header("Set positions with contextmenus")]
    public Vector2 startPosition;
    public Vector2 endPosition;
    [Space(10)]
    [Header("Buttons that activate sliding")]
    public GameObject slideInButton;
    public GameObject slideOutButton;

    [Header("Slide movement")]
    public AnimationCurve movementEase;
    public float slideSpeed;

    //private variables
    private float timer;
    private bool movingIn = false;
    private bool movingOut = false;
    private RectTransform rt;

    #region init

    private void Start()
    {
        //set rt to gameobjects recttransform, this is an expensive function you don't want to call every update
        rt = GetComponent<RectTransform>();
    }
    #endregion
        
    void FixedUpdate()
    {
        if (movingIn)
        {
            if (timer < 1)
            {
                float currentPosition = movementEase.Evaluate(timer);
                rt.anchoredPosition = new Vector2(Mathf.Lerp(startPosition.x, endPosition.x, currentPosition), Mathf.Lerp(startPosition.y, endPosition.y, currentPosition));
                timer += Time.deltaTime * slideSpeed;
            }
            else
            {
                movingIn = false;
            }
        }
        else if (movingOut)
        {
            if (timer < 1)
            {
                float currentPosition = movementEase.Evaluate(timer);
                rt.anchoredPosition = new Vector2(Mathf.Lerp(endPosition.x, startPosition.x, currentPosition), Mathf.Lerp(endPosition.y, startPosition.y, currentPosition));
                timer += Time.deltaTime * slideSpeed;
            }
            else
	        {
                movingOut = false;
            }
        }
    }

    #region contextmenus

    [ContextMenu("Set start position")]
    public void SetStartPosition()
    {
        startPosition = rt.anchoredPosition;
    }

    [ContextMenu("Set end position")]
    public void SetEndPosition()
    {
        endPosition = rt.anchoredPosition;
    }

    [ContextMenu("Show start position")]
    public void ShowStartPosition()
    {
        rt.anchoredPosition = startPosition;
    }

    [ContextMenu("Show end position")]
    public void ShowEndPosition()
    {
        rt.anchoredPosition = endPosition;
    }
    #endregion

    #region slideFunctions

    /// <summary>
    /// Slide recttransform from startposition to endposition
    /// </summary>
    /// <param name="slidingIn">if false, slide from endposition to startposition</param>
    public void Slide(bool slidingIn)
    {
        SetSlideButtons(slidingIn);
        movingOut = !slidingIn;
        movingIn = slidingIn;
        timer = 0;
    }

    private void SetSlideButtons(bool slidingIn)
    {
        slideInButton.SetActive(!slidingIn);
        slideOutButton.SetActive(slidingIn);
    }
    #endregion   
}

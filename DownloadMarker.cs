using UnityEngine;

/// <summary>
/// Script for visiting the webpage that is hosting the vuforia marker
/// </summary>
public class DownloadMarker : MonoBehaviour {

	public void VisitDownloadMarker(string url)
    {
        Application.OpenURL(url);
    }
}

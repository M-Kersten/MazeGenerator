using MazeGeneratorCollection;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for managing UI menus
/// </summary>
public class ScreenManager : MonoBehaviour
{
    private static ScreenManager instance;
    public static ScreenManager Instance { get { return instance; } }

    public bool StartAtFirstScreen;
    public List<Page> pages = new List<Page>();

    private void Awake()
    {
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
        if (StartAtFirstScreen)
        {
            OpenPage(0);
        }
    }

    public void OpenPage(int page)
    {
        ResetActiveMenus();
        pages[page].pageComponents.ForEach(item => item.SetActive(true));
    }

    public void OpenPage(string page)
    {
        ResetActiveMenus();
        for (int i = 0; i < pages.Count; i++)
        {
            if (pages[i].pageName.ToLower() == page.ToLower())
            {
                pages[i].pageComponents.ForEach(item => item.SetActive(true));
            }
        }
    }

    public void ResetActiveMenus()
    {
        pages.ForEach(item => item.pageComponents.ForEach(subItem => subItem.SetActive(false)));
    }

    public void CloseApplication()
    {
        Application.Quit();
    }

}

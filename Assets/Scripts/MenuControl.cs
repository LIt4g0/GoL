using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuControl : MonoBehaviour
{
    [SerializeField] RectTransform menuPanel;
    [SerializeField] RectTransform showMenuButton;
    [SerializeField] RectTransform helpMenu;
    [SerializeField] TMPro.TMP_Text stable;
    [SerializeField] TMPro.TMP_Text generations;
    bool showMenu = true;
    bool showHelp = false;
    int prevPercent = 0;

    void Start()
    {
        //life = FindObjectOfType<Life>();
    }

    public void HideShowMenu()
    {
        showMenu = !showMenu;

        menuPanel.gameObject.SetActive(showMenu);
        showMenuButton.gameObject.SetActive(!showMenu);
    }
    public void HideShowHelp()
    {
        showHelp = !showHelp;

        helpMenu.gameObject.SetActive(showHelp);
    }
    public void SetStableText(bool inStable, int percentage)
    {
        if (percentage == prevPercent) return;
        string newString = " ";
        newString = percentage + "% of cells have stabilized";
        if (inStable) newString = "Simulation has stabilized!";

        stable.text = newString;
        prevPercent = percentage;
    }

    public void SetGenerationText(int generationIn)
    {
        generations.text = ""+generationIn;
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(0);
    }

    public void Quit()
    {
        Application.Quit();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuControl : MonoBehaviour
{
    [SerializeField] RectTransform menuPanel;
    [SerializeField] RectTransform showMenuButton;
    [SerializeField] RectTransform helpMenu;
    [SerializeField] TMPro.TMP_Text stable;
    //Life life;
    bool showMenu = true;
    bool showHelp = true;
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
        string newString = " ";
        newString = percentage + "% of cells have stabilized";
        if (inStable) newString = "Simulation has stabilized!";

        stable.text = newString;
        
    }
}

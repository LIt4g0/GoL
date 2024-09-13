using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuControl : MonoBehaviour
{
    [SerializeField] RectTransform menuPanel;
    [SerializeField] RectTransform showMenuButton;
    [SerializeField] RectTransform helpMenu;
    bool showMenu = true;
    bool showHelp = true;
    void Start()
    {
        
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
}

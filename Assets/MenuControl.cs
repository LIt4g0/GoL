using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuControl : MonoBehaviour
{
    [SerializeField] RectTransform menuPanel;
    bool showMenu = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void HideShowMenu()
    {
        showMenu = !showMenu;

        menuPanel.gameObject.SetActive(showMenu);
    }
}

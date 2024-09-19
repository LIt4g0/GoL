using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManager : MonoBehaviour
{
    [Header("Color sets")]
    public List<Color> aliveColors = new List<Color>();
    public List<Color> dyingColors = new List<Color>();
    public List<Color> stableColors = new List<Color>();
    public List<Color> oscilatingColors = new List<Color>();
    public List<Color> deadColors = new List<Color>();
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

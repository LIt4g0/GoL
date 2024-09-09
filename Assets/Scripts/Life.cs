//using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using Unity.Mathematics;
//using System.Numerics;
using UnityEngine;

public class Life : MonoBehaviour
{
    [SerializeField] GameObject refSquare;
    [SerializeField][Range(0.01f,1.0f)] float refreshRate = 1;
    float refreshTimer = 0;
    public Color aliveColor;
    public Color dyingColor;
    public Color stableColor;
    Texture2D refTexture;
    [SerializeField] Cell[,] xy;
    [SerializeField] int xArraySize = 1;
    [SerializeField] int yArraySize = 1;
    [SerializeField] int lifeChance = 1;
    bool hasChecked = false;
    [SerializeField] int stableCells = 0;
    [SerializeField] int unstableGenerations;
    [SerializeField] bool stable;
    [SerializeField] float gridGap = 0.1f;
    Vector2 clickPos;
    bool playing = false;
    Camera cam;
  
    void Start()
    {
        cam = Camera.main;
        Application.targetFrameRate = 60;
        unstableGenerations = 0;
        refTexture = refSquare.GetComponent<SpriteRenderer>().sprite.texture;
        refSquare.SetActive(false);
        xy = new Cell[xArraySize,yArraySize];
        
        bool toLive = false;
        for (int x = 0; x < xArraySize; x++)
        {
            for (int y = 0; y < yArraySize; y++)
            {
                var newO = new GameObject("Cell :" + x + ", " + y);
                SpriteRenderer newOSprite = newO.AddComponent<SpriteRenderer>();
                newOSprite.sprite = Sprite.Create(refTexture, new Rect(0,0,100-gridGap,100-gridGap),new Vector3(0.5f,0.5f,0.5f));
                newOSprite.sortingOrder = - 1;
                if (Random.Range(0,100) < lifeChance) toLive = true;
                    else toLive = false;

                xy[x,y] = new Cell(newO.transform, -xArraySize*0.5f + x,-yArraySize*0.5f + y,toLive, newOSprite, this);
            }
        }
    }

    void Update()
    {

        cam.orthographicSize -= Input.GetAxisRaw("Mouse ScrollWheel")*5f;

        if (Input.GetButtonDown("Jump"))
        {
            playing = !playing;
        }

        if (playing) refreshTimer -= Time.deltaTime;

        if (!hasChecked && refreshTimer < 0)
        {
            CheckCells();
        }

        if (hasChecked)
        {
            UpdateCells();
        }

        if (Input.GetButton("Fire1"))
        {
            FireCell();
        }

        if (Input.GetButtonDown("Fire2"))
        {
            FireAcorn();
        }

    }



    void CheckAndSetLife(int xIn, int yIn)
    {
        if (xIn >= 0 && xIn < xArraySize)
        {
            if (yIn >= 0 && yIn < yArraySize)
            {
                xy[xIn,yIn].SetLife(true);
            }
        }
    }

    private void GetClickPosToGrid(out int xPos, out int yPos)
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        //Debug.DrawLine(clickPos, mousePos, Color.red);

        float xClick = Mathf.Clamp(mousePos.x + (xArraySize * .5f), 0, xArraySize - 1);
        float yClick = Mathf.Clamp(mousePos.y + (yArraySize * .5f), 0, yArraySize - 1);
        xPos = Mathf.RoundToInt(xClick);
        yPos = Mathf.RoundToInt(yClick);
    }


    private void CheckCells()
    {
        //Debug.Log("//Do cycle of checks");
        hasChecked = true;
        for (int x = 0; x < xy.GetLength(0); x++)
        {
            for (int y = 0; y < xy.GetLength(1); y++)
            {
                Cell thisCell = xy[x, y];
                int xPos = x;
                int yPos = y;
                int aliveNeighbours = 0;

                bool left = (xPos == 0); //Debug.Log("I am all the way to the left");
                bool right = (xPos == xArraySize - 1);// Debug.Log("I am all the way to the right");
                bool bottom = (yPos == 0);// Debug.Log("I am all the way in the bottom");
                bool top = (yPos == yArraySize - 1);// Debug.Log("I am all the way in the top");

                //below checks
                if (!bottom && xy[x, y - 1].alive) aliveNeighbours++;// Debug.Log("Has alive neighbour below");
                if (!bottom && !left && xy[x - 1, y - 1].alive) aliveNeighbours++;// Debug.Log("Has alive neighbour below left");
                if (!bottom && !right && xy[x + 1, y - 1].alive) aliveNeighbours++;// Debug.Log("Has alive neighbour below left");
                //above checks
                if (!top && xy[x, y + 1].alive) aliveNeighbours++;//Debug.Log("Has alive neighbour above");
                if (!top && !left && xy[x - 1, y + 1].alive) aliveNeighbours++;// Debug.Log("Has alive neighbour top left");
                if (!top && !right && xy[x + 1, y + 1].alive) aliveNeighbours++;// Debug.Log("Has alive neighbour top left");
                //left check
                if (!left && xy[x - 1, y].alive) aliveNeighbours++;//Debug.Log("Has alive neighbour to the left");
                //right check
                if (!right && xy[x + 1, y].alive) aliveNeighbours++;//Debug.Log("Has alive neighbour to the right");

                thisCell.aliveNeighbours = aliveNeighbours;
            }
        }
    }

    private void UpdateCells()
    {

        stableCells = 0;
        for (int x = 0; x < xy.GetLength(0); x++)
        {
            for (int y = 0; y < xy.GetLength(1); y++)
            {

                Cell thisCell = xy[x, y];
                bool alive = thisCell.alive;
                bool setAlive = alive;
                int neighbours = thisCell.aliveNeighbours;
                if (alive && neighbours < 2)
                {
                    //Debug.Log("Die due to loneliness");
                    setAlive = false;
                }
                if (alive && neighbours >= 2)
                {
                    //Debug.Log("Live on");
                }
                if (!alive && neighbours == 3)
                {
                    //Debug.Log("Come to life");
                    setAlive = true;
                }
                if (alive && neighbours > 3)
                {
                    //Debug.Log("Die due to overpopulation on");
                    setAlive = false;
                }

                thisCell.SetLife(setAlive);
                thisCell.aliveNeighbours = 0;
                if (thisCell.stableGenerations > 1)
                {
                    stableCells ++;
                }
            }
        }
        
        if (stableCells == xArraySize*yArraySize)
        {
            stable = true;
        }
        else
        {
            stable = false;
            unstableGenerations ++;
        }
        
        refreshTimer = refreshRate;
        hasChecked = false;
    
    }
    private void FireCell()
    {
        int xPos, yPos;
        GetClickPosToGrid(out xPos, out yPos);
        xy[xPos, yPos].SetLife(true);
    }

    private void FireAcorn()
    {
        int xPos, yPos;
        GetClickPosToGrid(out xPos, out yPos);
        xy[xPos, yPos].SetLife(true);
        //Spawn acorn
        CheckAndSetLife(xPos - 2, yPos + 1);
        CheckAndSetLife(xPos - 2, yPos - 1);
        CheckAndSetLife(xPos - 3, yPos - 1);
        CheckAndSetLife(xPos + 1, yPos - 1);
        CheckAndSetLife(xPos + 2, yPos - 1);
        CheckAndSetLife(xPos + 3, yPos - 1);
    }
}

public class Cell
{
    public Vector2 position;
    Life life;
    Transform transform;
    public bool alive;
    bool prevState;
    SpriteRenderer spriteRenderer;
    public int aliveNeighbours;
    public int stableGenerations = 0;

    public Cell(Transform artHolderGameObject, float x, float y, bool live, SpriteRenderer spriteRendererIn, Life lifeClass)
    {
        position = new Vector2(x, y);
        transform = artHolderGameObject;
        transform.position += (Vector3)position;
        spriteRenderer = spriteRendererIn;
        life = lifeClass;
        SetLife(live);

    }

    public void SetLife(bool lifeIn)
    {
        if (lifeIn == alive || lifeIn == prevState)
        {
            stableGenerations ++;
            prevState = alive;
        }
        else
        {
            prevState = alive;
            stableGenerations = 0;
        }



        if (lifeIn)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = life.aliveColor;
            if (stableGenerations > 1)
                spriteRenderer.color = life.stableColor;
        }
        
        if (alive != lifeIn && !lifeIn)
        {
            spriteRenderer.color = life.dyingColor;
        }

        if (stableGenerations > 1 && lifeIn)
        {
            spriteRenderer.color = life.stableColor;
        }

        if (alive == lifeIn && !lifeIn)
        {
            spriteRenderer.enabled = false;
        }

        
        // if (lifeIn)
        // {
        //     spriteRenderer.enabled = true;
        // }
        // else
        // {
        //     spriteRenderer.enabled = false;
        // }
        alive = lifeIn;
    }
}
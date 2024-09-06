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
    [SerializeField] float scaler = 1;
    [SerializeField][Range(0.01f,1.0f)] float refreshRate = 1;
    float refreshTimer = 0;

    //float height;
    //float width;
    Texture2D refTexture;
    Vector3 startPos;
    // [SerializeField] bool[][] xy2 =
    // {
    //     new bool[50], new bool [50]
    // };
    [SerializeField] Cell[,] xy;
    [SerializeField] int xArraySize = 1;
    [SerializeField] int yArraySize = 1;
    [SerializeField] int lifeChance = 1;
    bool hasChecked = false;
    [SerializeField] int stableCells = 0;
    [SerializeField] float gridGap = 0.1f;
    Vector2 clickPos;
    bool playing = false;
  
    void Start()
    {
        Application.targetFrameRate = 60;
        //Camera cam = Camera.main;
        //height = cam.orthographicSize;
        //width = height * cam.aspect;
        //startPos = new Vector3(-width*2 + width, -height*2 + height,0);
        refTexture = refSquare.GetComponent<SpriteRenderer>().sprite.texture;
        refSquare.SetActive(false);
        startPos = new Vector3();
        xy = new Cell[xArraySize,yArraySize];
        
   
        bool toLive = false;
        for (int x = 0; x < xArraySize; x++)
        {
            for (int y = 0; y < yArraySize; y++)
            {
                var newO = new GameObject("Cell :" + x + ", " + y);
                newO.transform.position = startPos;
                SpriteRenderer newOSprite = newO.AddComponent<SpriteRenderer>();
                newOSprite.sprite = Sprite.Create(refTexture, new Rect(0,0,scaler*100,scaler*100),new Vector3(0.5f,0.5f,0.5f));
                newOSprite.color = Color.white;
                newOSprite.sortingOrder = - 1;
                if (Random.Range(0,100) < lifeChance) toLive = true;
                    else toLive = false;

                xy[x,y] = new Cell(newO.transform, ((-xArraySize*0.5f) + x+(gridGap*x))*scaler,((-yArraySize*0.5f) + y+(gridGap*y))*scaler,toLive, newOSprite);
            }
        }
    }

    void Update()
    {
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
            refreshTimer = refreshRate;
        }

        if (Input.GetButtonDown("Fire1"))
        {
            clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetButton("Fire1"))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.DrawLine(clickPos,mousePos,Color.red);
            Debug.Log(mousePos.x);
            // int mouseIntX;
            // int mouse
            // mousePos.x -= Mathf.RoundToInt(gridGap*mousePos.x);
            // mousePos.y -= Mathf.RoundToInt(gridGap*mousePos.y);
            Debug.Log(mousePos.x);
            int xClick = Mathf.Clamp(Mathf.RoundToInt(mousePos.x + (xArraySize*.5f)), 0, xArraySize-1);
            int yClick = Mathf.Clamp(Mathf.RoundToInt(mousePos.y + (yArraySize*.5f)), 0, yArraySize-1);

            //Debug.Log(xClick);

            xy[xClick,yClick].SetLife(true);

        }

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
                //bool amIAlive = thisCell.alive;
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
                //Debug.Log("Alive neighbours: " + aliveNeighbours);
            }
        }
    }

    private void UpdateCells()
    {
        stableCells = 0;
        hasChecked = false;
        //Debug.Log("//Apply changes");
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
                    //thisCell.Die();
                    setAlive = false;
                }
                if (alive && neighbours >= 2)
                {
                    //Debug.Log("Live on");
                }
                if (!alive && neighbours == 3)
                {
                    //Debug.Log("Come to life");
                    //thisCell.Live();
                    setAlive = true;
                }
                if (alive && neighbours > 3)
                {
                    //Debug.Log("Die due to overpopulation on");
                    //thisCell.Die();
                    setAlive = false;
                }

                thisCell.SetLife(setAlive);
                if (thisCell.stableGenerations > 1)
                {
                    stableCells ++;
                }
                thisCell.aliveNeighbours = 0;
            }
        }
    }
}

public class Cell
{
    public Vector2 position;
    Transform transform;
    public bool alive;
    bool prevState;
    SpriteRenderer spriteRenderer;
    public int aliveNeighbours;
    public int stableGenerations = 0;

    public Cell(Transform artHolderGameObject, float x, float y, bool live, SpriteRenderer spriteRendererIn)
    {
        //Set our position when we create the code.
        position = new Vector2(x, y);
        transform = artHolderGameObject;
        transform.position += (Vector3)position;
        spriteRenderer = spriteRendererIn;

        SetLife(live);
        //stableGenerations -= 1;

    }

    public void UpdatePos(Vector3 posIn)
    {
        //Update position
        position = posIn;

        //Send new position to the art game object.
        transform.position = position;
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

        alive = lifeIn;

        // if (alive) spriteRenderer.color = Color.black;
        //     else spriteRenderer.color = Color.white;

        if (alive) spriteRenderer.enabled = true;
            else spriteRenderer.enabled = false;
        
    }
}
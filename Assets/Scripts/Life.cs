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

    float height;
    float width;
    // float scaleX;
    // float scaleY;s
    Texture2D refTexture;
    Vector3 startPos;
    // [SerializeField] bool[][] xy2 =
    // {
    //     new bool[50], new bool [50]
    // };
    [SerializeField] Cell[,] xy;
    [SerializeField] int xArraySize = 1;
    [SerializeField] int yArraySize = 1;
    bool hasChecked = false;
  

    void Start()
    {
        Application.targetFrameRate = 60;
        Camera cam = Camera.main;
        height = cam.orthographicSize;
        width = height * cam.aspect;
        // scaleX = refSquare.transform.localScale.x*0.5f;
        // scaleY = refSquare.transform.localScale.y*0.5f;
        //refSquare.transform.position = new Vector3(-width, -height,0);
        startPos = new Vector3(-width, -height,0);
        refTexture = refSquare.GetComponent<SpriteRenderer>().sprite.texture;
        refSquare.SetActive(false);

        xy = new Cell[xArraySize,yArraySize];
   
        bool toLive = false;
        for (int x = 0; x < xArraySize; x++)
        {
            for (int y = 0; y < yArraySize; y++)
            {
                var newO = new GameObject("Cell :" + x + ", " + y);
                newO.transform.position = startPos;
                SpriteRenderer newOSprite = newO.AddComponent<SpriteRenderer>();
                newOSprite.sprite = Sprite.Create(refTexture, new Rect(0,0,scaler*95,scaler*95),new Vector3(0.0f,0.0f,0.5f));
                newOSprite.color = Color.white;
                newOSprite.sortingOrder = - 1;
                if (Random.Range(0,2) > 0) toLive = true;
                    else toLive = false;

                xy[x,y] = new Cell(newO.transform, x*scaler,y*scaler,toLive, newOSprite);
            }
        }
    }

    void Update()
    {
        refreshTimer -= Time.deltaTime;
        if (!hasChecked && refreshTimer < 0)
        {
            CheckCells();
        }

        if (hasChecked)
        {
            UpdateCells();
            refreshTimer = refreshRate;
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
        hasChecked = false;
        //Debug.Log("//Apply changes");
        for (int x = 0; x < xy.GetLength(0); x++)
        {
            for (int y = 0; y < xy.GetLength(1); y++)
            {
                Cell thisCell = xy[x, y];
                bool alive = thisCell.alive;
                int neighbours = thisCell.aliveNeighbours;
                if (alive && neighbours < 2)
                {
                    Debug.Log("Die due to loneliness");
                    thisCell.Die();
                }
                if (alive && neighbours >= 2)
                {
                    Debug.Log("Live on");
                }
                if (!alive && neighbours == 3)
                {
                    Debug.Log("Come to life");
                    thisCell.Live();
                }
                if (alive && neighbours > 3)
                {
                    Debug.Log("Die due to overpopulation on");
                    thisCell.Die();
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
    SpriteRenderer spriteRenderer;
    public int aliveNeighbours;

    //Ball Constructor, called when we type new Ball(x, y);
    public Cell(Transform artHolderGameObject, float x, float y, bool live, SpriteRenderer spriteRendererIn)
    {
        //Set our position when we create the code.
        position = new Vector2(x, y);
        transform = artHolderGameObject;
        transform.position += (Vector3)position;
        alive = live;
        spriteRenderer = spriteRendererIn;

        if (alive)
        {
            Live();
        }
        else
        {
            Die();
        }

    }

    public void UpdatePos(Vector3 posIn)
    {
        //Update position
        position = posIn;

        //Send new position to the art game object.
        transform.position = position;
    }

    public void Die()
    {
        alive = false;
        spriteRenderer.color = Color.white;
    }

    public void Live()
    {
        alive = true;
        spriteRenderer.color = Color.black;
    }
}
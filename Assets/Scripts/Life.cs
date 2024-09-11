//using System;
//using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using System.Numerics;
using UnityEngine;

public class Life : MonoBehaviour
{
    [SerializeField] GameObject cellPrefab;
    [SerializeField][Range(0.01f,1.0f)] float refreshRate = 1;
    float refreshTimer = 0;
    public Color aliveColor;
    public Color dyingColor;
    public Color stableColor;
    public Color oscilatingColor;
    Texture2D refTexture;
    [SerializeField] Cell[,] xy;
    [SerializeField] int xArraySize = 1;
    [SerializeField] int yArraySize = 1;
    [SerializeField] int lifeChance = 1;
    bool hasChecked = false;
    [SerializeField] int stableCells = 0;
    [SerializeField] int unstableGenerations;
    [SerializeField] bool stable;
    [SerializeField] bool wrapping;
    [SerializeField] float gridGap = 0.1f;
    bool playing = false;
    Camera cam;
    

    void Start()
    {
        cam = Camera.main;
        Application.targetFrameRate = 60;
        unstableGenerations = 0;
        refTexture = cellPrefab.GetComponent<SpriteRenderer>().sprite.texture;
        cellPrefab.SetActive(false);
        
        xy = new Cell[xArraySize,yArraySize];
        
        bool toLive = false;
        for (int x = 0; x < xArraySize; x++)
        {
            for (int y = 0; y < yArraySize; y++)
            {
                Vector2 newPos = new Vector2(-xArraySize*0.5f + x,-yArraySize*0.5f + y);
                var newO = Instantiate(cellPrefab, newPos, Quaternion.identity);
                newO.SetActive(true);
                if (Random.Range(0,100) < lifeChance) toLive = true;
                    else toLive = false;

                xy[x,y] = newO.GetComponent<Cell>();
                xy[x,y].SetStartInfo(toLive,this);
            }
        }
    }

    void Update()
    {

        cam.orthographicSize -= Input.GetAxisRaw("Mouse ScrollWheel")*5f;

        if (Input.GetButton("Fire3"))
        {
            Vector3 pan = Vector3.zero;
            pan.x = -Input.GetAxis("Mouse X");
            pan.y = -Input.GetAxis("Mouse Y");
            cam.transform.position += pan;
        }

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
            //FireAcorn();
            FirePulsar();
            //FirePenta();
        }

        if (Input.GetKeyDown("backspace"))
        {
            ResetGrid();
        }
    }

    private void ResetGrid()
    {
        for (int x = 0; x < xy.GetLength(0); x++)
        {
            for (int y = 0; y < xy.GetLength(1); y++)
            {
                xy[x, y].SetLife(false);
            }
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

        float xClick = Mathf.Clamp(mousePos.x + (xArraySize * .5f), 0, xArraySize - 1);
        float yClick = Mathf.Clamp(mousePos.y + (yArraySize * .5f), 0, yArraySize - 1);
        xPos = Mathf.RoundToInt(xClick);
        yPos = Mathf.RoundToInt(yClick);
    }


    private void CheckCells()
    {
        hasChecked = true;
        for (int x = 0; x < xy.GetLength(0); x++)
        {
            for (int y = 0; y < xy.GetLength(1); y++)
            {
                int aliveNeighbours = 0;
                if (wrapping)
                {
                    aliveNeighbours = CheckNeighboursWrapping(x, y, aliveNeighbours);
                }
                else
                {
                    aliveNeighbours = CheckNeighbours(x, y, aliveNeighbours);
                }

                xy[x, y].aliveNeighbours = aliveNeighbours;

            }
        }
    }

    private int CheckNeighboursWrapping(int x, int y, int aliveNeighbours)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {

                if (i == 0 && j == 0) continue;

                int nx = (x + i + xArraySize) % xArraySize;
                int ny = (y + j + yArraySize) % yArraySize;
                if (xy[nx, ny].alive) aliveNeighbours++;
            }
        }

        return aliveNeighbours;
    }

    private int CheckNeighbours(int x, int y, int aliveNeighbours)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                // Skip the current cell
                if (i == 0 && j == 0) continue;

                // Check if the neighboring cell is within the grid bounds
                int nx = x + i;
                int ny = y + j;

                if (nx >= 0 && nx < xArraySize && ny >= 0 && ny < yArraySize)
                {
                    // Count if the neighboring cell is alive
                    if (xy[nx, ny].alive) aliveNeighbours++;
                }
            }
        }

        return aliveNeighbours;
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

    private void FirePenta()
    {
        int xPos, yPos;
        GetClickPosToGrid(out xPos, out yPos);
        xy[xPos, yPos].SetLife(false);
        //LeftWall
        CheckAndSetLife(xPos - 2, yPos +2);
        CheckAndSetLife(xPos - 2, yPos +1);
        CheckAndSetLife(xPos - 2, yPos +0);
        CheckAndSetLife(xPos - 2, yPos -1);
        CheckAndSetLife(xPos - 2, yPos -2);
        CheckAndSetLife(xPos - 2, yPos -3);

        //RightWall
        CheckAndSetLife(xPos + 2, yPos +2);
        CheckAndSetLife(xPos + 2, yPos +1);
        CheckAndSetLife(xPos + 2, yPos +0);
        CheckAndSetLife(xPos + 2, yPos -1);
        CheckAndSetLife(xPos + 2, yPos -2);
        CheckAndSetLife(xPos + 2, yPos -3);

        //Top
        CheckAndSetLife(xPos -1, yPos +3);
        CheckAndSetLife(xPos +0, yPos +4);
        CheckAndSetLife(xPos +1, yPos +3);

        //Bottom
        CheckAndSetLife(xPos -1, yPos -4);
        CheckAndSetLife(xPos +0, yPos -5);
        CheckAndSetLife(xPos +1, yPos -4);
    }

    private void FirePulsar()
    {
        int xPos, yPos;
        GetClickPosToGrid(out xPos, out yPos);
        xy[xPos, yPos].SetLife(false);
        //bottom left pulsar
        CheckAndSetLife(xPos - 1, yPos - 2);
        CheckAndSetLife(xPos - 1, yPos - 3);
        CheckAndSetLife(xPos - 1, yPos - 4);

        CheckAndSetLife(xPos - 6, yPos - 2);
        CheckAndSetLife(xPos - 6, yPos - 3);
        CheckAndSetLife(xPos - 6, yPos - 4);

        CheckAndSetLife(xPos - 2, yPos - 1);
        CheckAndSetLife(xPos - 3, yPos - 1);
        CheckAndSetLife(xPos - 4, yPos - 1);

        CheckAndSetLife(xPos - 2, yPos - 6);
        CheckAndSetLife(xPos - 3, yPos - 6);
        CheckAndSetLife(xPos - 4, yPos - 6);

        //top left pulsar
        CheckAndSetLife(xPos - 1, yPos + 2);
        CheckAndSetLife(xPos - 1, yPos + 3);
        CheckAndSetLife(xPos - 1, yPos + 4);

        CheckAndSetLife(xPos - 6, yPos + 2);
        CheckAndSetLife(xPos - 6, yPos + 3);
        CheckAndSetLife(xPos - 6, yPos + 4);

        CheckAndSetLife(xPos - 2, yPos + 1);
        CheckAndSetLife(xPos - 3, yPos + 1);
        CheckAndSetLife(xPos - 4, yPos + 1);

        CheckAndSetLife(xPos - 2, yPos + 6);
        CheckAndSetLife(xPos - 3, yPos + 6);
        CheckAndSetLife(xPos - 4, yPos + 6);

        //bottom right pulsar
        CheckAndSetLife(xPos +1, yPos - 2);
        CheckAndSetLife(xPos +1, yPos - 3);
        CheckAndSetLife(xPos +1, yPos - 4);

        CheckAndSetLife(xPos +6, yPos - 2);
        CheckAndSetLife(xPos +6, yPos - 3);
        CheckAndSetLife(xPos +6, yPos - 4);

        CheckAndSetLife(xPos +2, yPos - 1);
        CheckAndSetLife(xPos +3, yPos - 1);
        CheckAndSetLife(xPos +4, yPos - 1);

        CheckAndSetLife(xPos +2, yPos - 6);
        CheckAndSetLife(xPos +3, yPos - 6);
        CheckAndSetLife(xPos +4, yPos - 6);

        //top right pulsar
        CheckAndSetLife(xPos +1, yPos + 2);
        CheckAndSetLife(xPos +1, yPos + 3);
        CheckAndSetLife(xPos +1, yPos + 4);

        CheckAndSetLife(xPos +6, yPos + 2);
        CheckAndSetLife(xPos +6, yPos + 3);
        CheckAndSetLife(xPos +6, yPos + 4);

        CheckAndSetLife(xPos +2, yPos + 1);
        CheckAndSetLife(xPos +3, yPos + 1);
        CheckAndSetLife(xPos +4, yPos + 1);

        CheckAndSetLife(xPos +2, yPos + 6);
        CheckAndSetLife(xPos +3, yPos + 6);
        CheckAndSetLife(xPos +4, yPos + 6);
    }
}

// public class Cell
// {
//     public Vector2 position;
//     Life life;
//     Transform transform;
//     public bool alive;
//     List<bool> prevStates = new List<bool>(){false,false,false};
//     List<bool> secondStates= new List<bool>();
//     List<bool> firstStates= new List<bool>();

//     SpriteRenderer spriteRenderer;
//     public int aliveNeighbours;
//     public int stableGenerations = 0;
//     public int deadCount = 0;
//     bool oscilating = false;

//     public Cell(Transform artHolderGameObject, bool live, SpriteRenderer spriteRendererIn, Life lifeClass)
//     {
//         //position = new Vector2(x, y);
//         transform = artHolderGameObject;
//         //transform.position += (Vector3)position;
//         spriteRenderer = spriteRendererIn;
//         life = lifeClass;
//         SetLife(live);
//     }

//     public void SetLife(bool lifeIn)
//     {
//         if (!lifeIn) deadCount ++;
//             else deadCount = 0;
        
//         prevStates.Insert(0,lifeIn);
//         bool thisLifeStability = false;
//         //clean list at random points to separate performance costs
//         if (prevStates.Count > Random.Range(100,250))
//         {
//             prevStates.RemoveRange(50,prevStates.Count()-50);
//             //Debug.Log("Wiped end of lists");
//         }
    

//         // store relvenat states in new list, compare lists:
        
//         int checkLength = 3;
//         //Debug.Log(checkLength);
//         if (prevStates.Count > checkLength*2 && deadCount < 20)
//         {
//             firstStates = new List<bool>(prevStates.GetRange(0, checkLength));
//             secondStates = new List<bool>(prevStates.GetRange(checkLength,checkLength));
//             // Attempt to check
//             if (secondStates.SequenceEqual(firstStates))
//             {
//                 thisLifeStability = true;
//             }
//             else
//             {
//                 checkLength = 15;
//                 if (prevStates.Count > checkLength*2 && deadCount < 20)
//                 {
//                     firstStates = new List<bool>(prevStates.GetRange(0, checkLength));
//                     secondStates = new List<bool>(prevStates.GetRange(checkLength,checkLength));
//                     // Attempt to check
//                     if (secondStates.SequenceEqual(firstStates))
//                     {
//                         thisLifeStability = true;
//                     }
//                 }
//             }
//         }


//         // Old system with 1 lookback for stability
//         if (lifeIn == alive)
//         {
//             thisLifeStability = true;
//         }

//         if (lifeIn == prevStates[2] && lifeIn != prevStates[1])
//         {
//             thisLifeStability = true;
//             oscilating = true;
//         }
//         else
//         {
//             oscilating = false;
//         }


//         if (thisLifeStability) stableGenerations++;
//             else stableGenerations = 0;

//         //Coloration and more
//         if (lifeIn)
//         {
//             spriteRenderer.enabled = true;
//             spriteRenderer.color = life.aliveColor;
//             if (lifeIn == prevStates[1])
//                 spriteRenderer.color = life.stableColor;
//         }
        
//         if (alive != lifeIn && !lifeIn)
//         {
//             spriteRenderer.color = life.dyingColor;
//         }

//         if (oscilating)
//         {
//             spriteRenderer.color = life.oscilatingColor;
//         }
        
//         if (alive == lifeIn && !lifeIn)
//         {
//             spriteRenderer.enabled = false;
//         }

//         //primitive on off:
//         if (lifeIn)
//         {
//             spriteRenderer.enabled = true;
//         }
//         else
//         {
//             spriteRenderer.enabled = false;
//         }

        
//         alive = lifeIn;
//     }
// }
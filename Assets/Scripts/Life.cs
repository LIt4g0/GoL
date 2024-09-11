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
    [SerializeField] Cell[,] xy;
    [SerializeField] int columnsMax = 1;
    [SerializeField] int rowsMax = 1;
    [SerializeField] int columnsTarget = 1;
    [SerializeField] int rowsTarget = 1;
    [SerializeField] int lifeChance = 1;
    bool hasChecked = false;
    [SerializeField] int stableCells = 0;
    [SerializeField] int unstableGenerations;
    [SerializeField] bool stable;
    [SerializeField] bool wrapping;
    [SerializeField] float gridGap = 0.1f;
    bool playing = false;
    Camera cam;
    [SerializeField]int columnsSpawned;
    [SerializeField]int rowsSpawned;
    

    void Start()
    {
        cam = Camera.main;
        Application.targetFrameRate = 60;
        unstableGenerations = 0;
        
        xy = new Cell[columnsMax,rowsMax];
        //columnsTarget = columnsMax;
        //rowsTarget = rowsMax;


        for (int x = 0; x < columnsTarget; x++)
        {
            for (int y = 0; y < rowsTarget; y++)
            {
                SpawnCell(x, y);
            }
        }
        columnsSpawned = columnsTarget;
        rowsSpawned = rowsTarget;
        cam.transform.position = new Vector3((columnsSpawned-1)*0.5f,(rowsSpawned-1)*0.5f,cam.transform.position.z);
    }

    private void SpawnCell(int x, int y)
    {
        if (xy[x,y] != null)
        {
            Debug.Log("Exists already");
            return;
        } 
        bool toLive = false;
        Vector2 newPos = new Vector2(x, y);
        var newO = Instantiate(cellPrefab, newPos, Quaternion.identity);
        if (Random.Range(0, 100) < lifeChance) toLive = true;

        xy[x, y] = newO.GetComponent<Cell>();
        xy[x, y].SetStartInfo(toLive, this);
    }

    void Update()
    {
        CheckAndAddNewColumnsRows();

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            //fix bug with zooming through zero and flipping it. zoom at distance also seems slows
            cam.orthographicSize -= Input.GetAxisRaw("Mouse ScrollWheel") * 5f;
            Vector2 camToMouse = cam.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log(camToMouse);
            camToMouse = (camToMouse - (Vector2)cam.transform.position);
            camToMouse *= Mathf.Clamp(Input.GetAxisRaw("Mouse ScrollWheel") * 1f,0, 3f);
            cam.transform.position += (Vector3)camToMouse;
        }

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

    private void CheckAndAddNewColumnsRows()
    {
        if (columnsTarget > columnsSpawned)
        {
            if (columnsTarget > columnsMax) 
            {
                columnsTarget = columnsMax;
                return;
            }
            // for (int x = columnsSpawned; x < columnsTarget; x++)
            // {
                for (int y = 0; y < rowsSpawned; y++)
                {
                    SpawnCell(columnsSpawned, y);
                }
            // }
            columnsSpawned ++;
        }
        if (rowsTarget > rowsSpawned)
        {
            if (rowsTarget > rowsMax)
            {
                rowsTarget = rowsMax;
                return;
            }
            // for (int y = rowsSpawned; y < rowsTarget; y++)
            // {
                for (int x = 0; x < columnsSpawned; x++)
                {
                    SpawnCell(x, rowsSpawned);
                }
            // }
            rowsSpawned ++;
        }
    }

    private void ResetGrid()
    {
        for (int x = 0; x < columnsSpawned; x++)
        {
            for (int y = 0; y < rowsSpawned; y++)
            {
                xy[x, y].SetLife(false);
            }
        }
    }

    void CheckAndSetLife(int xIn, int yIn)
    {
        if (xIn >= 0 && xIn < columnsSpawned)
        {
            if (yIn >= 0 && yIn < rowsSpawned)
            {
                xy[xIn,yIn].SetLife(true);
            }
        }
    }

    private void GetClickPosToGrid(out int xPos, out int yPos)
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        float xClick = Mathf.Clamp(mousePos.x, 0, columnsSpawned - 1);
        float yClick = Mathf.Clamp(mousePos.y, 0, rowsSpawned - 1);
        xPos = Mathf.RoundToInt(xClick);
        yPos = Mathf.RoundToInt(yClick);
    }


    private void CheckCells()
    {
        hasChecked = true;
        for (int x = 0; x < columnsSpawned; x++)
        {
            for (int y = 0; y < rowsSpawned; y++)
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

                int nx = (x + i + columnsSpawned) % columnsSpawned;
                int ny = (y + j + rowsSpawned) % rowsSpawned;
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

                if (nx >= 0 && nx < columnsSpawned && ny >= 0 && ny < rowsSpawned)
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
        for (int x = 0; x < columnsSpawned; x++)
        {
            for (int y = 0; y < rowsSpawned; y++)
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
        
        if (stableCells == columnsSpawned*rowsSpawned)
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
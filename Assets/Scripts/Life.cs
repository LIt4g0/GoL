//using System;
//using System.Collections;
using System.Collections.Generic;
using System.Linq;
//using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Life : MonoBehaviour
{
    [SerializeField] GameObject cellPrefab;
    [SerializeField] TMPro.TMP_Dropdown selectorDropDown;
    [SerializeField] MenuControl menu;


    [SerializeField][Range(0.01f,1.0f)] float refreshRate = 1;
    float refreshTimer = 0;

    public Color aliveColor;
    public Color dyingColor;
    public Color stableColor;
    public Color oscilatingColor;

    [SerializeField] [Range(5,1000)]int columnsMax = 1;
    [SerializeField] [Range(5,1000)]int rowsMax = 1;
    [SerializeField] [Range(5,1000)]int columnsTarget = 1;
    [SerializeField] [Range(5,1000)]int rowsTarget = 1;
    [SerializeField]int columnsSpawned;
    [SerializeField]int rowsSpawned;
    [SerializeField] [Range(0,100)]int lifeChance = 1;

    [SerializeField] int stableCells = 0;
    [SerializeField] int unstableGenerations;
    [SerializeField] bool stable;
    [SerializeField] bool wrapping;
    
    [SerializeField] [Range(1,500)]float zoomSpeed = 100f;
    [SerializeField] [Range(0,50)]float zoomAimMod = 5;
    [SerializeField] [Range(1,50)]float panSpeed = 5;

    [Range(0,2)]public int spawnType = 0;
    //[SerializeField] float gridGap = 0.1f;
    Camera cam;
    Cell[,] xy;
    bool playing = false;
    bool hasChecked = false;
    Vector2 startPos;
    EventSystem eventSystem;

    void Start()
    {
        cam = Camera.main;
        Application.targetFrameRate = 60;
        unstableGenerations = 0;
        eventSystem = EventSystem.current; //FindObjectOfType<EventSystem>();
        xy = new Cell[columnsMax,rowsMax];
        //columnsTarget = columnsMax;
        //rowsTarget = rowsMax;
        startPos = new Vector2(0,0);
        for (int x = 0; x < columnsTarget; x++)
        {
            for (int y = 0; y < rowsTarget; y++)
            {
                SpawnCell(x, y);
            }
        }
        columnsSpawned = columnsTarget;
        rowsSpawned = rowsTarget;
        cam.transform.position = new Vector3(((columnsSpawned-1)*0.5f)-startPos.x,((rowsSpawned-1)*0.5f)-startPos.y,cam.transform.position.z);
    }

    private void SpawnCell(int x, int y)
    {
        if (xy[x,y] != null)
        {
            Debug.Log("Exists already");
            return;
        } 
        bool toLive = false;
        Vector2 newPos = new Vector2(x-startPos.x, y-startPos.y);
        //if (flip) newPos = new Vector2(x+startPos.x, y-startPos.y);
        var newO = Instantiate(cellPrefab, newPos, Quaternion.identity);
        if (Random.Range(0, 100) < lifeChance) toLive = true;

        xy[x, y] = newO.GetComponent<Cell>();
        xy[x, y].SetStartInfo(toLive, this);
    }

    private void DespawnCells(int x, int y)
    {
        if (xy[x,y] == null)
        {
            Debug.Log("Despawned already");
            return;
        } 
        Destroy(xy[x, y].gameObject);
        xy[x,y] = null;
    }

    void Update()
    {
        CheckAndSetColumnsRows();

        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - (Input.GetAxisRaw("Mouse ScrollWheel") * zoomSpeed *Time.deltaTime),1,Mathf.Infinity);
            Vector2 camToMouse = cam.ScreenToWorldPoint(Input.mousePosition);
            camToMouse -= (Vector2)cam.transform.position;
            camToMouse *= Mathf.Clamp(Input.GetAxisRaw("Mouse ScrollWheel") ,0, 1f);
            cam.transform.position += Time.deltaTime * zoomAimMod * (Vector3)camToMouse;
        }

        if (Input.GetButton("Fire3") && !eventSystem.IsPointerOverGameObject())
        {
            Vector3 pan = Vector3.zero;
            pan.x = Mathf.Clamp(-Input.GetAxisRaw("Mouse X"), -1,1);
            pan.y = Mathf.Clamp(-Input.GetAxisRaw("Mouse Y"), -1,1);
            cam.transform.position += cam.orthographicSize * panSpeed * Time.deltaTime * pan;
        }

        if (Input.GetButton("Fire1") && !eventSystem.IsPointerOverGameObject())
        {
            FireCell();
        }

        if (Input.GetButtonDown("Fire2") && !eventSystem.IsPointerOverGameObject())
        {
            SpawnSpecial();
        }

        if (Input.GetKeyDown("backspace"))
        {
            ClearLife();
        }

        if (Input.GetButtonDown("Jump"))
        {
            PlayPause();
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
    }

    private void SpawnSpecial()
    {
        switch (spawnType)
        {
            case 0:
                FireAcorn();
                break;
            case 1:
                FirePulsar();
                break;
            case 2:
                FirePenta();
                break;
            default:
                break;
        }
    }

    public void PlayPause()
    {
        playing = !playing;
    }

    public void SetSpawnType()
    {
        spawnType = selectorDropDown.value;
        //spawnType = inType;
        //if (spawnType > 2) spawnType = 0;
    }

    private void CheckAndSetColumnsRows()
    {
        if (columnsTarget > columnsSpawned)
        {
            if (columnsTarget > columnsMax) 
            {
                columnsTarget = columnsMax;
                return;
            }
            
            for (int y = 0; y < rowsSpawned; y++)
            {
                SpawnCell(columnsSpawned, y);
            }
            columnsSpawned ++;
        }
        else if (columnsTarget < columnsSpawned)
        {
            for (int y = 0; y < rowsSpawned; y++)
            {
                DespawnCells(columnsSpawned+1, y);
            }
            columnsSpawned --;
        }

        if (rowsTarget > rowsSpawned)
        {
            if (rowsTarget > rowsMax)
            {
                rowsTarget = rowsMax;
                return;
            }
            for (int x = 0; x < columnsSpawned; x++)
            {
                SpawnCell(x, rowsSpawned);
            }
            rowsSpawned ++;
        }
        else if (rowsTarget < rowsSpawned)
        {
            for (int x = 0; x < columnsSpawned; x++)
            {
                DespawnCells(x, rowsSpawned+1);
            }
            rowsSpawned --;
        }
    }

    public void SpawnLife()
    {
        ClearLife();
        for (int x = 0; x < columnsTarget; x++)
        {
            for (int y = 0; y < rowsTarget; y++)
            {
                bool toLive = false;
                if (Random.Range(0, 100) < lifeChance) toLive = true;
                xy[x,y].SetLife(toLive);
            }
        }
    }
    private void GetClickPosToGrid(out int xPos, out int yPos)
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        // float xClick = Mathf.Clamp(mousePos.x, 0, columnsSpawned - 1);
        // float yClick = Mathf.Clamp(mousePos.y, 0, rowsSpawned - 1);
        float xClick = Mathf.Clamp(mousePos.x + startPos.x, 0, columnsSpawned);
        float yClick = Mathf.Clamp(mousePos.y + startPos.y, 0, rowsSpawned);

        xPos = Mathf.Clamp(Mathf.RoundToInt(xClick),0,columnsMax-1);
        yPos = Mathf.Clamp(Mathf.RoundToInt(yClick),0,rowsMax-1);

        if (Mathf.Clamp(xPos+1,0,columnsMax) > columnsSpawned) columnsTarget ++;
        if (Mathf.Clamp(yPos+1,0,rowsMax) > rowsSpawned) rowsTarget ++;
        // bool check = false;
        // if (mousePos.x < startPos.x*-1) check = true;

        CheckAndSetColumnsRows();
    }

    public void ClearLife()
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
        float percentage = ((float)stableCells/((float)columnsSpawned*(float)rowsSpawned))*100.0f;
        menu.SetStableText(stable, (int)percentage);
        
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
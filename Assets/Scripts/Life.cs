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
    [Header("References")]
    [SerializeField] GameObject cellPrefab;
    [SerializeField] TMPro.TMP_Dropdown selectorDropDown;
    [SerializeField] MenuControl menu;

    [Header("Cell Refresh")]
    [Range(0.01f,1.0f)] public float refreshRate = 1;
    float refreshTimer = 0;

    [Header("Color Management")]
    public Color aliveColor;
    public Color dyingColor;
    public Color stableColor;
    public Color oscilatingColor;
    public Color deadColor;
    [Range(0.00f,10.0f)] public float deadFadeTime = 1.0f;

    [Header("Grid Settings")]
    [Range(5,1000)] int columnsMax = 1000;
    [Range(5,1000)] int rowsMax = 1000;
    [SerializeField] [Range(5,1000)]int columnsTarget = 1;
    [SerializeField] [Range(5,1000)]int rowsTarget = 1;
    [SerializeField] bool enableWrapping;
    [SerializeField] int columnsSpawned;
    [SerializeField] int rowsSpawned;
    [SerializeField] [Range(0,100)]int chanceOfLife = 1;

    [Header("State of Cells")]
    [SerializeField] bool stable;
    [SerializeField] int stableCells = 0;
    [SerializeField] int unstableGenerations;
    
    [Header("Camera Settings")]
    [SerializeField] [Range(1,500)]float zoomSpeed = 100f;
    [SerializeField] [Range(0,50)]float zoomAimMod = 5;
    [SerializeField] [Range(1,50)]float panSpeed = 5;

    [HideInInspector]public int spawnType = 0;
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
        eventSystem = EventSystem.current;
        xy = new Cell[columnsMax,rowsMax];
        columnsTarget = (int)Mathf.Floor(Camera.main.orthographicSize * Camera.main.aspect * 2);
		rowsTarget = (int)Mathf.Floor(Camera.main.orthographicSize * 2);
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
        UpdateLifeName();
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
        var newO = Instantiate(cellPrefab, newPos, Quaternion.identity,transform);
        if (Random.Range(0, 100) < chanceOfLife) toLive = true;
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

        InputHandling();

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

    private void InputHandling()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            Zoom();
        }

        if (Input.GetButton("Fire3") && !eventSystem.IsPointerOverGameObject())
        {
            Pan();
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
    }

    private void Pan()
    {
        Vector3 pan = Vector3.zero;
        pan.x = Mathf.Clamp(-Input.GetAxisRaw("Mouse X"), -1, 1);
        pan.y = Mathf.Clamp(-Input.GetAxisRaw("Mouse Y"), -1, 1);
        cam.transform.position += cam.orthographicSize * panSpeed * Time.deltaTime * pan;
    }

    private void Zoom()
    {
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - (Input.GetAxisRaw("Mouse ScrollWheel") * zoomSpeed * Time.deltaTime), 1, Mathf.Infinity);
        Vector2 camToMouse = cam.ScreenToWorldPoint(Input.mousePosition);
        camToMouse -= (Vector2)cam.transform.position;
        camToMouse *= Mathf.Clamp(Input.GetAxisRaw("Mouse ScrollWheel"), 0, 1f);
        cam.transform.position += Time.deltaTime * zoomAimMod * (Vector3)camToMouse;
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
            case 3:
                FireLineX();
                break;
            case 4:
                FireLineY();
                break;
            case 5:
                FireBlock();
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
            UpdateLifeName();
        }
        else if (columnsTarget < columnsSpawned)
        {
            for (int y = 0; y < rowsSpawned; y++)
            {
                DespawnCells(columnsSpawned + 1, y);
            }
            columnsSpawned--;
            UpdateLifeName();
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
            UpdateLifeName();
        }
        else if (rowsTarget < rowsSpawned)
        {
            for (int x = 0; x < columnsSpawned; x++)
            {
                DespawnCells(x, rowsSpawned+1);
            }
            rowsSpawned --;
            UpdateLifeName();
        }
    }

    private void UpdateLifeName()
    {
        this.name = "Life has " + rowsSpawned * columnsSpawned + " cells.";
    }

    public void SpawnLife()
    {
        ClearLife();
        for (int x = 0; x < columnsTarget; x++)
        {
            for (int y = 0; y < rowsTarget; y++)
            {
                bool toLive = false;
                if (Random.Range(0, 100) < chanceOfLife) toLive = true;
                xy[x,y].SetLife(toLive);
            }
        }
    }
    private void GetClickPosToGrid(out int xPos, out int yPos)
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        float xClick = Mathf.Clamp(mousePos.x + startPos.x, 0, columnsSpawned);
        float yClick = Mathf.Clamp(mousePos.y + startPos.y, 0, rowsSpawned);

        xPos = Mathf.Clamp(Mathf.RoundToInt(xClick),0,columnsMax-1);
        yPos = Mathf.Clamp(Mathf.RoundToInt(yClick),0,rowsMax-1);

        if (Mathf.Clamp(xPos+1,0,columnsMax) > columnsSpawned) columnsTarget ++;
        if (Mathf.Clamp(yPos+1,0,rowsMax) > rowsSpawned) rowsTarget ++;

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
                if (enableWrapping)
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
                if (alive && neighbours < 2) //Debug.Log("Die due to loneliness");
                {
                    setAlive = false;
                }
                //if neighbours == 2 //Debug.Log("Live on");
                if (!alive && neighbours == 3)//Debug.Log("Come to life");
                {
                    setAlive = true;
                }
                if (alive && neighbours > 3)//Debug.Log("Die due to overpopulation on");
                {
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
            menu.SetGenerationText(unstableGenerations);
        }
        float percentage = (float)(stableCells/((float)columnsSpawned*(float)rowsSpawned))*100.0f;
        menu.SetStableText(stable, (int)percentage);
        
        refreshTimer = refreshRate;
        hasChecked = false;
        cam.backgroundColor = deadColor;
    }
    private void FireCell()
    {
        GetClickPosToGrid(out int xPos, out int yPos);
        xy[xPos, yPos].SetLife(true);
    }

    private void FireAcorn()
    {
        GetClickPosToGrid(out int xPos, out int yPos);
        xy[xPos, yPos].SetLife(true);
        //Spawn acorn
        CheckAndSetLife(xPos - 2, yPos + 1);
        CheckAndSetLife(xPos - 2, yPos - 1);
        CheckAndSetLife(xPos - 3, yPos - 1);
        CheckAndSetLife(xPos + 1, yPos - 1);
        CheckAndSetLife(xPos + 2, yPos - 1);
        CheckAndSetLife(xPos + 3, yPos - 1);
    }

    private void FirePulsar()
    {
        GetClickPosToGrid(out int xPos, out int yPos);
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

    private void FirePenta()
    {
        GetClickPosToGrid(out int xPos, out int yPos);
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

    private void FireLineX()
    {
        GetClickPosToGrid(out int xPos, out int yPos);
        xy[xPos, yPos].SetLife(true);
        //Fire Line X
        for (int x = 0; x < columnsSpawned; x++)
        {
            CheckAndSetLife(x, yPos);
        }
    }

    private void FireLineY()
    {
        GetClickPosToGrid(out int xPos, out int yPos);
        xy[xPos, yPos].SetLife(true);
        //Fire Line Y
        for (int y = 0; y < rowsSpawned; y++)
        {
            CheckAndSetLife(xPos, y);
        }
    }

    private void FireBlock()
    {
        GetClickPosToGrid(out int xPos, out int yPos);
        xy[xPos, yPos].SetLife(true);
        //Fire block of 20
        for (int y = -10; y < 10; y++)
        {
            for (int x = -10; x < 10; x++)
            {
                if (y == 0 && x == 0) continue;
                
                CheckAndSetLife(x+xPos, y+yPos);
            }
        }
    }
}
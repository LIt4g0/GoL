using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Life : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject cellPrefab;
    [SerializeField] TMPro.TMP_Dropdown selectorDropDown;
    [SerializeField] TMPro.TMP_Dropdown edgeDropDown;
    [SerializeField] TMPro.TMP_Dropdown colorSelectDropDown;
    [SerializeField] Slider lifeChanceSlider;
    [SerializeField] Slider lifeTimeSlider;
    [SerializeField] Slider deadTimeSlider;
    [SerializeField] MenuControl menu;

    [Header("Cell Refresh")]
    [Range(0.01f, 1.0f)] public float refreshRate = 1;
    float refreshTimer = 0;

    [Header("Color Management")]
    public Color aliveColor;
    public Color dyingColor;
    public Color stableColor;
    public Color oscilatingColor;
    public Color deadColor;
    [Range(0.00f, 10.0f)] public float deadFadeTime = 1.0f;

    [Header("Grid Settings")]
    [Range(5, 1000)] int columnsMax = 1000;
    [Range(5, 1000)] int rowsMax = 1000;
    [SerializeField][Range(5, 1000)] int columnsTarget = 1;
    [SerializeField][Range(5, 1000)] int rowsTarget = 1;
    [SerializeField] int columnsSpawned;
    [SerializeField] int rowsSpawned;
    [SerializeField][Range(0, 100)] int chanceOfLife = 1;

    [Header("State of Cells")]
    [SerializeField] bool stable;
    [SerializeField] int stableCells = 0;
    [SerializeField] int unstableGenerations;

    [Header("Camera Settings")]
    [SerializeField][Range(1, 500)] float zoomSpeed = 100f;
    [SerializeField][Range(0, 50)] float zoomAimMod = 5;
    [SerializeField][Range(1, 50)] float panSpeed = 5;

    int spawnType = 0;
    int edgeType = 0;
    int colorSet = 0;
    Camera cam;
    Cell[,] xy;
    bool playing = false;
    bool hasChecked = false;
    Vector2 startPos;
    EventSystem eventSystem;
    ColorManager colorManager;

    void Start()
    {
        colorManager = GetComponent<ColorManager>();
        SetPercentageLife();
        SetEdgeType();
        SetSpawnType();
        SetColors();
        SetLifeTime();
        SetDieTime();
        cam = Camera.main;
        Application.targetFrameRate = 60;
        unstableGenerations = 0;
        eventSystem = EventSystem.current;
        xy = new Cell[columnsMax, rowsMax];
        columnsTarget = (int)Mathf.Floor(Camera.main.orthographicSize * Camera.main.aspect * 2);
        rowsTarget = (int)Mathf.Floor(Camera.main.orthographicSize * 2);
        startPos = new Vector2(0, 0);
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
        cam.transform.position = new Vector3(((columnsSpawned - 1) * 0.5f) - startPos.x, ((rowsSpawned - 1) * 0.5f) - startPos.y, cam.transform.position.z);
    }



    void Update()
    {


        CheckAndSetColumnsRows();

        InputHandling();

        CheckCells();

        UpdateCells();
    }

    void InputHandling()
    {
  
        Zoom();
        Pan();
        FireCell();
        SpawnSpecial();

        if (Input.GetKeyDown("backspace"))
        {
            ClearLife();
        }

        if (Input.GetButtonDown("Jump"))
        {
            PlayPause();
        }
    }

    void Pan()
    {
        if (Input.GetButton("Fire3") && IsUIBlocking())
        {
            Vector3 pan = Vector3.zero;
            pan.x = Mathf.Clamp(-Input.GetAxisRaw("Mouse X"), -1, 1);
            pan.y = Mathf.Clamp(-Input.GetAxisRaw("Mouse Y"), -1, 1);
            cam.transform.position += cam.orthographicSize * panSpeed * Time.deltaTime * pan;
        }
    }

    void Zoom()
    {
        if (Input.GetAxis("Mouse ScrollWheel") == 0) return;

        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - (Input.GetAxisRaw("Mouse ScrollWheel") * zoomSpeed * Time.deltaTime), 1, Mathf.Infinity);
        Vector2 camToMouse = cam.ScreenToWorldPoint(Input.mousePosition);
        camToMouse -= (Vector2)cam.transform.position;
        camToMouse *= Mathf.Clamp(Input.GetAxisRaw("Mouse ScrollWheel"), 0, 1f);
        cam.transform.position += Time.deltaTime * zoomAimMod * (Vector3)camToMouse;
    }

    public void PlayPause()
    {
        playing = !playing;
    }

    public void SetSpawnType()
    {
        spawnType = selectorDropDown.value;
    }

    public void SetEdgeType()
    {
        edgeType = edgeDropDown.value;
    }

    public void SetColors()
    {
        colorSet = colorSelectDropDown.value;

        aliveColor = colorManager.aliveColors[colorSet];
        dyingColor = colorManager.dyingColors[colorSet];
        stableColor = colorManager.stableColors[colorSet];
        oscilatingColor = colorManager.oscilatingColors[colorSet];
        deadColor = colorManager.deadColors[colorSet];
    }


    public void SetPercentageLife()
    {
        chanceOfLife = (int)lifeChanceSlider.value;
    }

    public void SetLifeTime()
    {
        refreshRate = lifeTimeSlider.value;
    }

    public void SetDieTime()
    {
        deadFadeTime = deadTimeSlider.value;
    }

    public void SpawnLife()
    {
        ClearLife();
        for (int x = 0; x < columnsSpawned; x++)
        {
            for (int y = 0; y < rowsSpawned; y++)
            {
                bool toLive = false;
                if (Random.Range(0, 100) < chanceOfLife) toLive = true;
                xy[x, y].SetLife(toLive);
            }
        }
    }

    void GetClickPosToGrid(out int xPos, out int yPos)
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        float xClick = Mathf.Clamp(mousePos.x + startPos.x, 0, columnsSpawned);
        float yClick = Mathf.Clamp(mousePos.y + startPos.y, 0, rowsSpawned);

        xPos = Mathf.Clamp(Mathf.RoundToInt(xClick), 0, columnsMax - 1);
        yPos = Mathf.Clamp(Mathf.RoundToInt(yClick), 0, rowsMax - 1);

        if (Mathf.Clamp(xPos + 1, 0, columnsMax) > columnsSpawned) columnsTarget++;
        if (Mathf.Clamp(yPos + 1, 0, rowsMax) > rowsSpawned) rowsTarget++;

        CheckAndSetColumnsRows();
    }

    void CheckAndSetColumnsRows()
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
            columnsSpawned++;
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
            rowsSpawned++;
            UpdateLifeName();
        }
        else if (rowsTarget < rowsSpawned)
        {
            for (int x = 0; x < columnsSpawned; x++)
            {
                DespawnCells(x, rowsSpawned + 1);
            }
            rowsSpawned--;
            UpdateLifeName();
        }
    }

    void UpdateLifeName()
    {
        this.name = "Life has " + rowsSpawned * columnsSpawned + " cells.";
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
                xy[xIn, yIn].SetLife(true);
            }
        }
    }

    void CheckCells()
    {
        if (playing) refreshTimer -= Time.deltaTime;

        if (hasChecked || refreshTimer > 0) return;
        
        //float checktime = Time.realtimeSinceStartup;
        hasChecked = true;
        for (int x = 0; x < columnsSpawned; x++)
        {
            for (int y = 0; y < rowsSpawned; y++)
            {
                if (!xy[x,y].alive) continue;
                switch (edgeType)
                {
                    case 0:
                        xy[x, y].aliveNeighbours = CheckNeighbours(x, y);
                        break;
                    case 1:
                        xy[x, y].aliveNeighbours = CheckNeighboursWrapping(x, y);
                        break;
                    case 2:
                        xy[x, y].aliveNeighbours = CheckNeighboursBounce(x, y);
                        break;
                    default:
                        break;
                }
            }
        }

        //Debug.Log(Time.realtimeSinceStartup - checktime);
    }

    int CheckNeighbours(int x, int y)
    {
        int foundNeighbours = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;

                int nx = x + i;
                int ny = y + j;
                if (nx >= 0 && nx < columnsSpawned && ny >= 0 && ny < rowsSpawned)
                {
                    if (xy[nx, ny].alive) foundNeighbours++;// Count if the neighboring cell is alive
                        else xy[nx,ny].aliveNeighbours++;// Add neighbour to neighbour cell if its dead to attempt ressurection
                }
            }
        }

        return foundNeighbours;
    }

    int CheckNeighboursWrapping(int x, int y)
    {
        int foundNeighbours = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;

                int nx = (x + i + columnsSpawned) % columnsSpawned;
                int ny = (y + j + rowsSpawned) % rowsSpawned;
                if (xy[nx, ny].alive) foundNeighbours++;
                    else xy[nx,ny].aliveNeighbours++;// Add neighbour to neighbour cell if its dead to attempt ressurection
            }
        }

        return foundNeighbours;
    }

    int CheckNeighboursBounce(int x, int y)
    {
        int foundNeighbours = 0;
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;

                int nx = x + i;
                int ny = y + j;

                // If cell to check is out of bounds, check other side in attempt to create a bounce effect
                if (nx < 0 && i == -1) nx = nx + 2;
                else if (nx > columnsSpawned - 1 && i == 1) nx = nx - 2;

                if (ny < 0 && j == -1) ny = ny + 2;
                else if (ny > rowsSpawned - 1 && j == 1) ny = ny - 2;
                
                if (xy[nx, ny].alive) foundNeighbours++;// Count if the neighboring cell is alive
                    else xy[nx,ny].aliveNeighbours++;// Add neighbour to neighbour cell if its dead to attempt ressurection
            }
        }

        return foundNeighbours;
    }

    void UpdateCells()
    {
        if (!hasChecked) return;
        //float timer = Time.realtimeSinceStartup;
        stableCells = 0;
        for (int x = 0; x < columnsSpawned; x++)
        {
            for (int y = 0; y < rowsSpawned; y++)
            {
                Cell thisCell = xy[x, y];
                bool alive = thisCell.alive;
                bool setAlive = alive;
                int neighbours = thisCell.aliveNeighbours;
                
                if (neighbours < 2) setAlive = false;//Debug.Log("Die due to loneliness");
                else if (neighbours == 3) setAlive = true;//Debug.Log("Come to life");
                else if (neighbours > 3) setAlive = false;//Debug.Log("Die due to overpopulation on");
                
                //if neighbours == 2 //Debug.Log("No change");    

                thisCell.SetLife(setAlive);
                thisCell.aliveNeighbours = 0;
                if (thisCell.stableGenerations > 1)
                {
                    stableCells++;
                }
            }
        }

        if (stableCells == columnsSpawned * rowsSpawned)
        {
            stable = true;
        }
        else
        {
            stable = false;
            unstableGenerations++;
            menu.SetGenerationText(unstableGenerations);
        }
        float percentage = (float)(stableCells / ((float)columnsSpawned * (float)rowsSpawned)) * 100.0f;
        menu.SetStableText(stable, (int)percentage);

        refreshTimer = refreshRate;
        hasChecked = false;
        cam.backgroundColor = deadColor;

        //Debug.Log(Time.realtimeSinceStartup - timer);
    }
    
    void SpawnCell(int x, int y)
    {
        if (xy[x, y] != null)
        {
            //Debug.Log("Exists already");
            return;
        }
        bool toLive = false;
        Vector2 newPos = new Vector2(x - startPos.x, y - startPos.y);
        var newO = Instantiate(cellPrefab, newPos, Quaternion.identity, transform);
        if (Random.Range(0, 100) < chanceOfLife) toLive = true;
        xy[x, y] = newO.GetComponent<Cell>();
        xy[x, y].SetStartInfo(toLive, this);
    }

    void DespawnCells(int x, int y)
    {
        if (xy[x, y] == null)
        {
            //Debug.Log("Despawned already");
            return;
        }
        Destroy(xy[x, y].gameObject);
        xy[x, y] = null;
    }

    void FireCell()
    {
        if (Input.GetButton("Fire1") && IsUIBlocking())
        {
            GetClickPosToGrid(out int xPos, out int yPos);
            xy[xPos, yPos].SetLife(true);
        }
    }

    void SpawnSpecial()
    {
        if (Input.GetButtonDown("Fire2") && IsUIBlocking())
        {
            GetClickPosToGrid(out int xPos, out int yPos);
            switch (spawnType)
            {
                case 0:
                    FireAcorn(xPos, yPos);
                    break;
                case 1:
                    FirePulsar(xPos, yPos);
                    break;
                case 2:
                    FirePenta(xPos, yPos);
                    break;
                case 3:
                    FireLineX(xPos, yPos);
                    break;
                case 4:
                    FireLineY(xPos, yPos);
                    break;
                case 5:
                    FireCross(xPos, yPos);
                    break;
                case 6:
                    FireBlock(xPos, yPos);
                    break;
                case 7:
                    FireChaos(xPos, yPos);
                    break;
                default:
                    break;
            }
        }
    }

    bool IsUIBlocking() // Credit daveMennenoh Unity forums
    {
        PointerEventData eventData = new PointerEventData(eventSystem);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        eventSystem.RaycastAll(eventData, raycastResults);
       
        for (int index = 0; index < raycastResults.Count; index++)
        {
            RaycastResult curRaysastResult = raycastResults[index];
            if (curRaysastResult.gameObject.layer == LayerMask.NameToLayer("UI"))
                return false;
        }
        return true;
    }

    void FireAcorn(int xPos, int yPos)
    {
        xy[xPos, yPos].SetLife(true);
        //Spawn acorn
        CheckAndSetLife(xPos - 2, yPos + 1);
        CheckAndSetLife(xPos - 2, yPos - 1);
        CheckAndSetLife(xPos - 3, yPos - 1);
        CheckAndSetLife(xPos + 1, yPos - 1);
        CheckAndSetLife(xPos + 2, yPos - 1);
        CheckAndSetLife(xPos + 3, yPos - 1);
    }

    void FirePulsar(int xPos, int yPos)
    {
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
        CheckAndSetLife(xPos + 1, yPos - 2);
        CheckAndSetLife(xPos + 1, yPos - 3);
        CheckAndSetLife(xPos + 1, yPos - 4);

        CheckAndSetLife(xPos + 6, yPos - 2);
        CheckAndSetLife(xPos + 6, yPos - 3);
        CheckAndSetLife(xPos + 6, yPos - 4);

        CheckAndSetLife(xPos + 2, yPos - 1);
        CheckAndSetLife(xPos + 3, yPos - 1);
        CheckAndSetLife(xPos + 4, yPos - 1);

        CheckAndSetLife(xPos + 2, yPos - 6);
        CheckAndSetLife(xPos + 3, yPos - 6);
        CheckAndSetLife(xPos + 4, yPos - 6);

        //top right pulsar
        CheckAndSetLife(xPos + 1, yPos + 2);
        CheckAndSetLife(xPos + 1, yPos + 3);
        CheckAndSetLife(xPos + 1, yPos + 4);

        CheckAndSetLife(xPos + 6, yPos + 2);
        CheckAndSetLife(xPos + 6, yPos + 3);
        CheckAndSetLife(xPos + 6, yPos + 4);

        CheckAndSetLife(xPos + 2, yPos + 1);
        CheckAndSetLife(xPos + 3, yPos + 1);
        CheckAndSetLife(xPos + 4, yPos + 1);

        CheckAndSetLife(xPos + 2, yPos + 6);
        CheckAndSetLife(xPos + 3, yPos + 6);
        CheckAndSetLife(xPos + 4, yPos + 6);
    }

    void FirePenta(int xPos, int yPos)
    {
        xy[xPos, yPos].SetLife(false);
        //LeftWall
        CheckAndSetLife(xPos - 2, yPos + 2);
        CheckAndSetLife(xPos - 2, yPos + 1);
        CheckAndSetLife(xPos - 2, yPos + 0);
        CheckAndSetLife(xPos - 2, yPos - 1);
        CheckAndSetLife(xPos - 2, yPos - 2);
        CheckAndSetLife(xPos - 2, yPos - 3);

        //RightWall
        CheckAndSetLife(xPos + 2, yPos + 2);
        CheckAndSetLife(xPos + 2, yPos + 1);
        CheckAndSetLife(xPos + 2, yPos + 0);
        CheckAndSetLife(xPos + 2, yPos - 1);
        CheckAndSetLife(xPos + 2, yPos - 2);
        CheckAndSetLife(xPos + 2, yPos - 3);

        //Top
        CheckAndSetLife(xPos - 1, yPos + 3);
        CheckAndSetLife(xPos + 0, yPos + 4);
        CheckAndSetLife(xPos + 1, yPos + 3);

        //Bottom
        CheckAndSetLife(xPos - 1, yPos - 4);
        CheckAndSetLife(xPos + 0, yPos - 5);
        CheckAndSetLife(xPos + 1, yPos - 4);
    }

    void FireLineX(int xPos, int yPos)
    {
        xy[xPos, yPos].SetLife(true);
        //Fire Line X
        for (int x = 0; x < columnsSpawned; x++)
        {
            CheckAndSetLife(x, yPos);
        }
    }

    void FireLineY(int xPos, int yPos)
    {
        xy[xPos, yPos].SetLife(true);
        //Fire Line Y
        for (int y = 0; y < rowsSpawned; y++)
        {
            CheckAndSetLife(xPos, y);
        }
    }

    void FireBlock(int xPos, int yPos)
    {
        xy[xPos, yPos].SetLife(true);
        //Fire block of 20
        for (int y = -10; y < 10; y++)
        {
            for (int x = -10; x < 10; x++)
            {
                if (y == 0 && x == 0) continue;

                CheckAndSetLife(x + xPos, y + yPos);
            }
        }
    }

    void FireCross(int xPos, int yPos)
    {
        FireLineY(xPos, yPos);
        FireLineX(xPos, yPos);
    }

    void FireChaos(int xPos, int yPos)
    {
        xPos = Random.Range(0, columnsSpawned);
        yPos = Random.Range(0, rowsSpawned);
        FireAcorn(xPos, yPos);
        xPos = Random.Range(0, columnsSpawned);
        yPos = Random.Range(0, rowsSpawned);
        FireBlock(xPos, yPos);
        // xPos = Random.Range(0, columnsSpawned);
        // yPos = Random.Range(0, rowsSpawned);
        // FireCross(xPos, yPos);
        // xPos = Random.Range(0, columnsSpawned);
        // yPos = Random.Range(0, rowsSpawned);
        // FireLineX(xPos, yPos);
        // xPos = Random.Range(0, columnsSpawned);
        // yPos = Random.Range(0, rowsSpawned);
        // FireLineY(xPos, yPos);
        xPos = Random.Range(0, columnsSpawned);
        yPos = Random.Range(0, rowsSpawned);
        FirePulsar(xPos, yPos);
        xPos = Random.Range(0, columnsSpawned);
        yPos = Random.Range(0, rowsSpawned);
        FirePenta(xPos, yPos);
    }
}
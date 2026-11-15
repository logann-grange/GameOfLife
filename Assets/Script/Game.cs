using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class Game : MonoBehaviour
{
    private const int MARGIN = 2;
    public int visibleWidth = 64;
    public int visibleHeight = 48;
    public bool symetricMode = false;
    public GridOverlay overlay;
    [Range(0f, 0.5f)] public float uiLeft = 0.18f;
    [Range(0f, 0.5f)] public float uiRight = 0.18f;
    private int lastScreenW;
    private int lastScreenH;
    public float ticksPerSecond = 10;
    public bool simulationEnable = false;
    private int GridWidth => visibleWidth + 2 * MARGIN;
    private int GridHeight => visibleHeight + 2 * MARGIN;
    public TMP_InputField inputWidth;
    public TMP_InputField inputHeight;
    public TMP_InputField inputSpeed;
    private float timer = 0;
    private Cell[,] grid;
    private Cell prefab;
    private string SavePath => Path.Combine(Application.dataPath, "..", "saves");
    public TMP_InputField inputSaveName;
    public TMP_Dropdown dropdownSaves;

    
    void Start()
    {
        prefab = Resources.Load<Cell>("Prefabs/Cell");
        ResizeGrid(visibleWidth, visibleHeight);

        inputWidth.text = visibleWidth.ToString();
        inputHeight.text = visibleHeight.ToString();
        inputSpeed.text = ticksPerSecond.ToString();

        RefreshSaveList();
    }

    
    void Update()
    {
        if (simulationEnable)
        {
            float speed = 1f/ticksPerSecond;
            timer += Time.deltaTime;
            if (timer >= speed)
            {
                timer -= speed;
                NextStep();
            }
        }
        UserInput();

        if (Screen.width != lastScreenW || Screen.height != lastScreenH)
            FitCamera();
    }

    public void NextStep()
    {
        CountNeighbors();
        PopulationControl();
    }

    public void AppliSymetricMode() { symetricMode = true; }
    public void AppliInfiniteMode() { symetricMode = false; }
    public void StartSimulation() { simulationEnable = true; }
    public void StopSimulation()  { simulationEnable = false; }

    public void ApplySpeed()
    {
        if (float.TryParse(inputSpeed.text, out float speed))
            ticksPerSecond = speed;
    }

    public void ApplyGridSize()
    {
        if (int.TryParse(inputWidth.text, out int width) && int.TryParse(inputHeight.text, out int height))
        {
            // max values
            width = Mathf.Clamp(width, 4, 200);
            height = Mathf.Clamp(height, 4, 200);
            ResizeGrid(width, height);
        }
    }

    public void ResizeGrid(int newW, int newH)
    {
        newW = Mathf.Max(4, newW);
        newH = Mathf.Max(4, newH);

        Cell[,] old = grid;
        int oldW = visibleWidth, oldH = visibleHeight;
        bool firstBuild = old == null;

        visibleWidth = newW;
        visibleHeight = newH;
        grid = new Cell[GridWidth, GridHeight];

        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                int wx = x - MARGIN;
                int wy = y - MARGIN;

                bool visible = wx >= 0 && wx < visibleWidth && wy >= 0 && wy < visibleHeight;

                bool alive = false;
                //if (firstBuild)
                    //alive = visible && RandomAliveCell();
                if (visible && wx < oldW && wy < oldH && !firstBuild)
                    alive = old[wx + MARGIN, wy + MARGIN].isAlive;

                Cell cell = Instantiate(prefab, new Vector2(wx, wy), Quaternion.identity);
                cell.isBuffer = !visible;
                cell.SetAlive(alive);
                grid[x, y] = cell;
            }
        }

        if (old != null)
            foreach (Cell c in old) Destroy(c.gameObject);

        FitCamera();
        if (overlay != null) overlay.Rebuild(visibleWidth, visibleHeight);
    }

    void FitCamera()
    {
        Camera cam = Camera.main;
        cam.rect = new Rect(0f, 0f, 1f, 1f);

        float freeFraction = 1f - uiLeft - uiRight;      // grid part
        float centerFraction = uiLeft + freeFraction / 2f;

        float freeAspect = (Screen.width * freeFraction) / Screen.height;

        float sizeForHeight = visibleHeight / 2f;
        float sizeForWidth = visibleWidth / (2f * freeAspect);
        float size = Mathf.Max(sizeForHeight, sizeForWidth) * 1.02f;
        cam.orthographicSize = size;

        float worldScreenWidth = 2f * size * cam.aspect;

        // place the grid in center
        float gridCenterX = (visibleWidth - 1) / 2f;
        float offsetX = (0.5f - centerFraction) * worldScreenWidth;

        cam.transform.position = new Vector3(gridCenterX + offsetX, (visibleHeight - 1) / 2f, -10f);

        lastScreenW = Screen.width;
        lastScreenH = Screen.height;
    }

    void UserInput()
    {
        var selected = UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject;
        bool typing = selected != null && selected.GetComponent<TMP_InputField>() != null;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector2 mousePoint = Camera.main.ScreenToWorldPoint(screenPos);
            int x = Mathf.RoundToInt(mousePoint.x) + MARGIN;
            int y = Mathf.RoundToInt(mousePoint.y) + MARGIN;

            if (x >= MARGIN && y >= MARGIN && x < GridWidth - MARGIN && y < GridHeight - MARGIN)
                grid[x, y].SetAlive(!grid[x, y].isAlive);
        }

        if (!typing && Keyboard.current != null)
        {
            if (Keyboard.current.pKey.wasReleasedThisFrame) simulationEnable = false;
            if (Keyboard.current.oKey.wasReleasedThisFrame) simulationEnable = true;

            // Increase / decrease grid and keep 4:3 ratio
            if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                ResizeGrid(visibleWidth + 8, visibleHeight + 6);
            if (Keyboard.current.downArrowKey.wasPressedThisFrame)
                ResizeGrid(visibleWidth - 8, visibleHeight - 6);
        }
    }

    public void PlaceRandomCells()
    {
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                bool inVisibleArea = x >= MARGIN && x < GridWidth - MARGIN && y >= MARGIN && y < GridHeight - MARGIN;
                grid[x, y].SetAlive(inVisibleArea && RandomAliveCell());
            }
        }
    }

    public void ClearGrid()
    {
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                grid[x, y].SetAlive(false);
            }
        }
    }

    bool RandomAliveCell()
    {
        int rand = UnityEngine.Random.Range(0, 100);

        if (rand > 75)
        {
            return true;
        }
        return false;
    }

    void CountNeighbors()
    {
        int minX = symetricMode ? MARGIN : 0;
        int maxX = symetricMode ? GridWidth - MARGIN : GridWidth;
        int minY = symetricMode ? MARGIN : 0;
        int maxY = symetricMode ? GridHeight - MARGIN : GridHeight;

        for (int y = minY; y < maxY; y++)
        {
            for (int x = minX; x < maxX; x++)
            {
                int n = 0;

                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0) 
                            continue;

                        int nx = x + dx;
                        int ny = y + dy;

                        if (symetricMode)
                        {
                            // On enroule dans la zone visible [MARGIN, MARGIN + visible)
                            nx = MARGIN + ((nx - MARGIN) % visibleWidth + visibleWidth) % visibleWidth;
                            ny = MARGIN + ((ny - MARGIN) % visibleHeight + visibleHeight) % visibleHeight;
                        }
                        else if (nx < 0 || nx >= GridWidth || ny < 0 || ny >= GridHeight)
                        {
                            continue;
                        }

                        if (grid[nx, ny].isAlive) 
                            n++;
                    }
                }

                grid[x, y].numNeighboors = n;
            }
        }
    }

    void PopulationControl()
    {
        int minX = symetricMode ? MARGIN : 0;
        int maxX = symetricMode ? GridWidth - MARGIN : GridWidth;
        int minY = symetricMode ? MARGIN : 0;
        int maxY = symetricMode ? GridHeight - MARGIN : GridHeight;

        for (int y = minY; y < maxY; y++)
        {
            for (int x = minX; x < maxX; x++)
            {
                Cell c = grid[x, y];
                if (c.isAlive)
                {
                    if (c.numNeighboors != 2 && c.numNeighboors != 3) c.SetAlive(false);
                }
                else if (c.numNeighboors == 3)
                {
                    c.SetAlive(true);
                }
            }
        }
    }



    public void SaveGridButton()
    {
        string name = SanitizeFileName(inputSaveName != null ? inputSaveName.text : "");
        if (string.IsNullOrWhiteSpace(name))
            name = "save_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");

        SaveToPng(name + ".png");
        RefreshSaveList(name); // met à jour la liste et sélectionne la nouvelle save
    }

    public void LoadGridButton()
    {
        string name = GetSelectedSaveName();
        if (name == null) return;

        simulationEnable = false;
        LoadFromPng(name + ".png");
    }

    public void DeleteSaveButton()
    {
        string name = GetSelectedSaveName();
        if (name == null) return;

        string fullPath = Path.Combine(SavePath, name + ".png");
        if (File.Exists(fullPath)) File.Delete(fullPath);
        RefreshSaveList();
    }

    public void RefreshSaveList(string selectName = null)
    {
        if (dropdownSaves == null) return;

        var names = new List<string>();
        if (Directory.Exists(SavePath))
        {
            var files = new List<string>(Directory.GetFiles(SavePath, "*.png"));
            // plus récentes en premier
            files.Sort((a, b) => File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)));
            foreach (string f in files)
                names.Add(Path.GetFileNameWithoutExtension(f));
        }

        dropdownSaves.ClearOptions();
        dropdownSaves.AddOptions(names);

        int index = selectName != null ? names.IndexOf(selectName) : 0;
        dropdownSaves.value = Mathf.Max(0, index);
        dropdownSaves.RefreshShownValue();
    }

    string GetSelectedSaveName()
    {
        if (dropdownSaves == null || dropdownSaves.options.Count == 0)
            return null;
        return dropdownSaves.options[dropdownSaves.value].text;
    }

    string SanitizeFileName(string name)
    {
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c.ToString(), "");
        return name.Trim();
    }

    public void SaveToPng(string fileName)
    {
        var pixels = new Color32[visibleWidth * visibleHeight];
        var white = new Color32(255, 255, 255, 255);
        var black = new Color32(0, 0, 0, 255);

        for (int y = 0; y < visibleHeight; y++)
            for (int x = 0; x < visibleWidth; x++)
                pixels[y * visibleWidth + x] = grid[x + MARGIN, y + MARGIN].isAlive ? white : black;

        var tex = new Texture2D(visibleWidth, visibleHeight, TextureFormat.RGBA32, false);
        tex.SetPixels32(pixels);
        tex.Apply();

        Directory.CreateDirectory(SavePath);
        string fullPath = Path.Combine(SavePath, fileName);
        File.WriteAllBytes(fullPath, tex.EncodeToPNG());
        Destroy(tex);

        Debug.Log("Sauvegardé : " + fullPath);
    }

    public void LoadFromPng(string fileName)
    {
        string fullPath = Path.Combine(SavePath, fileName);
        if (!File.Exists(fullPath))
        {
            Debug.LogWarning("Fichier introuvable : " + fullPath);
            return;
        }

        var tex = new Texture2D(2, 2);
        if (!tex.LoadImage(File.ReadAllBytes(fullPath)))
        {
            Debug.LogWarning("PNG invalide");
            Destroy(tex);
            return;
        }

        Color32[] pixels = tex.GetPixels32();
        int imgW = tex.width, imgH = tex.height;
        Destroy(tex);

        // Redimensionne la grille à la taille de l'image (mêmes limites que ApplyGridSize)
        ResizeGrid(Mathf.Clamp(imgW, 4, 200), Mathf.Clamp(imgH, 4, 200));

        for (int y = 0; y < visibleHeight; y++)
        {
            for (int x = 0; x < visibleWidth; x++)
            {
                bool alive = x < imgW && y < imgH && pixels[y * imgW + x].r > 127;
                grid[x + MARGIN, y + MARGIN].SetAlive(alive);
            }
        }

        inputWidth.text = visibleWidth.ToString();
        inputHeight.text = visibleHeight.ToString();
    }
}

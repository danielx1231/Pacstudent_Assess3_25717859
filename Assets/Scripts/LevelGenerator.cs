using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Level Settings")]
    public Vector2Int levelSize = new Vector2Int(20, 20);
    public float gridSize = 1f;
    public Vector2 levelCenter = Vector2.zero;
    
    [Header("Tile Types")]
    public int emptyTile = 0;
    public int wallTile = 1;
    public int ghostWallTile = 2;
    public int pelletTile = 3;
    
    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject ghostWallPrefab;
    public GameObject pelletPrefab;
    
    [Header("References")]
    public PacStudentController pacStudent;
    public CherryController cherryController;
    
    private int[,] levelMap;
    private GameObject[,] tileObjects;
    
    void Start()
    {
        GenerateLevel();
    }
    
    void GenerateLevel()
    {
        // Initialize level map
        levelMap = new int[levelSize.x, levelSize.y];
        tileObjects = new GameObject[levelSize.x, levelSize.y];
        
        // Generate basic level layout
        GenerateBasicLevel();
        
        // Instantiate visual tiles
        InstantiateTiles();
        
        // Update controllers with level data
        if (pacStudent != null)
        {
            pacStudent.SetLevelMap(levelMap, levelSize, levelCenter);
        }
        
        if (cherryController != null)
        {
            cherryController.SetLevelBounds(levelCenter, new Vector2(levelSize.x * gridSize, levelSize.y * gridSize));
        }
    }
    
    void GenerateBasicLevel()
    {
        // Create walls around the perimeter
        for (int x = 0; x < levelSize.x; x++)
        {
            for (int y = 0; y < levelSize.y; y++)
            {
                if (x == 0 || x == levelSize.x - 1 || y == 0 || y == levelSize.y - 1)
                {
                    levelMap[x, y] = wallTile;
                }
                else
                {
                    levelMap[x, y] = emptyTile;
                }
            }
        }
        
        // Add some internal walls
        AddInternalWalls();
        
        // Add ghost walls (special walls that can be passed through from inside)
        AddGhostWalls();
        
        // Add pellets
        AddPellets();
    }
    
    void AddInternalWalls()
    {
        // Add some internal wall patterns
        for (int x = 5; x < levelSize.x - 5; x += 3)
        {
            for (int y = 5; y < levelSize.y - 5; y += 3)
            {
                if (Random.Range(0f, 1f) < 0.3f) // 30% chance
                {
                    levelMap[x, y] = wallTile;
                }
            }
        }
    }
    
    void AddGhostWalls()
    {
        // Add ghost walls in the center area
        int centerX = levelSize.x / 2;
        int centerY = levelSize.y / 2;
        
        // Create a small ghost area
        for (int x = centerX - 2; x <= centerX + 2; x++)
        {
            for (int y = centerY - 2; y <= centerY + 2; y++)
            {
                if (x >= 0 && x < levelSize.x && y >= 0 && y < levelSize.y)
                {
                    if (levelMap[x, y] == emptyTile)
                    {
                        levelMap[x, y] = ghostWallTile;
                    }
                }
            }
        }
    }
    
    void AddPellets()
    {
        // Add pellets to empty spaces
        for (int x = 1; x < levelSize.x - 1; x++)
        {
            for (int y = 1; y < levelSize.y - 1; y++)
            {
                if (levelMap[x, y] == emptyTile)
                {
                    levelMap[x, y] = pelletTile;
                }
            }
        }
    }
    
    void InstantiateTiles()
    {
        for (int x = 0; x < levelSize.x; x++)
        {
            for (int y = 0; y < levelSize.y; y++)
            {
                Vector3 worldPos = GridToWorld(new Vector2Int(x, y));
                
                switch (levelMap[x, y])
                {
                    case 1: // Wall
                        if (wallPrefab != null)
                        {
                            tileObjects[x, y] = Instantiate(wallPrefab, worldPos, Quaternion.identity);
                        }
                        break;
                    case 2: // Ghost Wall
                        if (ghostWallPrefab != null)
                        {
                            tileObjects[x, y] = Instantiate(ghostWallPrefab, worldPos, Quaternion.identity);
                        }
                        break;
                    case 3: // Pellet
                        if (pelletPrefab != null)
                        {
                            tileObjects[x, y] = Instantiate(pelletPrefab, worldPos, Quaternion.identity);
                        }
                        break;
                }
            }
        }
    }
    
    Vector3 GridToWorld(Vector2Int gridPos)
    {
        return levelCenter + new Vector3(gridPos.x * gridSize, gridPos.y * gridSize, 0);
    }
    
    Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - (Vector3)levelCenter;
        return new Vector2Int(
            Mathf.RoundToInt(localPos.x / gridSize),
            Mathf.RoundToInt(localPos.y / gridSize)
        );
    }
    
    // Public methods for other scripts to access level data
    public int[,] GetLevelMap()
    {
        return levelMap;
    }
    
    public Vector2Int GetLevelSize()
    {
        return levelSize;
    }
    
    public Vector2 GetLevelCenter()
    {
        return levelCenter;
    }
    
    public float GetGridSize()
    {
        return gridSize;
    }
    
    public bool IsWalkable(Vector2Int gridPos)
    {
        if (gridPos.x < 0 || gridPos.x >= levelSize.x || 
            gridPos.y < 0 || gridPos.y >= levelSize.y)
        {
            return false;
        }
        
        int tileType = levelMap[gridPos.x, gridPos.y];
        return tileType == emptyTile || tileType == ghostWallTile || tileType == pelletTile;
    }
    
    public bool HasPellet(Vector2Int gridPos)
    {
        if (gridPos.x < 0 || gridPos.x >= levelSize.x || 
            gridPos.y < 0 || gridPos.y >= levelSize.y)
        {
            return false;
        }
        
        return levelMap[gridPos.x, gridPos.y] == pelletTile;
    }
    
    public void RemovePellet(Vector2Int gridPos)
    {
        if (gridPos.x >= 0 && gridPos.x < levelSize.x && 
            gridPos.y >= 0 && gridPos.y < levelSize.y)
        {
            if (levelMap[gridPos.x, gridPos.y] == pelletTile)
            {
                levelMap[gridPos.x, gridPos.y] = emptyTile;
                
                // Destroy pellet object
                if (tileObjects[gridPos.x, gridPos.y] != null)
                {
                    Destroy(tileObjects[gridPos.x, gridPos.y]);
                    tileObjects[gridPos.x, gridPos.y] = null;
                }
            }
        }
    }
}
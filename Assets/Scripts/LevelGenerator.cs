using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Level Map")]
    [Tooltip("2D array representing the level layout")]
    public int[,] levelMap;
    
    [Header("Map Dimensions")]
    [SerializeField] private int mapWidth = 28;
    [SerializeField] private int mapHeight = 31;
    
    [Header("Tile Types")]
    [Tooltip("0=Empty, 1=Wall, 2=Pellet, 3=PowerPellet, 4=GhostWall, 5=EmptySpace, 6=GhostZone")]
    
    void Awake()
    {
        InitializeLevelMap();
    }
    
    void InitializeLevelMap()
    {
        // Initialize the level map with default dimensions
        if (levelMap == null)
        {
            levelMap = new int[mapHeight, mapWidth];
            
            // Fill with default values (you should replace this with your actual level data)
            // This is a simple example - you'll need to load your actual level layout
            for (int y = 0; y < mapHeight; y++)
            {
                for (int x = 0; x < mapWidth; x++)
                {
                    // Default pattern: walls on borders, walkable inside
                    if (x == 0 || x == mapWidth - 1 || y == 0 || y == mapHeight - 1)
                        levelMap[y, x] = 1; // Wall
                    else
                        levelMap[y, x] = 2; // Pellet
                }
            }
        }
    }
    
    // Helper method to get tile type at grid position
    public int GetTileAt(int x, int y)
    {
        if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
            return 1; // Return wall for out of bounds
        
        return levelMap[y, x];
    }
    
    // Helper method to set tile type at grid position
    public void SetTileAt(int x, int y, int tileType)
    {
        if (x < 0 || x >= mapWidth || y < 0 || y >= mapHeight)
            return;
        
        levelMap[y, x] = tileType;
    }
    
    // Helper method to check if position is walkable
    public bool IsWalkable(int x, int y)
    {
        int tile = GetTileAt(x, y);
        return tile != 1; // Not a wall
    }
    
    // Get world position from grid coordinates
    public Vector3 GridToWorld(int x, int y)
    {
        return new Vector3(x, y, 0f);
    }
    
    // Get grid coordinates from world position
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
    }
}

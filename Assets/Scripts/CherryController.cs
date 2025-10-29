using UnityEngine;

public class CherryController : MonoBehaviour
{
    [Header("Cherry Settings")]
    public GameObject cherryPrefab;
    public float spawnDelay = 5f;
    public float moveSpeed = 2f;
    public int sortingOrder = 10; // Draw over other sprites
    
    [Header("Level Bounds")]
    public Vector2 levelCenter = Vector2.zero;
    public Vector2 levelSize = new Vector2(20f, 20f);
    public float spawnDistance = 5f; // Distance outside level bounds
    
    [Header("Sprite Mask")]
    public SpriteMask levelMask;
    
    [Header("Collision")]
    public LayerMask pacStudentLayer = 1;
    
    private GameObject currentCherry;
    private bool isMoving = false;
    private Vector2 startPosition;
    private Vector2 endPosition;
    private float moveStartTime;
    private float moveDuration;
    private float lastSpawnTime;
    private bool cherryDestroyed = false;
    
    void Start()
    {
        lastSpawnTime = Time.time;
        SpawnCherry();
    }
    
    void Update()
    {
        // Check if we need to spawn a new cherry
        if (currentCherry == null && !isMoving)
        {
            if (Time.time - lastSpawnTime >= spawnDelay)
            {
                SpawnCherry();
            }
        }
        
        // Update cherry movement
        if (isMoving && currentCherry != null)
        {
            UpdateCherryMovement();
        }
    }
    
    void SpawnCherry()
    {
        if (cherryPrefab == null) return;
        
        // Generate random spawn position outside level bounds
        Vector2 spawnPos = GetRandomSpawnPosition();
        
        // Instantiate cherry
        currentCherry = Instantiate(cherryPrefab, spawnPos, Quaternion.identity);
        
        // Set sorting order to draw over other sprites
        SpriteRenderer spriteRenderer = currentCherry.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = sortingOrder;
        }
        
        // Apply sprite mask if available
        if (levelMask != null)
        {
            SpriteMask cherryMask = currentCherry.GetComponent<SpriteMask>();
            if (cherryMask == null)
            {
                cherryMask = currentCherry.AddComponent<SpriteMask>();
            }
            cherryMask.isMask = false; // This is the masked object
        }
        
        // Start movement
        StartCherryMovement(spawnPos);
        
        lastSpawnTime = Time.time;
        cherryDestroyed = false;
    }
    
    Vector2 GetRandomSpawnPosition()
    {
        Vector2 spawnPos;
        int attempts = 0;
        int maxAttempts = 50;
        
        do
        {
            // Randomly choose which side to spawn from
            int side = Random.Range(0, 4);
            
            switch (side)
            {
                case 0: // Top
                    spawnPos = new Vector2(
                        Random.Range(levelCenter.x - levelSize.x / 2 - spawnDistance, 
                                   levelCenter.x + levelSize.x / 2 + spawnDistance),
                        levelCenter.y + levelSize.y / 2 + spawnDistance
                    );
                    break;
                case 1: // Right
                    spawnPos = new Vector2(
                        levelCenter.x + levelSize.x / 2 + spawnDistance,
                        Random.Range(levelCenter.y - levelSize.y / 2 - spawnDistance, 
                                   levelCenter.y + levelSize.y / 2 + spawnDistance)
                    );
                    break;
                case 2: // Bottom
                    spawnPos = new Vector2(
                        Random.Range(levelCenter.x - levelSize.x / 2 - spawnDistance, 
                                   levelCenter.x + levelSize.x / 2 + spawnDistance),
                        levelCenter.y - levelSize.y / 2 - spawnDistance
                    );
                    break;
                case 3: // Left
                    spawnPos = new Vector2(
                        levelCenter.x - levelSize.x / 2 - spawnDistance,
                        Random.Range(levelCenter.y - levelSize.y / 2 - spawnDistance, 
                                   levelCenter.y + levelSize.y / 2 + spawnDistance)
                    );
                    break;
                default:
                    spawnPos = Vector2.zero;
                    break;
            }
            
            attempts++;
        } while (attempts < maxAttempts);
        
        return spawnPos;
    }
    
    void StartCherryMovement(Vector2 startPos)
    {
        startPosition = startPos;
        
        // Calculate end position (opposite side of level)
        Vector2 direction = (levelCenter - startPos).normalized;
        endPosition = levelCenter + direction * (levelSize.magnitude + spawnDistance * 2);
        
        moveStartTime = Time.time;
        moveDuration = Vector2.Distance(startPosition, endPosition) / moveSpeed;
        
        isMoving = true;
    }
    
    void UpdateCherryMovement()
    {
        if (currentCherry == null)
        {
            isMoving = false;
            return;
        }
        
        float elapsed = Time.time - moveStartTime;
        float t = elapsed / moveDuration;
        
        if (t >= 1f)
        {
            // Movement complete - destroy cherry
            DestroyCherry();
        }
        else
        {
            // Continue lerping
            Vector2 currentPos = Vector2.Lerp(startPosition, endPosition, t);
            currentCherry.transform.position = currentPos;
        }
    }
    
    void DestroyCherry()
    {
        if (currentCherry != null)
        {
            Destroy(currentCherry);
            currentCherry = null;
        }
        isMoving = false;
        cherryDestroyed = true;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if PacStudent collided with cherry
        if (other.gameObject.layer == pacStudentLayer || 
            other.CompareTag("PacStudent"))
        {
            // Handle cherry collection
            CollectCherry();
        }
    }
    
    void CollectCherry()
    {
        if (currentCherry != null && !cherryDestroyed)
        {
            // Add score or other effects here
            Debug.Log("Cherry collected!");
            
            // Destroy cherry
            DestroyCherry();
        }
    }
    
    // Public method to set level bounds from LevelGenerator
    public void SetLevelBounds(Vector2 center, Vector2 size)
    {
        levelCenter = center;
        levelSize = size;
    }
    
    // Public method to check if cherry is active
    public bool IsCherryActive()
    {
        return currentCherry != null && isMoving;
    }
    
    // Public method to get current cherry
    public GameObject GetCurrentCherry()
    {
        return currentCherry;
    }
}
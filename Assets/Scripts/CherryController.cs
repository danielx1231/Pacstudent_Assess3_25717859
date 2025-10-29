using UnityEngine;
using System.Collections;

public class CherryController : MonoBehaviour
{
    [Header("Cherry Prefab")]
    [SerializeField] private GameObject cherryPrefab;
    
    [Header("Spawn Settings")]
    [SerializeField] private float spawnDelay = 5f;
    [SerializeField] private float moveSpeed = 3f;
    
    [Header("Level Bounds")]
    [SerializeField] private Vector2 levelCenter = Vector2.zero;
    [SerializeField] private float levelWidth = 20f;
    [SerializeField] private float levelHeight = 20f;
    [SerializeField] private float spawnOffset = 2f; // Distance outside level bounds
    
    [Header("Sprite Masking")]
    [SerializeField] private SpriteMask levelMask; // Optional: for masking visibility
    
    private GameObject currentCherry;
    private float nextSpawnTime;
    
    void Start()
    {
        // Schedule first cherry spawn
        nextSpawnTime = Time.time + spawnDelay;
    }
    
    void Update()
    {
        // Check if it's time to spawn a new cherry
        if (currentCherry == null && Time.time >= nextSpawnTime)
        {
            SpawnCherry();
        }
    }
    
    void SpawnCherry()
    {
        if (cherryPrefab == null)
        {
            Debug.LogWarning("Cherry prefab is not assigned!");
            return;
        }
        
        // Determine random spawn side (0=top, 1=right, 2=bottom, 3=left)
        int spawnSide = Random.Range(0, 4);
        
        Vector2 spawnPos = GetSpawnPosition(spawnSide);
        Vector2 targetPos = GetTargetPosition(spawnSide);
        
        // Instantiate cherry
        currentCherry = Instantiate(cherryPrefab, spawnPos, Quaternion.identity);
        
        // Add movement component
        CherryMovement movement = currentCherry.AddComponent<CherryMovement>();
        movement.Initialize(spawnPos, targetPos, levelCenter, moveSpeed, this);
        
        // Set sorting order to be on top of everything
        SpriteRenderer sr = currentCherry.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 100; // High value to render above other sprites
        }
        
        // Apply sprite mask if available
        if (levelMask != null)
        {
            SpriteMask mask = currentCherry.AddComponent<SpriteMask>();
            mask.sprite = levelMask.sprite;
        }
    }
    
    Vector2 GetSpawnPosition(int side)
    {
        Vector2 pos = levelCenter;
        
        switch (side)
        {
            case 0: // Top
                pos.x = Random.Range(levelCenter.x - levelWidth / 2, levelCenter.x + levelWidth / 2);
                pos.y = levelCenter.y + levelHeight / 2 + spawnOffset;
                break;
            case 1: // Right
                pos.x = levelCenter.x + levelWidth / 2 + spawnOffset;
                pos.y = Random.Range(levelCenter.y - levelHeight / 2, levelCenter.y + levelHeight / 2);
                break;
            case 2: // Bottom
                pos.x = Random.Range(levelCenter.x - levelWidth / 2, levelCenter.x + levelWidth / 2);
                pos.y = levelCenter.y - levelHeight / 2 - spawnOffset;
                break;
            case 3: // Left
                pos.x = levelCenter.x - levelWidth / 2 - spawnOffset;
                pos.y = Random.Range(levelCenter.y - levelHeight / 2, levelCenter.y + levelHeight / 2);
                break;
        }
        
        return pos;
    }
    
    Vector2 GetTargetPosition(int side)
    {
        Vector2 pos = levelCenter;
        
        // Target is opposite side of spawn
        switch (side)
        {
            case 0: // Spawned from top, go to bottom
                pos.x = levelCenter.x + Random.Range(-levelWidth / 4, levelWidth / 4);
                pos.y = levelCenter.y - levelHeight / 2 - spawnOffset;
                break;
            case 1: // Spawned from right, go to left
                pos.x = levelCenter.x - levelWidth / 2 - spawnOffset;
                pos.y = levelCenter.y + Random.Range(-levelHeight / 4, levelHeight / 4);
                break;
            case 2: // Spawned from bottom, go to top
                pos.x = levelCenter.x + Random.Range(-levelWidth / 4, levelWidth / 4);
                pos.y = levelCenter.y + levelHeight / 2 + spawnOffset;
                break;
            case 3: // Spawned from left, go to right
                pos.x = levelCenter.x + levelWidth / 2 + spawnOffset;
                pos.y = levelCenter.y + Random.Range(-levelHeight / 4, levelHeight / 4);
                break;
        }
        
        return pos;
    }
    
    public void OnCherryDestroyed()
    {
        currentCherry = null;
        nextSpawnTime = Time.time + spawnDelay;
    }
    
    public void OnCherryCollected()
    {
        // Called when PacStudent collects the cherry
        if (currentCherry != null)
        {
            Destroy(currentCherry);
            currentCherry = null;
            nextSpawnTime = Time.time + spawnDelay;
        }
    }
}

// Separate class to handle individual cherry movement
public class CherryMovement : MonoBehaviour
{
    private Vector2 startPos;
    private Vector2 targetPos;
    private Vector2 centerPos;
    private float speed;
    private CherryController controller;
    
    private float journeyLength;
    private float startTime;
    
    public void Initialize(Vector2 start, Vector2 target, Vector2 center, float moveSpeed, CherryController cherryController)
    {
        startPos = start;
        targetPos = target;
        centerPos = center;
        speed = moveSpeed;
        controller = cherryController;
        
        // Calculate the direction to pass through center
        Vector2 direction = (targetPos - startPos).normalized;
        
        // Adjust target to ensure it passes through center
        // Find intersection point with center and extend to other side
        Vector2 toCenter = centerPos - startPos;
        float distToCenter = toCenter.magnitude;
        float totalDist = Vector2.Distance(startPos, targetPos);
        
        // Calculate journey
        journeyLength = totalDist;
        startTime = Time.time;
        
        transform.position = startPos;
    }
    
    void Update()
    {
        if (controller == null)
        {
            Destroy(gameObject);
            return;
        }
        
        // Linear interpolation from start to target
        float distCovered = (Time.time - startTime) * speed;
        float fractionOfJourney = distCovered / journeyLength;
        
        transform.position = Vector2.Lerp(startPos, targetPos, fractionOfJourney);
        
        // Check if reached target (outside level bounds)
        if (fractionOfJourney >= 1.0f)
        {
            controller.OnCherryDestroyed();
            Destroy(gameObject);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check collision with PacStudent
        if (other.CompareTag("Player"))
        {
            // Handle collision with PacStudent
            controller.OnCherryCollected();
            // Note: Controller will destroy this object
        }
    }
}

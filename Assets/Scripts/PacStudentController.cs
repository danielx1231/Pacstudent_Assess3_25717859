using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float gridSize = 1f;
    
    [Header("Audio")]
    public AudioSource moveAudioSource;
    public AudioClip movingClip;
    public AudioClip eatingClip;
    
    [Header("Animation")]
    public Animator animator;
    
    [Header("Particle Effects")]
    public DustParticleEffect dustEffect;
    
    [Header("Grid System")]
    public LayerMask wallLayerMask = -1;
    public LayerMask pelletLayerMask = 1;
    public float collisionCheckRadius = 0.4f;
    
    [Header("Game Integration")]
    public GameManager gameManager;
    
    [Header("Debug")]
    public bool showDebugGizmos = true;
    
    // Movement state
    private Vector2 lastInput;
    private Vector2 currentInput;
    private bool isMoving = false;
    private Vector2 startPosition;
    private Vector2 targetPosition;
    private float moveStartTime;
    private float moveDuration;
    
    // Grid position
    private Vector2Int currentGridPos;
    private Vector2Int targetGridPos;
    
    // Level bounds (will be set by LevelGenerator or manually)
    public Vector2Int levelSize = new Vector2Int(20, 20);
    public Vector2 levelCenter = Vector2.zero;
    
    // Grid map for collision detection (will be populated by LevelGenerator)
    private int[,] levelMap;
    
    void Start()
    {
        // Initialize grid position
        currentGridPos = WorldToGrid(transform.position);
        targetGridPos = currentGridPos;
        
        // Snap to grid center to avoid floating point issues
        Vector2 snapped = GridToWorld(currentGridPos);
        transform.position = snapped;
        
        // Set initial facing direction (right)
        if (animator != null)
        {
            animator.SetInteger("Direction", 1); // Right
        }
        
        // Initialize level map (placeholder - should be set by LevelGenerator)
        InitializeLevelMap();
        
        // Debug info
        Debug.Log($"PacStudent initialized at grid position: {currentGridPos}");
        Debug.Log($"Wall Layer Mask: {wallLayerMask.value}");
    }
    
    void Update()
    {
        // Gather input
        HandleInput();
        
        // Handle movement
        if (!isMoving)
        {
            TryMove();
        }
        else
        {
            UpdateMovement();
        }
        
        // Update animation and audio
        UpdateAnimationAndAudio();
    }
    
    void HandleInput()
    {
        Vector2 input = Vector2.zero;
        
        if (Input.GetKeyDown(KeyCode.W))
            input = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.S))
            input = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.A))
            input = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.D))
            input = Vector2.right;
        
        if (input != Vector2.zero)
        {
            lastInput = input;
            Debug.Log($"Input received: {input}");
        }
    }
    
    void TryMove()
    {
        // Try to move in lastInput direction first
        if (lastInput != Vector2.zero)
        {
            Vector2Int nextGridPos = currentGridPos + Vector2Int.RoundToInt(lastInput);
            if (IsWalkable(nextGridPos))
            {
                currentInput = lastInput;
                Debug.Log($"Moving with lastInput: {lastInput} to {nextGridPos}");
                StartMove(nextGridPos);
                return;
            }
            else
            {
                Debug.Log($"Cannot move with lastInput: {lastInput} to {nextGridPos} - blocked");
            }
        }
        
        // If lastInput doesn't work, try currentInput
        if (currentInput != Vector2.zero)
        {
            Vector2Int nextGridPos = currentGridPos + Vector2Int.RoundToInt(currentInput);
            if (IsWalkable(nextGridPos))
            {
                Debug.Log($"Moving with currentInput: {currentInput} to {nextGridPos}");
                StartMove(nextGridPos);
                return;
            }
            else
            {
                Debug.Log($"Cannot move with currentInput: {currentInput} to {nextGridPos} - blocked");
            }
        }
        
        // If neither works, stop moving
        currentInput = Vector2.zero;
    }
    
    void StartMove(Vector2Int nextGridPos)
    {
        isMoving = true;
        startPosition = transform.position;
        targetGridPos = nextGridPos;
        targetPosition = GridToWorld(nextGridPos);
        
        moveStartTime = Time.time;
        moveDuration = gridSize / moveSpeed;
        
        // Start dust particles
        if (dustEffect != null)
        {
            dustEffect.StartDustEffect();
            dustEffect.UpdateParticleDirection(currentInput);
        }
    }
    
    void UpdateMovement()
    {
        float elapsed = Time.time - moveStartTime;
        float t = elapsed / moveDuration;
        
        if (t >= 1f)
        {
            // Movement complete
            transform.position = targetPosition;
            currentGridPos = targetGridPos;
            isMoving = false;
            
            // Check if we reached a pellet
            if (HasPellet(currentGridPos))
            {
                OnPelletEaten();
            }
            
            // Stop dust particles
            if (dustEffect != null)
            {
                dustEffect.StopDustEffect();
            }
        }
        else
        {
            // Continue lerping
            transform.position = Vector2.Lerp(startPosition, targetPosition, t);
        }
    }
    
    void UpdateAnimationAndAudio()
    {
        Vector2 direction = isMoving ? currentInput : lastInput;
        
        // Update animator direction
        if (animator != null && direction != Vector2.zero)
        {
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                animator.SetInteger("Direction", direction.x > 0 ? 1 : 3); // Right : Left
            }
            else
            {
                animator.SetInteger("Direction", direction.y > 0 ? 0 : 2); // Up : Down
            }
        }
        
        // Update audio
        if (moveAudioSource != null)
        {
            if (isMoving)
            {
                // Check if next position has a pellet
                Vector2Int nextPos = currentGridPos + Vector2Int.RoundToInt(currentInput);
                bool hasPellet = HasPellet(nextPos);
                
                AudioClip clipToPlay = hasPellet ? eatingClip : movingClip;
                
                if (moveAudioSource.clip != clipToPlay)
                {
                    moveAudioSource.clip = clipToPlay;
                    moveAudioSource.loop = true;
                    moveAudioSource.Play();
                }
            }
            else
            {
                if (moveAudioSource.isPlaying)
                {
                    moveAudioSource.Stop();
                }
            }
        }
    }
    
    bool IsWalkable(Vector2Int gridPos)
    {
        // Check bounds
        if (gridPos.x < 0 || gridPos.x >= levelSize.x || 
            gridPos.y < 0 || gridPos.y >= levelSize.y)
        {
            Debug.Log($"Position {gridPos} is out of bounds");
            return false;
        }
        
        // Check level map first
        if (levelMap != null)
        {
            int tileType = levelMap[gridPos.x, gridPos.y];
            bool isWalkable = tileType == 0 || tileType == 2;
            Debug.Log($"Level map check: {gridPos} = {tileType}, walkable: {isWalkable}");
            return isWalkable;
        }
        
        // Fallback to physics check - use multiple detection methods
        Vector2 worldPos = GridToWorld(gridPos);
        
        // Method 1: Check target position
        Collider2D hit1 = Physics2D.OverlapCircle(worldPos, collisionCheckRadius, wallLayerMask);
        
        // Method 2: Check midpoint between current and target
        Vector2 currentWorldPos = GridToWorld(currentGridPos);
        Vector2 midPoint = Vector2.Lerp(currentWorldPos, worldPos, 0.5f);
        Collider2D hit2 = Physics2D.OverlapCircle(midPoint, collisionCheckRadius, wallLayerMask);
        
        // Method 3: BoxCast from current to target
        Vector2 dir = (worldPos - currentWorldPos).normalized;
        float dist = Vector2.Distance(currentWorldPos, worldPos);
        RaycastHit2D hit3 = Physics2D.BoxCast(currentWorldPos, Vector2.one * collisionCheckRadius, 0f, dir, dist * 0.9f, wallLayerMask);
        
        bool isBlocked = hit1 != null || hit2 != null || hit3.collider != null;
        
        if (isBlocked)
        {
            Debug.Log($"Physics check blocked at {gridPos}. Hit1: {hit1}, Hit2: {hit2}, Hit3: {hit3.collider}");
        }
        
        return !isBlocked;
    }
    
    bool HasPellet(Vector2Int gridPos)
    {
        if (gridPos.x < 0 || gridPos.x >= levelSize.x || 
            gridPos.y < 0 || gridPos.y >= levelSize.y)
        {
            return false;
        }
        
        Vector2 worldPos = GridToWorld(gridPos);
        Collider2D pelletCollider = Physics2D.OverlapCircle(worldPos, 0.1f, pelletLayerMask);
        return pelletCollider != null;
    }
    
    Vector2Int WorldToGrid(Vector2 worldPos)
    {
        Vector2 localPos = worldPos - levelCenter;
        return new Vector2Int(
            Mathf.RoundToInt(localPos.x / gridSize),
            Mathf.RoundToInt(localPos.y / gridSize)
        );
    }
    
    Vector2 GridToWorld(Vector2Int gridPos)
    {
        return levelCenter + new Vector2(gridPos.x * gridSize, gridPos.y * gridSize);
    }
    
    void InitializeLevelMap()
    {
        // Placeholder level map - should be replaced by LevelGenerator
        levelMap = new int[levelSize.x, levelSize.y];
        
        // Create a simple test level with walls around the edges
        for (int x = 0; x < levelSize.x; x++)
        {
            for (int y = 0; y < levelSize.y; y++)
            {
                if (x == 0 || x == levelSize.x - 1 || y == 0 || y == levelSize.y - 1)
                {
                    levelMap[x, y] = 1; // Wall
                }
                else
                {
                    levelMap[x, y] = 0; // Walkable
                }
            }
        }
    }
    
    // Public method to set level map from LevelGenerator
    public void SetLevelMap(int[,] map, Vector2Int size, Vector2 center)
    {
        levelMap = map;
        levelSize = size;
        levelCenter = center;
    }
    
    // Public method to get current grid position
    public Vector2Int GetCurrentGridPosition()
    {
        return currentGridPos;
    }
    
    // Public method to check if moving
    public bool IsMoving()
    {
        return isMoving;
    }
    
    void OnPelletEaten()
    {
        // Notify game manager
        if (gameManager != null)
        {
            gameManager.OnPelletEaten(currentGridPos);
        }
        
        // Play eating sound effect
        if (moveAudioSource != null && eatingClip != null)
        {
            moveAudioSource.PlayOneShot(eatingClip);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check for cherry collision
        if (other.CompareTag("Cherry"))
        {
            if (gameManager != null)
            {
                gameManager.OnCherryCollected();
            }
            Destroy(other.gameObject);
        }
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!showDebugGizmos) return;
        
        // Draw current grid position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(GridToWorld(WorldToGrid(transform.position)), 0.1f);
        
        if (Application.isPlaying)
        {
            // Current grid position
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(GridToWorld(currentGridPos), 0.12f);
            
            // Target grid position
            if (isMoving)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(targetPosition, 0.12f);
                
                // Draw movement path
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, targetPosition);
            }
            
            // Draw collision check radius
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, collisionCheckRadius);
            
            // Draw grid lines
            Gizmos.color = Color.white;
            for (int x = 0; x < levelSize.x; x++)
            {
                for (int y = 0; y < levelSize.y; y++)
                {
                    Vector2 pos = GridToWorld(new Vector2Int(x, y));
                    Gizmos.DrawWireCube(pos, Vector2.one * gridSize * 0.9f);
                }
            }
        }
    }
#endif
}
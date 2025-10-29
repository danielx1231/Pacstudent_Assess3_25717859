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
    public LayerMask wallLayerMask = 1;
    public LayerMask pelletLayerMask = 1;
    
    [Header("Game Integration")]
    public GameManager gameManager;
    
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
        
        // Set initial facing direction (right)
        if (animator != null)
        {
            animator.SetInteger("Direction", 1); // Right
        }
        
        // Initialize level map (placeholder - should be set by LevelGenerator)
        InitializeLevelMap();
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
                StartMove(nextGridPos);
                return;
            }
        }
        
        // If lastInput doesn't work, try currentInput
        if (currentInput != Vector2.zero)
        {
            Vector2Int nextGridPos = currentGridPos + Vector2Int.RoundToInt(currentInput);
            if (IsWalkable(nextGridPos))
            {
                StartMove(nextGridPos);
                return;
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
            return false;
        }
        
        // Check level map
        if (levelMap != null)
        {
            int tileType = levelMap[gridPos.x, gridPos.y];
            // 0 = walkable, 1 = wall, 2 = ghost wall (special case)
            return tileType == 0 || tileType == 2;
        }
        
        // Fallback to physics check
        Vector2 worldPos = GridToWorld(gridPos);
        Collider2D wallCollider = Physics2D.OverlapCircle(worldPos, 0.1f, wallLayerMask);
        return wallCollider == null;
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
}
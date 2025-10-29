using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private LevelGenerator levelGenerator;
    
    [Header("Animation & Audio")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource movementAudioSource;
    [SerializeField] private AudioClip movingClip;
    [SerializeField] private AudioClip eatingPelletClip;
    
    [Header("Particle System")]
    [SerializeField] private ParticleSystem dustParticles;
    
    // Input variables
    private KeyCode lastInput = KeyCode.None;
    private KeyCode currentInput = KeyCode.None;
    
    // Lerp variables
    private bool isLerping = false;
    private Vector2 currentGridPos;
    private Vector2 targetGridPos;
    private float lerpStartTime;
    private float lerpDuration;
    
    // Direction enum for clarity
    private enum Direction { Up = 0, Right = 1, Down = 2, Left = 3, None = -1 }
    private Direction currentDirection = Direction.Right;
    
    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        
        if (movementAudioSource == null)
            movementAudioSource = GetComponent<AudioSource>();
            
        if (levelGenerator == null)
            levelGenerator = FindObjectOfType<LevelGenerator>();
        
        // Initialize to current grid position
        currentGridPos = WorldToGrid(transform.position);
        targetGridPos = currentGridPos;
        
        // Set initial facing direction to right
        if (animator != null)
            animator.SetInteger("Direction", (int)Direction.Right);
    }
    
    void Update()
    {
        // Gather player input
        GatherInput();
        
        // Handle movement
        if (!isLerping)
        {
            // Try to move based on lastInput first
            if (lastInput != KeyCode.None && TryStartMove(lastInput))
            {
                currentInput = lastInput;
            }
            // If lastInput direction is blocked, try currentInput
            else if (currentInput != KeyCode.None && TryStartMove(currentInput))
            {
                // Continue moving in current direction
            }
            else
            {
                // Can't move, stop
                StopMovement();
            }
        }
        else
        {
            // Continue lerping
            UpdateLerp();
        }
    }
    
    void GatherInput()
    {
        // Store the last key pressed without overriding unless new input
        if (Input.GetKeyDown(KeyCode.W))
            lastInput = KeyCode.W;
        else if (Input.GetKeyDown(KeyCode.A))
            lastInput = KeyCode.A;
        else if (Input.GetKeyDown(KeyCode.S))
            lastInput = KeyCode.S;
        else if (Input.GetKeyDown(KeyCode.D))
            lastInput = KeyCode.D;
    }
    
    bool TryStartMove(KeyCode direction)
    {
        Vector2 nextGridPos = GetNextGridPosition(direction);
        
        if (IsWalkable(nextGridPos))
        {
            // Start lerping to the next position
            targetGridPos = nextGridPos;
            lerpStartTime = Time.time;
            
            // Calculate lerp duration based on distance and speed
            float distance = Vector2.Distance(currentGridPos, targetGridPos);
            lerpDuration = distance / moveSpeed;
            
            isLerping = true;
            
            // Update direction for animation
            UpdateDirection(direction);
            
            // Start movement effects
            StartMovement(nextGridPos);
            
            return true;
        }
        
        return false;
    }
    
    void UpdateLerp()
    {
        float elapsed = Time.time - lerpStartTime;
        float t = Mathf.Clamp01(elapsed / lerpDuration);
        
        // Linear interpolation
        Vector3 startPos = GridToWorld(currentGridPos);
        Vector3 endPos = GridToWorld(targetGridPos);
        transform.position = Vector3.Lerp(startPos, endPos, t);
        
        // Check if lerp is complete
        if (t >= 1f)
        {
            isLerping = false;
            currentGridPos = targetGridPos;
            transform.position = GridToWorld(currentGridPos);
        }
    }
    
    Vector2 GetNextGridPosition(KeyCode direction)
    {
        Vector2 nextPos = currentGridPos;
        
        switch (direction)
        {
            case KeyCode.W: // Up
                nextPos += Vector2.up;
                break;
            case KeyCode.S: // Down
                nextPos += Vector2.down;
                break;
            case KeyCode.A: // Left
                nextPos += Vector2.left;
                break;
            case KeyCode.D: // Right
                nextPos += Vector2.right;
                break;
        }
        
        return nextPos;
    }
    
    bool IsWalkable(Vector2 gridPos)
    {
        if (levelGenerator == null)
            return true; // Default to walkable if no level generator
        
        // Check if position is within bounds
        int x = Mathf.RoundToInt(gridPos.x);
        int y = Mathf.RoundToInt(gridPos.y);
        
        if (x < 0 || y < 0 || x >= levelGenerator.levelMap.GetLength(1) || y >= levelGenerator.levelMap.GetLength(0))
            return false;
        
        // Get tile type at position
        int tileType = levelGenerator.levelMap[y, x];
        
        // 0 = empty walkable space
        // 1 = wall (not walkable)
        // 2 = normal pellet (walkable)
        // 3 = power pellet (walkable)
        // 4 = ghost wall (special handling)
        // 5 = empty space (walkable)
        // 6 = ghost spawn zone (walkable)
        
        if (tileType == 1) // Wall
            return false;
        
        if (tileType == 4) // Ghost wall - check if we're inside ghost zone
        {
            // Ghost wall acts as wall for PacStudent unless passing from inside
            // For simplicity, treat as wall for PacStudent
            return IsPassingFromInside(gridPos);
        }
        
        return true; // All other tiles are walkable
    }
    
    bool IsPassingFromInside(Vector2 gridPos)
    {
        // Check if current position is inside ghost spawn zone
        // This is a simplified check - you may need to adjust based on your level layout
        int currentX = Mathf.RoundToInt(currentGridPos.x);
        int currentY = Mathf.RoundToInt(currentGridPos.y);
        
        if (levelGenerator == null)
            return false;
            
        if (currentX < 0 || currentY < 0 || 
            currentX >= levelGenerator.levelMap.GetLength(1) || 
            currentY >= levelGenerator.levelMap.GetLength(0))
            return false;
        
        // If current position is in ghost spawn zone (tile 6), allow passing through ghost wall
        return levelGenerator.levelMap[currentY, currentX] == 6;
    }
    
    void UpdateDirection(KeyCode direction)
    {
        Direction newDir = Direction.None;
        
        switch (direction)
        {
            case KeyCode.W:
                newDir = Direction.Up;
                break;
            case KeyCode.D:
                newDir = Direction.Right;
                break;
            case KeyCode.S:
                newDir = Direction.Down;
                break;
            case KeyCode.A:
                newDir = Direction.Left;
                break;
        }
        
        if (newDir != Direction.None)
        {
            currentDirection = newDir;
            if (animator != null)
                animator.SetInteger("Direction", (int)newDir);
        }
    }
    
    void StartMovement(Vector2 nextGridPos)
    {
        // Check if next position has a pellet
        bool hasNextPellet = HasPelletAt(nextGridPos);
        
        // Play appropriate movement sound
        if (movementAudioSource != null)
        {
            if (hasNextPellet && eatingPelletClip != null)
            {
                if (movementAudioSource.clip != eatingPelletClip)
                {
                    movementAudioSource.clip = eatingPelletClip;
                    movementAudioSource.loop = true;
                    movementAudioSource.Play();
                }
            }
            else if (movingClip != null)
            {
                if (movementAudioSource.clip != movingClip)
                {
                    movementAudioSource.clip = movingClip;
                    movementAudioSource.loop = true;
                    movementAudioSource.Play();
                }
            }
        }
        
        // Start particle effect
        if (dustParticles != null && !dustParticles.isPlaying)
            dustParticles.Play();
    }
    
    void StopMovement()
    {
        // Stop audio
        if (movementAudioSource != null && movementAudioSource.isPlaying)
            movementAudioSource.Stop();
        
        // Stop particles
        if (dustParticles != null && dustParticles.isPlaying)
            dustParticles.Stop();
    }
    
    bool HasPelletAt(Vector2 gridPos)
    {
        if (levelGenerator == null)
            return false;
        
        int x = Mathf.RoundToInt(gridPos.x);
        int y = Mathf.RoundToInt(gridPos.y);
        
        if (x < 0 || y < 0 || x >= levelGenerator.levelMap.GetLength(1) || y >= levelGenerator.levelMap.GetLength(0))
            return false;
        
        int tileType = levelGenerator.levelMap[y, x];
        return tileType == 2 || tileType == 3; // Normal pellet or power pellet
    }
    
    Vector2 WorldToGrid(Vector3 worldPos)
    {
        // Assuming 1 unit = 1 grid cell
        return new Vector2(Mathf.Round(worldPos.x), Mathf.Round(worldPos.y));
    }
    
    Vector3 GridToWorld(Vector2 gridPos)
    {
        // Assuming 1 unit = 1 grid cell, keep z = 0
        return new Vector3(gridPos.x, gridPos.y, 0f);
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// PacStudent - grid-to-grid lerp without external tween libs.
/// - Single Tilemap level: drag ALL wall tiles into 'Wall Tiles'.
/// - WASD input with lastInput/currentInput policy.
/// - Fixed-speed, frame-rate independent lerp.
/// - Animator: Direction(int: 0=Up,1=Right,2=Down,3=Left), IsMoving(bool).
public class PacStudentController : MonoBehaviour
{
    [Header("Scene References")]
    public Grid grid;
    public Tilemap levelTilemap;

    [Header("Which tiles count as WALLS? (drag all wall tiles here)")]
    public List<TileBase> wallTiles = new List<TileBase>();

    [Header("Movement")]
    public float unitsPerSecond = 3f;
    public bool continuousMove = true;
    public bool lockZ = true;
    public float zLockValue = 0f;

    [Header("Animation (optional)")]
    public Animator animator;
    public string directionParam = "Direction";
    public string movingParam = "IsMoving";
    public bool pauseClipWhenIdle = true;

    [Header("Start Facing")]
    public Vector2Int initialFacing = Vector2Int.right; 

    // --- runtime state ---
    private Vector2Int lastInput = Vector2Int.zero;     
    private Vector2Int currentInput = Vector2Int.zero;
    private bool isMoving = false;

    private Vector3 startPos, targetPos;
    private float moveStart, moveDuration;

    private Vector3Int currentCell, targetCell;
    private Vector3 cellSize;
    private Vector3 centerOffset;

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (!grid || !levelTilemap)
        {
            Debug.LogError("[PacStudent] Please assign Grid & Level Tilemap.");
            enabled = false; return;
        }

        cellSize = grid.cellSize;
        centerOffset = new Vector3(cellSize.x * 0.5f, cellSize.y * 0.5f, 0f);

        
        currentCell = grid.WorldToCell(transform.position);
        transform.position = CellCenterWorld(currentCell);
        targetCell = currentCell;

        if (lockZ)
        {
            var p = transform.position;
            transform.position = new Vector3(p.x, p.y, zLockValue);
        }

       
        SetAnimFacing(initialFacing);
        SetMoving(false);
    }

    void Update()
    {
        HandleInput();

        if (!isMoving) TryPlanNextStep();
        else UpdateLerp();
    }

    // ---------------- input ----------------
    private void HandleInput()
    {
        Vector2Int input = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.W)) input = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S)) input = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A)) input = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D)) input = Vector2Int.right;

        if (input != Vector2Int.zero)
        {
            lastInput = input;

            
            if (!isMoving) SetAnimFacing(lastInput);
        }
    }

    // -------------- plan next step --------------
    private void TryPlanNextStep()
    {
        
        if (lastInput != Vector2Int.zero)
        {
            var next = currentCell + (Vector3Int)AxisSnap(lastInput);
            if (IsWalkable(next))
            {
                currentInput = lastInput;
                BeginSegment(next);
                SetMoving(true);
                return;
            }
        }

        
        if (continuousMove && currentInput != Vector2Int.zero)
        {
            var next = currentCell + (Vector3Int)AxisSnap(currentInput);
            if (IsWalkable(next))
            {
                BeginSegment(next);
                SetMoving(true);
                return;
            }
        }

        
        currentInput = Vector2Int.zero;
        SetMoving(false);
    }

    private void BeginSegment(Vector3Int nextCell)
    {
        isMoving = true;
        startPos = CellCenterWorld(currentCell);
        targetCell = nextCell;
        targetPos = CellCenterWorld(nextCell);

        
        Vector2Int dir = new Vector2Int(nextCell.x - currentCell.x, nextCell.y - currentCell.y);
        SetAnimFacing(dir);

        float dist = Vector2.Distance(startPos, targetPos);
        moveDuration = dist / Mathf.Max(0.0001f, unitsPerSecond);
        moveStart = Time.time;
    }

    private void UpdateLerp()
    {
        float t = Mathf.Clamp01((Time.time - moveStart) / moveDuration);

        Vector3 p = Vector3.Lerp(startPos, targetPos, t);
        if (lockZ) p.z = zLockValue;
        transform.position = p;

        if (t >= 1f)
        {
            transform.position = targetPos;
            currentCell = targetCell;
            isMoving = false;
            SetMoving(false);
        }
    }

    // -------------- collision / grid helpers --------------
    private bool IsWalkable(Vector3Int cell)
    {
        var tile = levelTilemap.GetTile(cell);
        if (tile == null) return true;
        return !wallTiles.Contains(tile);
    }

    private Vector3 CellCenterWorld(Vector3Int cell)
    {
        return grid.CellToWorld(cell) + centerOffset;
    }

    private Vector2Int AxisSnap(Vector2Int dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return new Vector2Int(dir.x > 0 ? 1 : -1, 0);
        else
            return new Vector2Int(0, dir.y > 0 ? 1 : -1);
    }

    // -------------- animation --------------
    private void SetAnimFacing(Vector2Int dir)
    {
        if (!animator) return;

        int d;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            d = dir.x > 0 ? 1 : 3; 
        else
            d = dir.y > 0 ? 0 : 2; 

        animator.SetInteger(directionParam, d);
    }

    private void SetMoving(bool moving)
    {
        if (!animator) return;

        animator.SetBool(movingParam, moving);
        if (pauseClipWhenIdle) animator.speed = moving ? 1f : 0f;
    }
}

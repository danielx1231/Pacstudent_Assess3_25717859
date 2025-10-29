using UnityEngine;
using UnityEngine.Tilemaps;

public class PacStudentController : MonoBehaviour
{
    [Header("Grid & Collision")]
    [Tooltip("用于网格换算（World<->Cell）。若留空，将在场景中查找 GridLayout。")]
    public GridLayout gridLayout;
    [Tooltip("不可通行的墙体 Tilemap（包含普通墙与不可穿越的障碍）。")]
    public Tilemap wallTilemap;
    [Tooltip("幽灵墙 Tilemap：默认为不可通行（从外部），如需从内部穿越请在逻辑中扩展。")]
    public Tilemap ghostWallTilemap;
    [Tooltip("用于在目标格检测是否有豆子（Pellet）。需在 Inspector 指定 Pellets 图层。")]
    public LayerMask pelletMask;
    [Min(0.01f)] public float pelletDetectRadius = 0.15f;

    [Header("Movement")]
    [Min(0.01f)] public float moveSpeed = 6f; // 单位/秒
    public bool lockZ = true;
    public float zLockValue = 0f;

    [Header("Animation & SFX & VFX")]
    public Animator animator; // 期望参数：int Direction (0:Up,1:Right,2:Down,3:Left)，bool IsMoving
    public AudioSource sfxSource;
    public AudioClip movingClip;           // 不吃豆时的移动音效
    public AudioClip movingEatPelletClip;  // 即将吃到豆子时的移动音效
    public bool loopMoveSfx = true;
    public ParticleSystem moveDust;        // 跑动时的尘土粒子（可在编辑器里自定义并拖拽引用）

    // 按题目要求的成员变量名
    [HideInInspector] public Vector2Int lastInput = Vector2Int.zero;    // 仅在玩家按键时更新，不清空
    [HideInInspector] public Vector2Int currentInput = Vector2Int.zero; // 当前移动方向

    // 运行时状态
    Vector3Int currentCell;
    bool isLerping;
    Vector3 fromPos, toPos;
    float segStartTime, segDuration;

    void Awake()
    {
        if (gridLayout == null)
            gridLayout = FindObjectOfType<GridLayout>();
        if (animator == null)
            animator = GetComponent<Animator>();
        if (lockZ)
            transform.position = new Vector3(transform.position.x, transform.position.y, zLockValue);
    }

    void Start()
    {
        // 将角色对齐至最近的格中心，并面向右侧，等待输入
        if (gridLayout != null)
        {
            currentCell = WorldToCell(transform.position);
            transform.position = CellCenterWorld(currentCell);
            if (lockZ)
                transform.position = new Vector3(transform.position.x, transform.position.y, zLockValue);
        }
        SetDirectionParam(Vector2Int.right); // 面向右
        SetIsMoving(false);
        StopMoveFx();
    }

    void Update()
    {
        ReadPlayerInput();

        if (isLerping)
        {
            // 进行线性插值，帧率无关
            float t = Mathf.Clamp01((Time.time - segStartTime) / Mathf.Max(0.0001f, segDuration));
            Vector3 p = Vector3.Lerp(fromPos, toPos, t);
            if (lockZ) p.z = zLockValue;
            transform.position = p;

            if (t >= 1f)
            {
                // 到达格中心
                transform.position = toPos;
                currentCell = WorldToCell(transform.position);
                isLerping = false;
                SetIsMoving(false);
                StopMoveFx();
            }
            return;
        }

        // 未在插值：尝试基于 lastInput，然后是 currentInput 发起移动
        if (TryBeginMove(lastInput))
        {
            currentInput = lastInput;
            return;
        }
        if (TryBeginMove(currentInput))
        {
            return;
        }
        // 两个方向都不通则原地等待（不移动，不清空 lastInput）
    }

    void ReadPlayerInput()
    {
        // 只在按键瞬间更新 lastInput，且不清空
        if (Input.GetKeyDown(KeyCode.W)) lastInput = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S)) lastInput = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A)) lastInput = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D)) lastInput = Vector2Int.right;
    }

    bool TryBeginMove(Vector2Int dir)
    {
        if (dir == Vector2Int.zero || gridLayout == null) return false;
        Vector3Int nextCell = currentCell + new Vector3Int(dir.x, dir.y, 0);
        if (!IsWalkable(nextCell)) return false;

        // 计算插值参数
        fromPos = CellCenterWorld(currentCell);
        toPos = CellCenterWorld(nextCell);
        if (lockZ) { fromPos.z = zLockValue; toPos.z = zLockValue; }
        float dist = Vector2.Distance(fromPos, toPos);
        segDuration = Mathf.Max(0.0001f, dist / Mathf.Max(0.001f, moveSpeed));
        segStartTime = Time.time;
        isLerping = true;

        // 动画、朝向、音效与粒子
        SetDirectionParam(dir);
        SetIsMoving(true);
        StartMoveFx(WillEatPelletAt(nextCell));
        return true;
    }

    bool IsWalkable(Vector3Int cell)
    {
        // 普通墙体不可通过
        if (wallTilemap != null && wallTilemap.HasTile(cell))
            return false;
        // 幽灵墙：默认视为墙体（从外部不可通过）。想要从内部穿出请扩展此处判断（例如根据幽灵区 Bounds）。
        if (ghostWallTilemap != null && ghostWallTilemap.HasTile(cell))
            return false;
        return true;
    }

    bool WillEatPelletAt(Vector3Int targetCell)
    {
        if (pelletMask == 0) return false;
        Vector3 center = CellCenterWorld(targetCell);
        var hit = Physics2D.OverlapCircle(center, pelletDetectRadius, pelletMask);
        return hit != null;
    }

    Vector3Int WorldToCell(Vector3 world)
    {
        return gridLayout.WorldToCell(world);
    }

    Vector3 CellCenterWorld(Vector3Int cell)
    {
        // 使用 Tilemap 的 cellCenterWorld 更准确；若无 Tilemap，则使用 GridLayout.CellToWorld 并手动偏移
        if (wallTilemap != null) return wallTilemap.GetCellCenterWorld(cell);
        if (ghostWallTilemap != null) return ghostWallTilemap.GetCellCenterWorld(cell);
        return gridLayout.CellToWorld(cell) + new Vector3(0.5f * gridLayout.cellSize.x, 0.5f * gridLayout.cellSize.y, 0f);
    }

    void SetDirectionParam(Vector2Int dir)
    {
        if (animator == null) return;
        if (dir == Vector2Int.zero) return;
        // 与现有 PathTweener 方向编码保持一致：0:Up, 1:Right, 2:Down, 3:Left
        int code;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) code = dir.x > 0 ? 1 : 3;
        else code = dir.y > 0 ? 0 : 2;
        animator.SetInteger("Direction", code);
    }

    void SetIsMoving(bool moving)
    {
        if (animator != null)
            animator.SetBool("IsMoving", moving);
    }

    void StartMoveFx(bool aboutToEatPellet)
    {
        // 粒子
        if (moveDust != null)
        {
            if (!moveDust.isPlaying) moveDust.Play();
        }
        // 音效
        if (sfxSource != null)
        {
            var clip = aboutToEatPellet && movingEatPelletClip != null ? movingEatPelletClip : movingClip;
            if (clip != null)
            {
                if (sfxSource.clip != clip)
                {
                    sfxSource.clip = clip;
                }
                sfxSource.loop = loopMoveSfx;
                if (!sfxSource.isPlaying) sfxSource.Play();
            }
        }
    }

    void StopMoveFx()
    {
        if (moveDust != null && moveDust.isPlaying) moveDust.Stop();
        if (sfxSource != null && sfxSource.isPlaying) sfxSource.Stop();
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (gridLayout == null) return;
        var cell = gridLayout.WorldToCell(transform.position);
        var c = CellCenterWorld(cell);
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.8f);
        Gizmos.DrawWireSphere(c, 0.12f);
    }
#endif
}

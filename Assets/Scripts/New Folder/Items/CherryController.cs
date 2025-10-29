using UnityEngine;
using UnityEngine.Tilemaps;

/// Spawns and moves a bonus cherry:
/// - Spawns 5s after scene start, and 5s after previous cherry was destroyed.
/// - Spawn just OUTSIDE the tilemap world-bounds, on a random side.
/// - Moves in a straight line via linear lerp, passing through the world center of the level,
///   and exits outside the opposite side; then destroys itself.
/// - Drawn over everything (sorting layer / order).
/// - Optional SpriteMask: set cherry SpriteRenderer to VisibleInsideMask.
public class CherryController : MonoBehaviour
{
    [Header("Scene References")]
    public Tilemap levelTilemap;                 // 拖你的关卡 Tilemap
    public SpriteRenderer cherryPrefab;          // 拖“樱桃”Prefab（必须含 SpriteRenderer）

    [Header("Spawn & Movement")]
    public float spawnDelaySeconds = 5f;         // 开始/每次销毁后的等待
    public float speedUnitsPerSecond = 6f;       // 直线速度（世界单位/秒）
    public float outsideMargin = 1.0f;           // 生成/销毁时相对边界额外偏移（世界单位）

    [Header("Sorting (render on top)")]
    public string sortingLayerName = "UIWorld";  // 没有就留空，使用当前层
    public int sortingOrder = 9999;              // 非常大，盖住其他内容

    [Header("Visibility (optional)")]
    public SpriteMask levelMask;                 // 若提供，则樱桃只在 mask 内可见

    // runtime
    SpriteRenderer activeCherry;
    Vector3 startPos, endPos;
    float travelDuration, travelStartTime;
    float respawnAtTime;

    void Start()
    {
        if (!levelTilemap || !cherryPrefab)
        {
            Debug.LogError("[CherryController] Please assign levelTilemap & cherryPrefab.");
            enabled = false; return;
        }
        respawnAtTime = Time.time + spawnDelaySeconds;
    }

    void Update()
    {
        // 没有樱桃且到时间 -> 生成
        if (!activeCherry && Time.time >= respawnAtTime)
        {
            SpawnCherry();
        }

        // 有樱桃则更新 Lerp
        if (activeCherry)
        {
            float t = Mathf.Clamp01((Time.time - travelStartTime) / travelDuration);
            activeCherry.transform.position = Vector3.Lerp(startPos, endPos, t);

            // 到达目的地 -> 销毁并开始下次计时
            if (t >= 1f)
            {
                Destroy(activeCherry.gameObject);
                activeCherry = null;
                respawnAtTime = Time.time + spawnDelaySeconds;
            }
        }
    }

    void SpawnCherry()
    {
        // 计算 Tilemap 的世界包围盒
        Bounds worldBounds = GetTilemapWorldBounds(levelTilemap);
        Vector3 center = worldBounds.center;

        // 随机选择一侧：0=Left,1=Right,2=Bottom,3=Top
        int side = Random.Range(0, 4);

        // 生成点：边界外一点
        switch (side)
        {
            case 0: // Left
                startPos = new Vector3(worldBounds.min.x - outsideMargin,
                                       Random.Range(worldBounds.min.y, worldBounds.max.y),
                                       0f);
                break;
            case 1: // Right
                startPos = new Vector3(worldBounds.max.x + outsideMargin,
                                       Random.Range(worldBounds.min.y, worldBounds.max.y),
                                       0f);
                break;
            case 2: // Bottom
                startPos = new Vector3(Random.Range(worldBounds.min.x, worldBounds.max.x),
                                       worldBounds.min.y - outsideMargin,
                                       0f);
                break;
            default: // Top
                startPos = new Vector3(Random.Range(worldBounds.min.x, worldBounds.max.x),
                                       worldBounds.max.y + outsideMargin,
                                       0f);
                break;
        }

        // 终点：相对中心做“镜像”，再加同样的 outsideMargin
        // 确保轨迹必过 center，并从另一侧飞出
        Vector3 mirrored = center + (center - startPos);
        Vector3 dir = (mirrored - center).normalized;
        // 把终点再往外推一点，避免停在边上
        endPos = mirrored + dir * outsideMargin;

        // 实例化 + 渲染层设置
        activeCherry = Instantiate(cherryPrefab, startPos, Quaternion.identity, transform);
        var sr = activeCherry; // SpriteRenderer
        if (!string.IsNullOrEmpty(sortingLayerName)) sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = sortingOrder;

        // 只在 Mask 内可见（可选）
        if (levelMask)
            sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        else
            sr.maskInteraction = SpriteMaskInteraction.None;

        // 计算时长（线性 Lerp）
        float dist = Vector2.Distance(startPos, endPos);
        travelDuration = dist / Mathf.Max(0.0001f, speedUnitsPerSecond);
        travelStartTime = Time.time;
    }

    // 把 Tilemap 的 localBounds 转成世界坐标 Bounds
    static Bounds GetTilemapWorldBounds(Tilemap tm)
    {
        // local bounds -> world space
        Bounds lb = tm.localBounds; // 注意：包含 cellBounds 的几何尺寸
        Vector3 min = tm.transform.TransformPoint(lb.min);
        Vector3 max = tm.transform.TransformPoint(lb.max);
        Bounds wb = new Bounds();
        wb.SetMinMax(Vector3.Min(min, max), Vector3.Max(min, max));
        // 锁定 z=0 的 2D 场景
        wb.min = new Vector3(wb.min.x, wb.min.y, 0f);
        wb.max = new Vector3(wb.max.x, wb.max.y, 0f);
        return wb;
    }
}

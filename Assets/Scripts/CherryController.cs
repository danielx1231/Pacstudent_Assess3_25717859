using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CherryController : MonoBehaviour
{
    [Header("Cherry Prefab & Visuals")]
    public GameObject cherryPrefab;                 // 你的樱桃预制体（仅 SpriteRenderer 即可）
    public int cherrySortingOrder = 10000;          // 高绘制顺序，保证覆盖所有精灵

    [Header("Spawn & Movement")]
    [Min(0.1f)] public float spawnDelaySeconds = 5f; // 场景开始等待 5s，之后每个樱桃销毁后再等 5s
    [Min(0.01f)] public float cherrySpeed = 5f;      // 单位/秒（线性插值）
    [Tooltip("从关卡边界外生成/销毁的额外边距（世界单位）")]
    public float outsideMargin = 1f;

    [Header("Mask (可选)")]
    [Tooltip("场景内的 SpriteMask（可选）。如提供，樱桃会仅在遮罩内可见。")]
    public SpriteMask levelSpriteMask;

    GameObject currentCherry;
    Coroutine loopCo;

    void OnEnable()
    {
        if (loopCo == null) loopCo = StartCoroutine(SpawnLoop());
    }

    void OnDisable()
    {
        if (loopCo != null) StopCoroutine(loopCo);
        loopCo = null;
    }

    IEnumerator SpawnLoop()
    {
        // 首次进入场景，先等 5 秒
        yield return new WaitForSeconds(Mathf.Max(0f, spawnDelaySeconds));

        while (enabled && gameObject.activeInHierarchy)
        {
            // 生成并移动一次
            yield return SpawnAndRunOnce();
            // 销毁后等待 5 秒再来一次
            yield return new WaitForSeconds(Mathf.Max(0f, spawnDelaySeconds));
        }
    }

    IEnumerator SpawnAndRunOnce()
    {
        if (cherryPrefab == null)
            yield break;

        // 计算关卡边界与中心
        Bounds levelBounds;
        if (!TryGetLevelBounds(out levelBounds))
        {
            // 没有 Tilemap，使用主摄像机可视区域近似
            levelBounds = CameraBoundsApprox();
        }
        Vector3 center = levelBounds.center;

        // 随机选择一个边：0左 1右 2下 3上
        int side = Random.Range(0, 4);
        Vector3 start, end;
        BuildStartEndThroughCenter(levelBounds, side, outsideMargin, out start, out end);

        currentCherry = Instantiate(cherryPrefab);
        currentCherry.name = "Cherry (Runtime)";
        var sr = currentCherry.GetComponentInChildren<SpriteRenderer>() ?? currentCherry.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = cherrySortingOrder;
            if (levelSpriteMask != null)
                sr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }

        // 放在起点
        currentCherry.transform.position = start;

        // 开始直线穿越（线性插值）
        yield return MoveCherryLinearly(currentCherry, start, end, cherrySpeed);

        // 超出另一侧边界后销毁
        if (currentCherry != null)
        {
            Destroy(currentCherry);
            currentCherry = null;
        }
    }

    IEnumerator MoveCherryLinearly(GameObject target, Vector3 from, Vector3 to, float speed)
    {
        if (target == null) yield break;
        float distance = Vector2.Distance(from, to);
        float duration = Mathf.Max(0.0001f, distance / Mathf.Max(0.001f, speed));
        float startTime = Time.time;

        while (target != null)
        {
            float t = Mathf.Clamp01((Time.time - startTime) / duration);
            target.transform.position = Vector3.Lerp(from, to, t);
            if (t >= 1f) break;
            yield return null;
        }
    }

    bool TryGetLevelBounds(out Bounds bounds)
    {
        // 以场景中所有 TilemapRenderer 的 bounds 合并作为关卡边界
        var renderers = FindObjectsOfType<TilemapRenderer>();
        if (renderers == null || renderers.Length == 0)
        {
            bounds = default;
            return false;
        }
        bool init = false;
        Bounds b = default;
        foreach (var r in renderers)
        {
            if (!r.enabled) continue;
            if (!init) { b = r.bounds; init = true; }
            else { b.Encapsulate(r.bounds); }
        }
        if (!init) { bounds = default; return false; }
        bounds = b;
        return true;
    }

    Bounds CameraBoundsApprox()
    {
        var cam = Camera.main;
        if (cam == null || !cam.orthographic)
            return new Bounds(Vector3.zero, new Vector3(30, 18, 1)); // 兜底
        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;
        return new Bounds(cam.transform.position, new Vector3(width, height, 1f));
    }

    static void BuildStartEndThroughCenter(Bounds b, int side, float margin, out Vector3 start, out Vector3 end)
    {
        var c = b.center;
        switch (side)
        {
            case 0: // 左->右，穿过中心
                start = new Vector3(b.min.x - margin, c.y, 0f);
                end   = new Vector3(b.max.x + margin, c.y, 0f);
                break;
            case 1: // 右->左
                start = new Vector3(b.max.x + margin, c.y, 0f);
                end   = new Vector3(b.min.x - margin, c.y, 0f);
                break;
            case 2: // 下->上
                start = new Vector3(c.x, b.min.y - margin, 0f);
                end   = new Vector3(c.x, b.max.y + margin, 0f);
                break;
            default: // 上->下
                start = new Vector3(c.x, b.max.y + margin, 0f);
                end   = new Vector3(c.x, b.min.y - margin, 0f);
                break;
        }
    }
}

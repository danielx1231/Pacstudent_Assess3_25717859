using UnityEngine;

public class PathTweener : MonoBehaviour
{
    [Header("Path (clockwise)")]
    public Transform[] points;
    [Min(0.01f)] public float speed = 3f;
    public bool loop = true;

    [Header("Animation & SFX (optional)")]
    public Animator animator;
    public AudioSource moveSfxSource;
    public AudioClip moveClip;
    public bool loopMoveSfx = true;

    [Header("Z Lock")]
    public bool lockZ = true;
    public float zLockValue = 0f;

    [Header("Gizmos")]
    public bool drawPathGizmos = true;

    int fromIndex, toIndex;
    float segStartTime, segDuration;
    Vector3 fromPos, toPos;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (lockZ) transform.position = new Vector3(transform.position.x, transform.position.y, zLockValue);
    }

    void OnEnable()
    {
        if (points == null || points.Length < 2) return;
        fromIndex = 0; toIndex = 1;
        BeginSegment();
        StartMoveSfx();
    }

    void OnDisable() => StopMoveSfx();

    Vector3 Sanitize(Vector3 v)
    {
        if (lockZ) v.z = zLockValue;
        if (!float.IsFinite(v.x) || !float.IsFinite(v.y) || !float.IsFinite(v.z))
            v = new Vector3(0, 0, zLockValue);
        return v;
    }

    void BeginSegment()
    {
        fromPos = Sanitize(points[fromIndex].position);
        toPos = Sanitize(points[toIndex].position);
        float dist = Vector2.Distance(fromPos, toPos);          
        segDuration = Mathf.Max(0.0001f, dist / Mathf.Max(0.001f, speed));
        segStartTime = Time.time;
        UpdateDirectionParam(toPos - fromPos);
    }

    void Update()
    {
        if (points == null || points.Length < 2) return;

        float t = Mathf.Clamp01((Time.time - segStartTime) / segDuration);
        Vector3 p = Vector3.Lerp(fromPos, toPos, t);
        transform.position = Sanitize(p);

        if (t >= 1f)
        {
            fromIndex = toIndex;
            toIndex++;
            if (toIndex >= points.Length)
            {
                if (loop) toIndex = 0;
                else { enabled = false; StopMoveSfx(); return; }
            }
            BeginSegment();
        }
    }

    void UpdateDirectionParam(Vector3 dir)
    {
        if (animator == null) return;
        if (dir.sqrMagnitude < 1e-6f) return;
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            animator.SetInteger("Direction", dir.x > 0 ? 1 : 3);
        else
            animator.SetInteger("Direction", dir.y > 0 ? 0 : 2);
    }

    void StartMoveSfx()
    {
        if (moveSfxSource && moveClip)
        {
            moveSfxSource.clip = moveClip;
            moveSfxSource.loop = loopMoveSfx;
            if (!moveSfxSource.isPlaying) moveSfxSource.Play();
        }
    }
    void StopMoveSfx() { if (moveSfxSource && moveSfxSource.isPlaying) moveSfxSource.Stop(); }

#if UNITY_EDITOR
    void OnValidate()   
    {
        if (lockZ)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, zLockValue);
            if (points != null)
                for (int i = 0; i < points.Length; i++)
                    if (points[i])
                        points[i].position = new Vector3(points[i].position.x, points[i].position.y, zLockValue);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!drawPathGizmos || points == null || points.Length < 2) return;

        Gizmos.color = new Color(1f, 0.7f, 0.1f, 0.9f);
        for (int i = 0; i < points.Length; i++)
        {
            var aT = points[i];
            if (!aT) continue;
            var a = Sanitize(aT.position);
            if (!float.IsFinite(a.x) || !float.IsFinite(a.y) || !float.IsFinite(a.z)) continue;

            Gizmos.DrawSphere(a, 0.08f);

            int j = (i + 1) % points.Length;
            if (j >= points.Length || !points[j]) continue;
            var b = Sanitize(points[j].position);
            if (!float.IsFinite(b.x) || !float.IsFinite(b.y) || !float.IsFinite(b.z)) continue;

            Gizmos.DrawLine(a, b);
        }
    }
#endif
}

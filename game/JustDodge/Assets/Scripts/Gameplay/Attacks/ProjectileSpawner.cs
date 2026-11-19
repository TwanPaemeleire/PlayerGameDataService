using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

public class ProjectileSpawner : MonoBehaviour
{
    public static ProjectileSpawner Instance { get; private set; }

    [Serializable]
    private struct PrewarmEntry
    {
        public ProjectileBase Prefab;
        public int Count;
    }

    [Header("Pools")]
    [SerializeField] private int _defaultCapacity = 32;
    [SerializeField] private int _maxPoolSize = 1000;
    [SerializeField] private List<PrewarmEntry> _prewarm = new List<PrewarmEntry>();

    [Header("Auto-despawn")]
    [SerializeField] private ArenaBounds _arenaBounds;
    [SerializeField] private float _despawnMargin = 1.0f;

    [Header("Fade out")]
    [SerializeField] private float _defaultFadeDuration = 0.4f;
    public UnityEvent OnFadeOutFinished = new UnityEvent();

    private sealed class ProjectileVisuals
    {
        public SpriteRenderer[] Renderers;
        public Color[] OriginalColors;
        public Collider2D[] Colliders;
    }

    private readonly Dictionary<ProjectileBase, ObjectPool<ProjectileBase>> _pools = new Dictionary<ProjectileBase, ObjectPool<ProjectileBase>>();
    private readonly Dictionary<ProjectileBase, ObjectPool<ProjectileBase>> _poolOfInstance = new Dictionary<ProjectileBase, ObjectPool<ProjectileBase>>();
    private readonly Dictionary<ProjectileBase, ProjectileVisuals> _visuals = new Dictionary<ProjectileBase, ProjectileVisuals>();
    private readonly HashSet<ProjectileBase> _active = new HashSet<ProjectileBase>();
    private readonly List<ProjectileBase> _despawnBuffer = new List<ProjectileBase>();

    private float _fadeAlpha = 1f;
    private bool _fadeActive = false;
    private Coroutine _fadeRoutine;

    public int ActiveCount => _active.Count;
    public bool IsFadedOrFading => _fadeActive;
    public float FadeAlpha => _fadeAlpha;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("A second ProjectileSpawner was found; destroying the new one.", this);
            Destroy(gameObject);
            return;
        }
        Instance = this;

        foreach (PrewarmEntry entry in _prewarm)
        {
            Prewarm(entry.Prefab, entry.Count);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (_arenaBounds == null || _active.Count == 0) return;

        Vector2 min = _arenaBounds.Min - Vector2.one * _despawnMargin;
        Vector2 max = _arenaBounds.Max + Vector2.one * _despawnMargin;

        _despawnBuffer.Clear();
        foreach (ProjectileBase projectile in _active)
        {
            Vector2 pos = projectile.transform.position;
            if (pos.x < min.x || pos.x > max.x || pos.y < min.y || pos.y > max.y)
            {
                _despawnBuffer.Add(projectile);
            }
        }
        foreach (ProjectileBase projectile in _despawnBuffer)
        {
            Despawn(projectile);
        }
    }

    public T Spawn<T>(T prefab, Vector2 position, Vector2 direction, float difficultyMultiplier = 1f)
        where T : ProjectileBase
    {
        if (prefab == null)
        {
            Debug.LogError("ProjectileSpawner.Spawn was called with a null prefab.", this);
            return null;
        }

        ObjectPool<ProjectileBase> pool = GetOrCreatePool(prefab);
        ProjectileBase instance = pool.Get();

        ApplyFade(instance);

        float angle = (direction.sqrMagnitude > 0f) ? Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg : 0f;
        instance.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, angle));
        instance.gameObject.SetActive(true);

        _active.Add(instance);
        instance.InitializeProjectile(difficultyMultiplier);

        return (T)instance;
    }

    public void Despawn(ProjectileBase instance)
    {
        if (instance == null || !_active.Remove(instance)) return;

        if (_poolOfInstance.TryGetValue(instance, out ObjectPool<ProjectileBase> pool))
        {
            pool.Release(instance);
        }
        else
        {
            Destroy(instance.gameObject);
        }
    }

    public void DespawnAll()
    {
        _despawnBuffer.Clear();
        _despawnBuffer.AddRange(_active);
        foreach (ProjectileBase projectile in _despawnBuffer)
        {
            Despawn(projectile);
        }
    }

    public void FadeOutAll()
    {
        FadeOutAll(_defaultFadeDuration);
    }

    public void FadeOutAll(float duration)
    {
        _fadeActive = true;
        ApplyFadeToAllActive();

        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeRoutine(duration));
    }

    public void ResetFade()
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = null;
        _fadeActive = false;
        _fadeAlpha = 1f;
        ApplyFadeToAllActive();
    }

    private IEnumerator FadeRoutine(float duration)
    {
        float startAlpha = _fadeAlpha;
        float elapsed = 0f;

        while (duration > 0f && elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _fadeAlpha = Mathf.Lerp(startAlpha, 0f, Mathf.Clamp01(elapsed / duration));
            ApplyFadeToAllActive();
            yield return null;
        }

        _fadeAlpha = 0f;
        ApplyFadeToAllActive();
        _fadeRoutine = null;
        OnFadeOutFinished.Invoke();
    }

    private void ApplyFadeToAllActive()
    {
        foreach (ProjectileBase projectile in _active)
        {
            ApplyFade(projectile);
        }
    }

    private void ApplyFade(ProjectileBase instance)
    {
        if (!_visuals.TryGetValue(instance, out ProjectileVisuals visuals)) return;

        float factor = _fadeActive ? _fadeAlpha : 1f;
        for (int i = 0; i < visuals.Renderers.Length; i++)
        {
            Color c = visuals.OriginalColors[i];
            c.a *= factor;
            visuals.Renderers[i].color = c;
        }

        bool collidersOn = !_fadeActive;
        foreach (Collider2D col in visuals.Colliders)
        {
            col.enabled = collidersOn;
        }
    }

    private void CacheVisuals(ProjectileBase instance)
    {
        SpriteRenderer[] renderers = instance.GetComponentsInChildren<SpriteRenderer>(true);
        Color[] colors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            colors[i] = renderers[i].color;
        }

        _visuals[instance] = new ProjectileVisuals
        {
            Renderers = renderers,
            OriginalColors = colors,
            Colliders = instance.GetComponentsInChildren<Collider2D>(true)
        };
    }

    public void Prewarm(ProjectileBase prefab, int count)
    {
        if (prefab == null || count <= 0) return;

        ObjectPool<ProjectileBase> pool = GetOrCreatePool(prefab);
        var temp = new List<ProjectileBase>(count);
        for (int i = 0; i < count; i++)
        {
            temp.Add(pool.Get());
        }
        foreach (ProjectileBase instance in temp)
        {
            pool.Release(instance);
        }
    }

    private ObjectPool<ProjectileBase> GetOrCreatePool(ProjectileBase prefab)
    {
        if (_pools.TryGetValue(prefab, out ObjectPool<ProjectileBase> existing)) return existing;

        Transform container = new GameObject($"Pool - {prefab.name}").transform;
        container.SetParent(transform, false);

        ObjectPool<ProjectileBase> pool = null;
        pool = new ObjectPool<ProjectileBase>(
            createFunc: () =>
            {
                ProjectileBase instance = Instantiate(prefab, container);
                instance.gameObject.SetActive(false);
                _poolOfInstance[instance] = pool;
                CacheVisuals(instance);
                return instance;
            },
            actionOnGet: null,
            actionOnRelease: instance => instance.gameObject.SetActive(false),
            actionOnDestroy: instance =>
            {
                _poolOfInstance.Remove(instance);
                _visuals.Remove(instance);
                if (instance != null) Destroy(instance.gameObject);
            },
            collectionCheck: true,
            defaultCapacity: _defaultCapacity,
            maxSize: _maxPoolSize);

        _pools[prefab] = pool;
        return pool;
    }
}

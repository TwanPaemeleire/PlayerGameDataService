using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectileSpawner : MonoSingleton<ProjectileSpawner>
{
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

    [Header("Auto-despawn (optional)")]
    [Tooltip("If set, projectiles that leave the arena by more than the margin are despawned automatically.")]
    [SerializeField] private ArenaBounds _arenaBounds;
    [SerializeField] private float _despawnMargin = 1.0f;

    private readonly Dictionary<ProjectileBase, ObjectPool<ProjectileBase>> _pools = new Dictionary<ProjectileBase, ObjectPool<ProjectileBase>>();
    private readonly Dictionary<ProjectileBase, ObjectPool<ProjectileBase>> _poolOfInstance = new Dictionary<ProjectileBase, ObjectPool<ProjectileBase>>();
    private readonly HashSet<ProjectileBase> _active = new HashSet<ProjectileBase>();
    private readonly List<ProjectileBase> _despawnBuffer = new List<ProjectileBase>();

    public int ActiveCount => _active.Count;

    private void Awake()
    {
        foreach (PrewarmEntry entry in _prewarm)
        {
            Prewarm(entry.Prefab, entry.Count);
        }
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

    public T Spawn<T>(T prefab, Vector2 position, Vector2 direction, float speedMultiplier = 1f)
        where T : ProjectileBase
    {
        if (prefab == null)
        {
            Debug.LogError("ProjectileSpawner.Spawn was called with a null prefab.", this);
            return null;
        }

        ObjectPool<ProjectileBase> pool = GetOrCreatePool(prefab);
        ProjectileBase instance = pool.Get();

        float angle = (direction.sqrMagnitude > 0f) ? Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg : 0f;
        instance.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, 0f, angle));
        instance.gameObject.SetActive(true);

        _active.Add(instance);
        instance.InitializeProjectile(speedMultiplier);

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
                return instance;
            },
            actionOnGet: null,
            actionOnRelease: instance => instance.gameObject.SetActive(false),
            actionOnDestroy: instance =>
            {
                _poolOfInstance.Remove(instance);
                if (instance != null) Destroy(instance.gameObject);
            },
            collectionCheck: true,
            defaultCapacity: _defaultCapacity,
            maxSize: _maxPoolSize);

        _pools[prefab] = pool;
        return pool;
    }
}

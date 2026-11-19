using System.Collections;
using UnityEngine;

public class SideTracingAttack : AttackBase
{
    [SerializeField] private Transform _bulletSpawnPoint;
    [SerializeField] private float _timePerSide = 0.5f;
    [SerializeField] private float _shootDelay = 0.2f;
    [SerializeField] private RegularProjectile _projectilePrefab;
    private ArenaSide _currentSide;
    public override void StartAttack()
    {
        StartCoroutine(MoveCoroutine());
        StartCoroutine(ShootCoroutine());
    }

    private void FullMovementFinished()
    {
        StopAllCoroutines();
        OnAttackFinished.Invoke();
    }

    private IEnumerator MoveCoroutine()
    {
        float totalTime = _timePerSide * 4.0f;
        float counter = 0.0f;
        while(counter < totalTime)
        {
            counter += Time.deltaTime;
            _bulletSpawnPoint.position = ArenaBounds.GetPointOnPerimeter(counter / totalTime, out _currentSide);
            yield return null;
        }
        FullMovementFinished();
    }

    private IEnumerator ShootCoroutine()
    {
        while(true)
        {
            ProjectileSpawner.Instance.Spawn<RegularProjectile>(_projectilePrefab, _bulletSpawnPoint.position, ArenaBounds.GetInwardDirection(_currentSide));
            yield return new WaitForSeconds(_shootDelay);
        }
    }
}

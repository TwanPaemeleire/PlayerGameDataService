using System.Collections;
using UnityEngine;

public class SideTracingAttack : AttackBase
{
    [SerializeField] private Transform _bulletSpawnPoint;
    [SerializeField] private float _timePerSide = 0.5f;
    [SerializeField] private float _shootDelay = 0.2f;
    [SerializeField] private float _minimumShootDelay = 0.08f;
    [SerializeField] private float _angleSpread = 30.0f;
    [SerializeField] private RegularProjectile _projectilePrefab;
    private ArenaSide _currentSide;
    private float _shootDifficultyDelay = 0.0f;
    public override void StartAttack()
    {
        _shootDifficultyDelay = _minimumShootDelay + (_shootDelay - _minimumShootDelay) / ScoreTracker.DifficultyMultiplier;
        StartCoroutine(MoveCoroutine());
        StartCoroutine(ShootCoroutine());
    }

    private void FullMovementFinished()
    {
        OnAttackFinished.Invoke();
        StopAllCoroutines();
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
            float offset = Random.Range(-_angleSpread / 2f, _angleSpread / 2f);
            Vector2 dir = Quaternion.Euler(0f, 0f, offset) * ArenaBounds.GetInwardDirection(_currentSide);
            ProjectileSpawner.Instance.Spawn(_projectilePrefab, _bulletSpawnPoint.position, dir, 1.0f);
            yield return new WaitForSeconds(_shootDifficultyDelay);
        }
    }
}

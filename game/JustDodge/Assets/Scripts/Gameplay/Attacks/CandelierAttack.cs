using System.Collections;
using UnityEngine;

public class CandelierAttack : AttackBase
{
    [SerializeField] private Transform _bulletSpawnPoint;
    [SerializeField] private float _shootDelay = 0.2f;
    [SerializeField] private float _minimumShootDelay = 0.08f;
    [SerializeField] private float _attackDuration = 3.0f;
    [SerializeField] private float _maxAttackDuration = 7.0f;
    [SerializeField] private float _spawnInwardsOffset = 0.1f;
    [SerializeField] private CandelierProjectile _projectilePrefab;
    private float _shootDifficultyDelay = 0.0f;
    private float _attackDifficultyDuration = 0.0f;
    public override void StartAttack()
    {
        _shootDifficultyDelay = _minimumShootDelay + (_shootDelay - _minimumShootDelay) / ScoreTracker.DifficultyMultiplier;
        _attackDifficultyDuration = _attackDuration + (_maxAttackDuration - _attackDuration) * (1.0f - 1.0f / ScoreTracker.DifficultyMultiplier);
        StartCoroutine(SpawnCoroutine());
    }

    private IEnumerator SpawnCoroutine()
    {
        float counter = 0.0f;
        while(counter < _attackDifficultyDuration)
        {
            counter += _shootDifficultyDelay;
            float randomT = Random.Range(0.0f, 1.0f);
            Vector2 position = ArenaBounds.GetPointOnPerimeter(randomT, out ArenaSide side);
            Vector2 direction = ArenaBounds.GetInwardDirection(side);
            position += direction * _spawnInwardsOffset;
            ProjectileSpawner.Instance.Spawn<CandelierProjectile>(_projectilePrefab, position, direction, 1.0f);
            yield return new WaitForSeconds(_shootDifficultyDelay);
        }
        OnAttackFinished.Invoke();
    }
}

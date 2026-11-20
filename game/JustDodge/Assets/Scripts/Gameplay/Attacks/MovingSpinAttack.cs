using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSpinAttack : AttackBase
{
    [SerializeField] private Transform _bulletSpawnPoint;
    [SerializeField] private RegularProjectile _projectilePrefab;
    [SerializeField] private int _amountOfShooters = 4;
    [SerializeField] private int _maxAmountOfShooters = 8;
    [SerializeField] private float _bulletShootDelay = 0.2f;
    [SerializeField] private float _minBulletShootDelay = 0.08f;
    [SerializeField] private float _rotationSpeed = 1.0f;
    [SerializeField] private float _maxRotationSpeed = 180.0f;
    [SerializeField] private float _rotationSpeedApplyTime = 2.0f;
    [SerializeField] private float _movementSpeed = 2.0f;
    [SerializeField] private float _movementSpeedApplyTime = 0.5f;
    [SerializeField] private float _borderSpawnOffset = 0.2f;
    private bool _rotatingClockWise = true;

    private float _shooterAngleInterval;
    List<float> _shootingAngles = new List<float>();

    private int _currentAmountOfShooters;
    private float _currentShootDelay;
    private float _currentRotationSpeed;

    public override void StartAttack()
    {
        int randomDirection = Random.Range(0, 2);
        _rotatingClockWise = randomDirection == 0;

        float difficulty = ScoreTracker.DifficultyMultiplier;
        _currentShootDelay = _minBulletShootDelay + (_bulletShootDelay - _minBulletShootDelay) / difficulty;
        _currentRotationSpeed = _rotationSpeed + (_maxRotationSpeed - _rotationSpeed) * (1.0f - 1.0f / difficulty);
        _currentAmountOfShooters = Mathf.Min(_maxAmountOfShooters, _amountOfShooters + Mathf.FloorToInt(difficulty - 1.0f));

        _shooterAngleInterval = 360.0f / (float)_currentAmountOfShooters;
        StartCoroutine(MoveBetween());
        for (int shooterIdx = 0; shooterIdx < _currentAmountOfShooters; ++shooterIdx)
        {
            float angle = shooterIdx * _shooterAngleInterval;
            _shootingAngles.Add(angle);
            StartCoroutine(ShootWhileRotating(angle, shooterIdx));
        }
    }

    private IEnumerator MoveBetween()
    {
        bool startLeft = Random.Range(0, 2) == 1;
        ArenaSide side = (startLeft) ? ArenaSide.Left : ArenaSide.Right;
        ArenaSide otherSide = (startLeft) ? ArenaSide.Right : ArenaSide.Left;
        Vector2 startPos = ArenaBounds.GetSideMidpoint(side);
        Vector2 endPos = ArenaBounds.GetSideMidpoint(otherSide);
        Vector2 dir = (startLeft) ? Vector2.right : Vector2.left;
        startPos.x += (startLeft) ? -_borderSpawnOffset : _borderSpawnOffset;

        _bulletSpawnPoint.position = startPos;
        float counter = 0.0f;
        while(true)
        {
            counter += Time.deltaTime;
            float smoothSpeedIncrease = Mathf.SmoothStep(0.0f, _movementSpeed, Mathf.Clamp01(counter / _movementSpeedApplyTime));
            Vector2 positionDelta = dir * smoothSpeedIncrease * Time.deltaTime;
            _bulletSpawnPoint.position += new Vector3(positionDelta.x, positionDelta.y, 0.0f);
            float difference = endPos.x - _bulletSpawnPoint.position.x;
            if ((startLeft && difference <= 0.0f) || (!startLeft && difference >= 0.0f)) break;
            yield return null;
        }

        _shootingAngles.Clear();
        OnAttackFinished.Invoke();
        StopAllCoroutines();
    }

    private IEnumerator ShootWhileRotating(float startAngle, int shooterIdx)
    {
        _shootingAngles[shooterIdx] = startAngle;

        StartCoroutine(ShootBullet(shooterIdx));

        float elapsedTime = 0.0f;
        while (true)
        {
            elapsedTime += Time.deltaTime;
            float smoothSpeedIncrease = Mathf.SmoothStep(0.0f, _currentRotationSpeed, elapsedTime / _rotationSpeedApplyTime);
            float angleToAdd = smoothSpeedIncrease * Time.deltaTime;

            if (!_rotatingClockWise) _shootingAngles[shooterIdx] -= angleToAdd;
            else _shootingAngles[shooterIdx] += angleToAdd;
            yield return null;
        }
    }

    private IEnumerator ShootBullet(int shooterIdx)
    {
        while (true)
        {
            Vector2 direction = new Vector2(Mathf.Cos(_shootingAngles[shooterIdx] * Mathf.Deg2Rad), Mathf.Sin(_shootingAngles[shooterIdx] * Mathf.Deg2Rad));
            ProjectileSpawner.Instance.Spawn(_projectilePrefab, _bulletSpawnPoint.position, direction, 1.0f);
            yield return new WaitForSeconds(_currentShootDelay);
        }
    }
}

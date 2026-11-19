using UnityEngine;

public class RegularProjectile : ProjectileBase
{
    [SerializeField] private float _speed = 50.0f;

    public override void Despawn()
    {
        ProjectileSpawner.Instance.Despawn(this);
    }

    public override void InitializeProjectile(float difficultyMultiplier)
    {
        _speed *= difficultyMultiplier;
    }

    private void Update()
    {
        transform.position += transform.right * _speed * Time.deltaTime;
    }
}

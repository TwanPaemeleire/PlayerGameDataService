using UnityEngine;

public class CandelierProjectile : ProjectileBase
{
    [SerializeField] private float _baseSpeed = 20.0f;

    private float _moveSpeed = 0.0f;

    private bool _isMoving = false;
    public override void Despawn()
    {
        _isMoving = false;
        ProjectileSpawner.Instance.Despawn(this);
    }

    private void Update()
    {
        if (!_isMoving) return;
        transform.position += transform.right * _moveSpeed * Time.deltaTime;
    }

    public void StartMoving() // Triggered by animation event
    {
        _isMoving = true;
        GetComponent<Collider2D>().enabled = true;
    }

    public override void InitializeProjectile(float difficultyMultiplier)
    {
        _isMoving = false;
        _moveSpeed = _baseSpeed;
        GetComponent<Collider2D>().enabled = false;
    }
}

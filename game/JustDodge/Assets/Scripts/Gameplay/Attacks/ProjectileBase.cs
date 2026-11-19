using UnityEngine;

public abstract class ProjectileBase : MonoBehaviour
{
    public abstract void InitializeProjectile(float difficultyMultiplier);
    public abstract void Despawn();
}

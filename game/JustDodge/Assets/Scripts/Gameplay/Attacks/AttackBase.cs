using UnityEngine;
using UnityEngine.Events;

public abstract class AttackBase : MonoBehaviour
{
    [SerializeField] private ArenaBounds _arenaBounds;
    public abstract void StartAttack();
    public UnityEvent OnAttackFinished = new UnityEvent();
    public ArenaBounds ArenaBounds => _arenaBounds;
}

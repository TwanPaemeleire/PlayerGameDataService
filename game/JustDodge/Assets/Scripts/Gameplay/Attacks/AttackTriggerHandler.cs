using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

public class AttackTriggerHandler : MonoBehaviour
{
    [SerializeField] private float _delayBetweenAttacks = 1.5f;
    [SerializeField] private List<AttackBase> _attacks = new List<AttackBase>();

    public UnityEvent OnCurrentAttackFinished = new UnityEvent();

    public void StartAttacks()
    {
        foreach(var attack in _attacks)
        {
            attack.OnAttackFinished.AddListener(OnCurrentAttackFinishedInternal);
        }
        PickNextAttack();
    }

    private void PickNextAttack()
    {
        StartCoroutine(AttackAfterDelayCoroutine());
    }

    private IEnumerator AttackAfterDelayCoroutine()
    {
        yield return new WaitForSeconds(_delayBetweenAttacks);
        int randIndex = Random.Range(0, _attacks.Count);
        _attacks[randIndex].StartAttack();
    }

    private void OnCurrentAttackFinishedInternal()
    {
        PickNextAttack();
    }
}

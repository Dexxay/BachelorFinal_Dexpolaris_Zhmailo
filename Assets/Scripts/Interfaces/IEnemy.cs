using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemy : IDamagable
{
    public IEnumerator AttackCoroutine();
    public void HitPlayer();
    public void Die();

    public event Action<IEnemy> EnemyDied;
}

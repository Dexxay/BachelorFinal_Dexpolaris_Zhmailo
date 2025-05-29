using System;
using System.Collections;

public interface IEnemy : IDamagable
{
    public IEnumerator AttackCoroutine();
    public void HitPlayer();
    public void Die();

    public event Action<IEnemy> EnemyDied;
}

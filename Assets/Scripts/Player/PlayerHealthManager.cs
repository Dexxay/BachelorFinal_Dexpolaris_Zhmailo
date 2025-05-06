using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthManager : MonoBehaviour
{
    [SerializeField] private int maxHealth;
    [SerializeField] private int startHealth;


    private int health;

    public event Action<int> HealthChanged;

    public event Action PlayerDied;
    public event Action PlayerDamaged;
    public event Action PlayerHealed;


    public int Health => health;

    void Start()
    {
        health += startHealth;
        HealthChanged?.Invoke(health);
    }

    public void AddHealth(int amount)
    {
        if (amount < 0)
            amount = 0;

        health += amount;
        PlayerHealed?.Invoke();
        if (health > maxHealth)
            health = maxHealth;
        HealthChanged?.Invoke(health);
    }

    public void ReduceHealth(int damage)
    {
        if (health <= 0 || damage <= 0)
            return;

        health -= damage;
        if (health <= 0)
        {
            health = 0;
            PlayerDied?.Invoke();
            return;
        }
        PlayerDamaged?.Invoke();
        HealthChanged?.Invoke(health);
    }


}

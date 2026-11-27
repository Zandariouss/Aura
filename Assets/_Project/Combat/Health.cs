using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    public event Action<float, float> HealthChanged;
    public event Action Died;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (currentHealth <= 0f) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0f);
        HealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            Died?.Invoke();
        }
    }

    [ContextMenu("Test: Take 30 Damage")]
    private void TestDamage()
    {
        TakeDamage(30f);
        Debug.Log("Health is now " + currentHealth);
    }
}
using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public const int MaxHearts = 3;

    [Header("Health & Shield Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float maxShield = 50f;

    [Header("Default Damage Values")]
    [SerializeField] private float defaultNormalDamage = 25f;
    [SerializeField] private float defaultFireDamage = 10f;

    [Header("Revive Hearts")]
    [SerializeField] private int startingHearts = 0;

    [field: SerializeField] public float CurrentHealth { get; private set; }
    [field: SerializeField] public float CurrentShield { get; private set; }
    [field: SerializeField] public int CurrentHearts { get; private set; }

    public bool IsDead => CurrentHealth <= 0f;

    public event Action OnHurt;
    public event Action OnBurned;
    public event Action OnDied;
    public event Action OnRevived;

    private void OnValidate()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        maxShield = Mathf.Max(0f, maxShield);
        defaultNormalDamage = Mathf.Max(0f, defaultNormalDamage);
        defaultFireDamage = Mathf.Max(0f, defaultFireDamage);
        startingHearts = Mathf.Clamp(startingHearts, 0, MaxHearts);
    }

    private void Awake()
    {
        CurrentHealth = maxHealth;
        CurrentShield = 0f;
        CurrentHearts = startingHearts;
    }

    public void TakeDamage(float amount = -1f)
    {
        float damage = amount < 0f ? defaultNormalDamage : amount;
        if (ApplyDamageMath(damage))
        {
            OnHurt?.Invoke();
        }
    }

    public void TakeFireDamage(float amount = -1f)
    {
        float damage = amount < 0f ? defaultFireDamage : amount;
        if (ApplyDamageMath(damage))
        {
            OnBurned?.Invoke();
        }
    }

    private bool ApplyDamageMath(float amount)
    {
        if (IsDead || amount <= 0f)
            return false;

        if (CurrentShield > 0f)
        {
            if (CurrentShield >= amount)
            {
                CurrentShield -= amount;
                return false; 
            }
            else
            {
                amount -= CurrentShield;
                CurrentShield = 0f;
            }
        }

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);

        if (CurrentHealth <= 0f)
        {
            Die();
            return false;
        }

        return true;
    }

    public void RestoreHealth(float amount)
    {
        if (amount <= 0f || IsDead) return;
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
    }

    public void AddShield(float amount)
    {
        if (amount <= 0f || IsDead) return;
        CurrentShield = Mathf.Min(CurrentShield + amount, maxShield);
    }

    public void AddHeart(int amount = 1)
    {
        if (amount <= 0 || IsDead) return;
        CurrentHearts = Mathf.Min(CurrentHearts + amount, MaxHearts);
    }

    public bool TryRevive()
    {
        if (!IsDead || CurrentHearts <= 0)
            return false;

        CurrentHearts--;
        CurrentHealth = maxHealth;
        CurrentShield = 0f;
        OnRevived?.Invoke();
        return true;
    }

    public void Die()
    {
        CurrentHealth = 0f;
        CurrentShield = 0f;
        OnDied?.Invoke();
    }
}
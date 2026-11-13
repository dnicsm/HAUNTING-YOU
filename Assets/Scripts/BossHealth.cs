
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    public int maxHealth = 100;

    private int currentHealth;

    public BossArenaLockIn arenaController;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Boss Defeated!");

        arenaController.CompleteEncounter();

        Destroy(gameObject);
    }
}

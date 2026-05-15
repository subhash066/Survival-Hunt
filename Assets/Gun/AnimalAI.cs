using UnityEngine;

public class AnimalAI : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        Debug.Log(gameObject.name + " spawned with HP: " + currentHealth);
    }

    // Called when bullet hits
    public void TakeDamage(int damage)
    {
        Debug.Log(gameObject.name + " took damage: " + damage);

        currentHealth -= damage;

        Debug.Log(gameObject.name + " current HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log(gameObject.name + " is DEAD");
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " destroyed");
        Destroy(gameObject);
    }
}
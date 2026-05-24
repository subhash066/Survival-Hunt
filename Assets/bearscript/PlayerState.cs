using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerState : MonoBehaviour
{
    public static PlayerState Instance { get; set; }

    // ---- Player Health ---- //
    public float currentHealth;
    public float maxHealth;

    [Header("Debug")]
    public bool logHealthToConsole = true;
    public float healthLogInterval = 1f;
    private float healthLogTimer = 0f;

    // ---- Unity Lifecycle ---- //
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        // Initialize stats
        currentHealth = maxHealth;
    }

    private void Update()
    {
        LogHealth();
    }

    void LogHealth()
    {
        if (!logHealthToConsole) return;

        healthLogTimer += Time.deltaTime;
        if (healthLogTimer < healthLogInterval) return;

        healthLogTimer = 0f;
        Debug.Log($"Player Health: {currentHealth}/{maxHealth}");
    }

    // ---- Damage Handling ---- //
    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player Died!");
        SceneManager.LoadScene("death");
    }
}

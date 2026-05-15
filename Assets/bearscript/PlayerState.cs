using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerState : MonoBehaviour
{
    public static PlayerState Instance { get; set; }

    // ---- Player Health ---- //
    public float currentHealth;
    public float maxHealth;

    // ---- Player Calories ---- //
    public float currentCalories;
    public float maxCalories;

    [Header("Debug")]
    public bool logHealthToConsole = true;
    public float healthLogInterval = 1f;
    private float healthLogTimer = 0f;

    float distanceTravelled = 0;
    Vector3 lastPosition;

    public GameObject playerBody;

    // ---- Player Hydration ---- //
    public float currentHydrationPercent;
    public float maxHydrationPercent;
    public bool isHydrationActive;

    // ---- Player Oxygen ---- //
    public float currentOxygenPercent;
    public float maxOxygenPercent = 100;
    public float oxygenDecreasedPerSecond = 1f;
    private float oxygenTimer = 0f;
    private float decreaseInterval = 1f;
    public float outOfAirDamagePerSecond = 5f;

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
        currentCalories = maxCalories;
        currentHydrationPercent = maxHydrationPercent;
        currentOxygenPercent = maxOxygenPercent;

        // Start hydration decrease coroutine
        StartCoroutine(DecreaseHydration());
    }

    // ---- Hydration System ---- //
    IEnumerator DecreaseHydration()
    {
        while (true)
        {
            currentHydrationPercent -= 1;
            yield return new WaitForSeconds(10); // decrease every 10 seconds
        }
    }

    // ---- Oxygen System ---- //
    private void Update()
    {
        oxygenTimer += Time.deltaTime;
        if (oxygenTimer >= decreaseInterval)
        {
            oxygenTimer = 0f;
            currentOxygenPercent -= oxygenDecreasedPerSecond;

            if (currentOxygenPercent <= 0)
            {
                TakeDamage(outOfAirDamagePerSecond * decreaseInterval);
            }
        }

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

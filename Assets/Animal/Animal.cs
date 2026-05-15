using UnityEngine;

public class Animal : MonoBehaviour
{
    [Header("Animal Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("AI & Detection")]
    public float detectionRadius = 15f;
    public float attackRadius = 5f;
    public float moveSpeed = 3f;
    public float rotationSpeed = 5f;

    private Transform player;
    private Animator animator;
    private CharacterController controller;

    [Header("Sound Settings")]
    public AudioClip attackSound;
    public AudioClip deathSound;
    public AudioClip hitSound;

    private AudioSource audioSource;

    [Header("Combat Settings")]
    public float attackInterval = 1.5f;
    public float damagePerAttack = 10f;

    private float nextAttackTime = 0f;
    private bool isAggravated = false;

    void Start()
    {
        currentHealth = maxHealth;

        animator = GetComponent<Animator>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        controller = GetComponent<CharacterController>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // Audio Source Setup
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 3D Sound Settings
        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 3f;
        audioSource.maxDistance = 20f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        // Rigidbody Safety
        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = true;
        }
    }

    void Update()
    {
        if (currentHealth <= 0) return;

        if (player != null)
        {
            HandleAI();
        }
        else
        {
            UpdateAnimations(false, false);
        }
    }

    void HandleAI()
    {
        float distanceToPlayer =
            Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRadius)
        {
            AttackPlayer();
        }
        else if (distanceToPlayer <= detectionRadius || isAggravated)
        {
            FollowPlayer();
        }
        else
        {
            Idle();
        }
    }

    void FollowPlayer()
    {
        Vector3 direction =
            (player.position - transform.position).normalized;

        direction.y = 0;

        if (controller != null)
        {
            Vector3 move =
                direction * moveSpeed * Time.deltaTime;

            move.y = -9.81f * Time.deltaTime;

            controller.Move(move);
        }
        else
        {
            Vector3 targetPosition =
                new Vector3(
                    player.position.x,
                    transform.position.y,
                    player.position.z
                );

            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    moveSpeed * Time.deltaTime
                );
        }

        RotateTowards(player.position);

        AlignToTerrain();

        UpdateAnimations(true, false);
    }

    void AttackPlayer()
    {
        Vector3 targetPosition =
            new Vector3(
                player.position.x,
                transform.position.y,
                player.position.z
            );

        RotateTowards(targetPosition);

        UpdateAnimations(false, true);

        AlignToTerrain();

        if (Time.time >= nextAttackTime)
        {
            // Play attack sound safely
            PlaySound(attackSound);

            // Trigger attack animation
            if (animator != null && HasParameter("AttackTrigger"))
            {
                animator.SetTrigger("AttackTrigger");
            }

            if (PlayerState.Instance != null)
            {
                PlayerState.Instance.TakeDamage(damagePerAttack);
                Debug.Log($"{gameObject.name} attacked player for {damagePerAttack} damage.");
            }

            nextAttackTime = Time.time + attackInterval;
        }
    }

    void Idle()
    {
        UpdateAnimations(false, false);

        AlignToTerrain();
    }

    void RotateTowards(Vector3 target)
    {
        Vector3 direction = target - transform.position;

        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation =
                Quaternion.LookRotation(direction);

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    lookRotation,
                    Time.deltaTime * rotationSpeed
                );
        }
    }

    void AlignToTerrain()
    {
        RaycastHit hit;

        if (Physics.Raycast(
            transform.position + Vector3.up * 1.5f,
            Vector3.down,
            out hit,
            3f))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return;
            }

            if (controller == null)
            {
                transform.position =
                    new Vector3(
                        transform.position.x,
                        hit.point.y,
                        transform.position.z
                    );
            }

            Quaternion slopeRotation =
                Quaternion.FromToRotation(
                    transform.up,
                    hit.normal
                ) * transform.rotation;

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    slopeRotation,
                    Time.deltaTime * rotationSpeed
                );
        }
    }

    void UpdateAnimations(bool isWalking, bool isAttacking)
    {
        if (animator == null) return;

        SetAnimBool("isWalking", isWalking);

        SetAnimBool("Attack", isAttacking);
    }

    void SetAnimBool(string paramName, bool value)
    {
        if (HasParameter(paramName))
        {
            animator.SetBool(paramName, value);
        }
    }

    bool HasParameter(string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
            {
                return true;
            }
        }

        return false;
    }

    // SAFE SOUND PLAYER
    void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        // Prevent same sound overlap
        if (audioSource.clip == clip && audioSource.isPlaying)
        {
            return;
        }

        audioSource.clip = clip;
        audioSource.Play();
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        isAggravated = true;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null && HasParameter("Hit"))
            {
                animator.SetTrigger("Hit");
            }

            PlaySound(hitSound);
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " has died!");

        UpdateAnimations(false, false);

        if (animator != null)
        {
            if (HasParameter("Death"))
            {
                animator.SetTrigger("Death");
            }
            else if (HasParameter("Die"))
            {
                animator.SetTrigger("Die");
            }
        }

        PlaySound(deathSound);

        if (controller != null)
        {
            controller.enabled = false;
        }

        Destroy(gameObject, 5f);
    }

    void OnDrawGizmosSelected()
    {
        // Detection Radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Attack Radius
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
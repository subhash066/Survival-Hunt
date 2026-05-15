using UnityEngine;

public class Deer : MonoBehaviour
{
    [Header("!!! Combat & Player !!!")]
    public Transform playerOverride;
    public int health = 200;
    public int maxHealth = 200;
    public float detectionRadius = 15f;

    [Header("!!! Stats !!!")]
    public float runSpeed = 7f;
    public float rotationSpeed = 10f;
    public float runAnimationSpeed = 2f;

    [Header("!!! Model Fixes !!!")]
    public Vector3 modelRotationOffset = Vector3.zero;
    public float footOffset = 0.05f;

    [Header("Sound Effects")]
    public AudioClip hitSound;
    public AudioClip deathSound;
    private AudioSource audioSource;

    [Header("Physics")]
    public LayerMask groundMask = 1;
    private Transform player;
    private Animator animator;
    private CharacterController controller;

    private bool isAggravated = false;
    private bool isDead = false;
    private bool isFleeing = false;
    private bool isHit = false;
    private float fleeDelayTimer = 0f;
    private float fleeDelay = 0.5f;
    private Vector3 moveDirection = Vector3.zero;
    private float verticalVelocity = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (animator != null) animator.applyRootMotion = false;

        // --- AUTO-FIX COLLIDER (Adjust to model size) ---
        if (controller != null)
        {
            AdjustCapsuleColliderToModel();
        }

        FindPlayer();

        if (animator != null)
        {
            string pList = "";
            foreach (var p in animator.parameters) pList += p.name + ", ";
            Debug.Log($"<color=cyan>Deer Animator Parameters: {pList}</color>");
        }
    }

    void AdjustCapsuleColliderToModel()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds totalBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            totalBounds.Encapsulate(renderers[i].bounds);
        }

        float modelHeight = totalBounds.size.y;
        float modelRadius = Mathf.Max(totalBounds.size.x, totalBounds.size.z) * 0.5f;

        controller.height = Mathf.Max(1.2f, modelHeight * 0.9f);
        controller.radius = Mathf.Clamp(modelRadius * 0.5f, 0.3f, controller.height * 0.45f);
        controller.center = new Vector3(0, controller.height * 0.5f, 0);

        Debug.Log($"<color=green>Deer Collider Auto-Fixed: height={controller.height:F2}, radius={controller.radius:F2}, center={controller.center}</color>");
    }

    // --- SELF-DEFENSE HIT DETECTION ---
    void OnTriggerEnter(Collider other)
    {
        if (isDead) return;
        if (other.name.ToLower().Contains("bullet") || other.CompareTag("Bullet"))
        {
            Debug.Log($"<color=red>DEER HIT BY BULLET TRIGGER: {other.name}</color>");
            TakeDamage(1);
            Destroy(other.gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;
        if (collision.gameObject.name.ToLower().Contains("bullet") || collision.gameObject.CompareTag("Bullet"))
        {
            Debug.Log($"<color=red>DEER HIT BY BULLET COLLISION: {collision.gameObject.name}</color>");
            TakeDamage(1);
            Destroy(collision.gameObject);
        }
    }

    void FindPlayer()
    {
        if (playerOverride != null) { player = playerOverride; return; }
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) playerObj = GameObject.Find("FirstPersonController");
        if (playerObj == null) playerObj = GameObject.Find("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (isDead) return;
        if (player == null) { FindPlayer(); return; }

        Debug.DrawLine(transform.position + Vector3.up, player.position, Color.red);

        float distance = Vector3.Distance(transform.position, player.position);
        moveDirection = Vector3.zero;

        // Handle flee delay countdown
        if (fleeDelayTimer > 0)
        {
            fleeDelayTimer -= Time.deltaTime;
            if (fleeDelayTimer <= 0)
            {
                // Delay expired, now allow running
                isFleeing = true;
            }
        }

        if (isHit)
        {
            // Show hit animation but don't move yet
            UpdateAnimations(false, false);
            ApplyMovement();
            return;
        }

        if (distance <= detectionRadius && fleeDelayTimer <= 0)
        {
            // Player detected and delay expired, start fleeing
            if (!isFleeing)
            {
                fleeDelayTimer = fleeDelay;
            }
            isFleeing = true;
            isAggravated = true;
        }
        else if (distance > detectionRadius * 1.25f && !isDead && fleeDelayTimer <= 0)
        {
            isFleeing = false;
            if (!isAggravated)
                UpdateAnimations(false, false);
        }

        if (isFleeing || isAggravated)
        {
            Flee();
        }
        else
        {
            UpdateAnimations(false, false);
        }

        ApplyMovement();
    }

    void Flee()
    {
        Vector3 direction = (transform.position - player.position);
        direction.y = 0;

        if (direction.magnitude > 0.1f)
        {
            moveDirection = direction.normalized * runSpeed;
            Quaternion lookRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(modelRotationOffset);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
            UpdateAnimations(false, true);
        }
    }

    void ApplyMovement()
    {
        if (controller != null && controller.enabled)
        {
            if (controller.isGrounded) verticalVelocity = -2f;
            else verticalVelocity += Physics.gravity.y * Time.deltaTime;

            Vector3 velocity = moveDirection;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }
        else if (!isDead)
        {
            transform.position += moveDirection * Time.deltaTime;
        }
        AlignToTerrain();
    }

    void AlignToTerrain()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down, out hit, 5f, groundMask))
        {
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, hit.normal);
            if (forward.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(forward, hit.normal) * Quaternion.Euler(modelRotationOffset);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
            }
        }
    }

    void LateUpdate()
    {
        if (isDead) return;
        HardSnapToGround();
    }

    void HardSnapToGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 5f, Vector3.down, out hit, 10f, groundMask))
            transform.position = new Vector3(transform.position.x, hit.point.y + footOffset, transform.position.z);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        health -= damage;
        isAggravated = true;
        isHit = true;
        fleeDelayTimer = fleeDelay; // Start 1 second delay before running

        Debug.Log($"<color=orange>Deer Damage Taken! Health: {health}/{maxHealth}</color>");

        if (health <= 0)
        {
            Die();
        }
        else
        {
            if (animator != null)
            {
                if (HasParameter("GetHit")) animator.SetTrigger("GetHit");
                else if (HasParameter("Hit")) animator.SetTrigger("Hit");
                else if (HasParameter("hit")) animator.SetTrigger("hit");
            }
            if (hitSound != null) audioSource.PlayOneShot(hitSound);
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        if (controller != null) controller.enabled = false;
        UpdateAnimations(false, false);
        if (animator != null)
        {
            if (HasParameter("Death")) animator.SetTrigger("Death");
            else if (HasParameter("Die")) animator.SetTrigger("Die");
        }
        if (deathSound != null) audioSource.PlayOneShot(deathSound);
        Destroy(gameObject, 2.3f);
    }

    void UpdateAnimations(bool isWalking, bool isRunning)
    {
        if (animator == null) return;
        SetAnimBool("isWalking", isWalking || isRunning);
        SetAnimBool("isRunning", isRunning);
        SetAnimBool("isScared", isRunning);
    }

    void SetAnimBool(string param, bool val)
    {
        if (HasParameter(param)) animator.SetBool(param, val);
    }

    bool HasParameter(string paramName)
    {
        if (animator == null) return false;
        foreach (AnimatorControllerParameter param in animator.parameters)
            if (param.name.Equals(paramName, System.StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }
}

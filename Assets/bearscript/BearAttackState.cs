using UnityEngine;
using UnityEngine.AI;

public class BearAttackState : StateMachineBehaviour
{
    NavMeshAgent agent;
    Transform player;

    public float stopAttackingDistance = 4f;   // distance at which bear stops attacking
    public float attackRate = 1f;              // attacks per second
    public int damageToInflict = 10;           // damage per attack

    private float attackTimer;

    // Called when entering Attack state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent = animator.GetComponent<NavMeshAgent>();

        attackTimer = 0f;
    }

    // Called every frame while in Attack state
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null || agent == null) return;

        // Face the player
        LookAtPlayer();

        // Attack logic
        if (attackTimer <= 0f)
        {
            Attack();
            attackTimer = 1f / attackRate;
        }
        else
        {
            attackTimer -= Time.deltaTime;
        }

        // Stop attacking if player moves away
        float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);
        if (distanceFromPlayer > stopAttackingDistance)
        {
            animator.SetBool("isAttacking", false);
        }
    }

    // Rotate to face the player
    private void LookAtPlayer()
    {
        Vector3 direction = player.position - agent.transform.position;
        agent.transform.rotation = Quaternion.LookRotation(direction);

        var yRotation = agent.transform.eulerAngles.y;
        agent.transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    // Inflict damage on the player
    private void Attack()
    {
        PlayerState.Instance.TakeDamage(damageToInflict);
    }
}

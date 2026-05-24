using UnityEngine;

public class BearIdleState : StateMachineBehaviour
{
    float timer;
    public float idleTime = 3f; // How long the bear stays idle
    public float detectionAreaRadius = 18f;

    Transform player;

    // Called when entering Idle state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer = 0f;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Called every frame while in Idle state
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Transition to Walk after idleTime
        timer += Time.deltaTime;
        if (timer > idleTime)
        {
            animator.SetBool("isWalking", true);
        }

        // Transition to Chase if player is close
        if (player != null)
        {
            float distanceFromPlayer = Vector3.Distance(player.position, animator.transform.position);
            if (distanceFromPlayer < detectionAreaRadius)
            {
                animator.SetBool("isChasing", true);
            }
        }
    }
}

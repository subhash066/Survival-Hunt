using System.Collections.Generic;
using UnityEngine;

public class BearWalkState : StateMachineBehaviour
{
    float timer;
    public float walkingTime = 10f; // how long the bear walks before idling

    Transform player;
    public float detectionAreaRadius = 18f;
    public float walkSpeed = 2f;

    List<Transform> waypointsList = new List<Transform>();
    int currentWaypointIndex = 0;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        timer = 0f;

        // Collect waypoints from NPCWaypoints cluster
        NPCWaypoints npcWaypoints = animator.GetComponent<NPCWaypoints>();
        if (npcWaypoints != null && npcWaypoints.npcWaypointsCluster != null)
        {
            waypointsList.Clear();
            foreach (Transform t in npcWaypoints.npcWaypointsCluster.transform)
            {
                waypointsList.Add(t);
            }
        }

        // Pick a random starting waypoint
        if (waypointsList.Count > 0)
        {
            currentWaypointIndex = Random.Range(0, waypointsList.Count);
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (waypointsList.Count > 0)
        {
            Transform targetWaypoint = waypointsList[currentWaypointIndex];
            animator.transform.position = Vector3.MoveTowards(
                animator.transform.position,
                targetWaypoint.position,
                walkSpeed * Time.deltaTime
            );

            // Rotate to face waypoint
            Vector3 direction = targetWaypoint.position - animator.transform.position;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                animator.transform.rotation = Quaternion.Slerp(animator.transform.rotation, lookRotation, 0.1f);
            }

            // Switch waypoint when close
            if (Vector3.Distance(animator.transform.position, targetWaypoint.position) < 0.2f)
            {
                currentWaypointIndex = Random.Range(0, waypointsList.Count);
            }
        }

        // Transition to Idle after walkingTime
        timer += Time.deltaTime;
        if (timer > walkingTime)
        {
            animator.SetBool("isWalking", false);
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

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Nothing to reset (no NavMeshAgent used)
    }
}

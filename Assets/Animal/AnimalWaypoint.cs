using UnityEngine;
using System.Collections.Generic;

public class AnimalWaypoint : MonoBehaviour
{
    public List<Transform> waypoints;
    public float moveSpeed = 2f;
    public float waypointStopDistance = 0.5f;

    private int currentIndex = 0;

    void Start()
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            enabled = false;
            return;
        }
        transform.position = waypoints[0].position;
    }

    void Update()
    {
        if (waypoints == null || waypoints.Count == 0) return;

        Transform target = waypoints[currentIndex];

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);

        transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < waypointStopDistance)
        {
            currentIndex = (currentIndex + 1) % waypoints.Count;
        }
    }
}
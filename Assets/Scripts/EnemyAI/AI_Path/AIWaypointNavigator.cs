using AI.Steering;
using Model.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIWaypointNavigator : MonoBehaviour
{
	public AI_Waypoints path;
	public float stoppingDistance = 1.5f;
	public float waypointRadius = 30f;
	private int currentWaypointIndex = 0;
	private AIShip aiShip;
	private SteeringAgent steeringAgent;


	private void Start()
	{
		aiShip = GetComponent<AIShip>();
		steeringAgent = GetComponent<SteeringAgent>();

		if (path == null)
		{
			// Try to find the path in scene if not assigned
			path = FindObjectOfType<AI_Waypoints>();
			if (path == null)
			{
				//Debug.LogError($"{name}: No path assigned and none found in scene!");
				aiShip.UpdatePatrolState(false); // Fallback to roaming
			}
		}
	}

	private void Update()
	{
		if (path == null) return;

		// Update target to current waypoint
		Vector3 targetPos = path.GetWaypoint(currentWaypointIndex).position;
		AIShip aiShip = GetComponent<AIShip>();
		if (aiShip != null)
		{
			aiShip.steeringAgent.TargetPosition = targetPos;
		}

		// Check distance to waypoint
		float dist = Vector3.Distance(transform.position, targetPos);
		if (dist < waypointRadius)
		{
			currentWaypointIndex = (currentWaypointIndex + 1) % path.WaypointCount;
		}
	}

	public void ClearPath()
	{
		path = null;
		currentWaypointIndex = 0;
	}
}

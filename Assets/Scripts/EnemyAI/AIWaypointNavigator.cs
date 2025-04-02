using AI.Steering;
using Model.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIWaypointNavigator : MonoBehaviour
{
	public AI_Waypoints path;
	public float stoppingDistance = 1.5f;

	private int currentWaypointIndex = 0;
	private AIShip aiShip;
	private SteeringAgent steeringAgent;


	private void Start()
	{
		aiShip = GetComponent<AIShip>();
		steeringAgent = GetComponent<SteeringAgent>();

		if (path == null)
			Debug.LogError("Path not assigned to AIWaypointNavigator.");
	}

	private void Update()
	{
		if (path == null || path.WaypointCount == 0) return;

		Transform waypoint = path.GetWaypoint(currentWaypointIndex);
		if (waypoint == null) return;

		Vector3 dir = (waypoint.position - transform.position);
		dir.y = 0;

		if (dir.magnitude < stoppingDistance)
		{
			currentWaypointIndex = (currentWaypointIndex + 1) % path.WaypointCount;
			waypoint = path.GetWaypoint(currentWaypointIndex);
		}

		steeringAgent.TargetPosition = waypoint.position;
	}
}

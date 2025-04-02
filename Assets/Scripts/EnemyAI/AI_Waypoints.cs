using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Waypoints : MonoBehaviour
{
	public Transform[] waypoints;

	private void Awake()
	{
		if (waypoints.Length == 0)
		{
			waypoints = new Transform[transform.childCount];
			for (int i = 0; i < waypoints.Length; i++)
			{
				waypoints[i] = transform.GetChild(i);
			}
		}
	}

	public Transform GetWaypoint(int index)
	{
		if (index < 0 || index >= waypoints.Length)
			return null;

		return waypoints[index];
	}

	public int WaypointCount => waypoints.Length;

	private void OnDrawGizmos()
	{
		if (waypoints == null || waypoints.Length == 0)
			return;

		// Draw waypoints as spheres
		Gizmos.color = Color.cyan;
		foreach (Transform waypoint in waypoints)
		{
			if (waypoint != null)
				Gizmos.DrawSphere(waypoint.position, 0.5f);
		}

		// Draw connections between waypoints
		
			Gizmos.color = Color.yellow;
			for (int i = 0; i < waypoints.Length; i++)
			{
				if (waypoints[i] == null || waypoints[(i + 1) % waypoints.Length] == null)
					continue;

				Gizmos.DrawLine(
					waypoints[i].position,
					waypoints[(i + 1) % waypoints.Length].position
				);
			}
		
	}
}

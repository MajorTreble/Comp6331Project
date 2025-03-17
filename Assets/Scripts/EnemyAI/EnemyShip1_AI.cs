using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShip1_AI : MonoBehaviour
{
	public Transform player;
	public float speed = 5f;
	public float avoidanceDistance = 10f; // Distance to detect obstacles
	public float avoidanceForce = 10f; // Force to steer away from obstacles
	private Rigidbody rb;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		if (player == null)
		{
			player = GameObject.FindGameObjectWithTag("Player").transform;
		}
	}

	void FixedUpdate()
	{
		if (player == null) return;

		Vector3 direction = (player.position - transform.position).normalized;
		float distanceToPlayer = Vector3.Distance(transform.position, player.position);

		// Flee if too close to the player
		if (distanceToPlayer < 10f) // Adjust the distance as needed
		{
			direction = -direction; // Move away from the player
		}

		AvoidObstacles(ref direction);
		rb.velocity = direction * speed;
	}

	void AvoidObstacles(ref Vector3 direction)
	{
		RaycastHit hit;

		// Check for obstacles in front of the enemy
		if (Physics.Raycast(transform.position, transform.forward, out hit, avoidanceDistance))
		{
			// Steer away from the obstacle
			Vector3 avoidanceDirection = Vector3.Reflect(transform.forward, hit.normal);
			direction = (direction + avoidanceDirection * avoidanceForce).normalized;
		}
	}
}

using UnityEngine;

public class Faction1AI : BaseScriptForEnemyAI
{
	public float flankDistance = 10f;
	//public float speed = 5f; // Add speed variable
	//private Transform player; // Add player variable
	//private Rigidbody rb; // Add Rigidbody variable

	void Start()
	{
		// Find the player by tag
		player = GameObject.FindGameObjectWithTag("Player").transform;

		// Get the Rigidbody component
		rb = GetComponent<Rigidbody>();

		// Check if player and Rigidbody are found
		if (player == null)
		{
			Debug.LogError("Player not found! Make sure the player has the 'Player' tag.");
		}
		if (rb == null)
		{
			Debug.LogError("Rigidbody not found! Make sure the enemy has a Rigidbody component.");
		}
	}

	void FixedUpdate()
	{
		if (player == null || rb == null) return;

		if (IsPlayerInRange())
		{
			// Player is in range, flank the player
			Vector3 direction = (player.position - transform.position).normalized;
			AvoidObstacles(ref direction); // Avoid obstacles

			Vector3 flankDirection = Vector3.Cross(direction, Vector3.up).normalized;

			rb.velocity = (direction + flankDirection) * speed;
		}
		else
		{
			// Player is not in range, roam randomly
			Roam();
		}
	}
}
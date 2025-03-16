using UnityEngine;

public class PirateAI : BaseScriptForEnemyAI
{
	public float attackRange = 15f;
	public float attackCooldown = 2f;
	//public float speed = 5f; // Add speed variable
	//private Transform player; // Add player variable
	//private Rigidbody rb; // Add Rigidbody variable
	private float lastAttackTime;
	public GameObject laserPrefab; 
	public Transform firePoint;

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
			// Player is in range, chase and attack
			Vector3 direction = (player.position - transform.position).normalized;
			AvoidObstacles(ref direction); // Avoid obstacles

			float distanceToPlayer = Vector3.Distance(transform.position, player.position);

			rb.velocity = direction * speed;

			if (distanceToPlayer < attackRange && Time.time > lastAttackTime + attackCooldown)
			{
				Attack();
				lastAttackTime = Time.time;
			}
		}
		else
		{
			// Player is not in range, roam randomly
			Roam();
		}
	}

	void Attack()
	{
		if (laserPrefab == null || firePoint == null) return;

		// Instantiate the laser
		GameObject laser = Instantiate(laserPrefab, firePoint.position, firePoint.rotation);
		laser.GetComponent<Rigidbody>().velocity = firePoint.forward * 20f; // Adjust speed as needed
	}
}
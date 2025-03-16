using UnityEngine;

public class PirateAI : BaseScriptForEnemyAI
{
	private enum AIState { Roaming, Seeking } // AI states
	private AIState currentState = AIState.Roaming; // Default to roaming

	float attackRange = 10f;
	public float attackCooldown = 2f;
	private float lastAttackTime;
	public GameObject laserPrefab;
	public Transform firePoint;
	public float rotationSpeed = 5f; // Speed at which the ship rotates toward the target

	void Start()
	{
		// Initialize state as Roaming
		currentState = AIState.Roaming;

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

		// Set an initial roam target
		SetNewRoamTarget();
	}

	public float acceleration = 5f; // Rate at which the ship accelerates
	private float currentSpeed = 0f; // Current speed of the ship

	void FixedUpdate()
	{
		if (player == null || rb == null) return;

		if (IsPlayerInRange())
		{
			currentState = AIState.Seeking; // Switch to seeking state
		}
		else
		{
			currentState = AIState.Roaming; // Switch to roaming state
		}

		if (currentState == AIState.Seeking)
		{
			SeekPlayer();
		}
		else if (currentState == AIState.Roaming)
		{
			Roam(); // Move randomly
		}
	}

	void SeekPlayer()
	{
		Vector3 direction = (player.position - transform.position).normalized;
		AvoidCollisions(ref direction);

		// Rotate toward the player
		RotateTowardTarget(direction);

		// Move toward the player but slow down when getting close
		float distanceToPlayer = Vector3.Distance(transform.position, player.position);

		if (distanceToPlayer > attackRange)
		{
			rb.velocity = transform.forward * speed * Mathf.Clamp01(distanceToPlayer / attackRange);
		}
		else
		{
			rb.velocity = Vector3.zero; // Stop moving when close to attack range
		}

		// Attack if in range
		if (distanceToPlayer < attackRange && Time.time > lastAttackTime + attackCooldown)
		{
			Attack();
			lastAttackTime = Time.time;
		}
	}

	protected override void Roam()
	{
		if (rb == null) return;

		// Move toward the roam target
		Vector3 direction = (roamTarget - transform.position).normalized;
		rb.velocity = direction * roamSpeed;

		// Rotate toward roam target
		RotateTowardTarget(direction);

		// If close to the target, set a new one
		if (Vector3.Distance(transform.position, roamTarget) < 5f)
		{
			SetNewRoamTarget();
		}
	}


	void Attack()
	{
		if (laserPrefab == null || firePoint == null)
		{
			Debug.LogError("Laser prefab or fire point is not assigned!");
			return;
		}

		// Log fire point position
		Debug.Log("Fire point position: " + firePoint.position);

		// Instantiate the laser
		GameObject laser = Instantiate(laserPrefab, firePoint.position, firePoint.rotation);
		if (laser == null)
		{
			Debug.LogError("Failed to instantiate laser!");
			return;
		}

		// Set laser velocity
		Rigidbody laserRb = laser.GetComponent<Rigidbody>();
		if (laserRb == null)
		{
			Debug.LogError("Laser prefab is missing a Rigidbody component!");
			return;
		}
		laserRb.velocity = firePoint.forward * 20f;

		// Ignore collisions between the laser and the pirate
		Collider laserCollider = laser.GetComponent<Collider>();
		Collider pirateCollider = GetComponent<Collider>();
		if (laserCollider != null && pirateCollider != null)
		{
			Physics.IgnoreCollision(laserCollider, pirateCollider);
		}

		Debug.Log("Laser fired! Next attack in " + attackCooldown + " seconds.");
	}
}
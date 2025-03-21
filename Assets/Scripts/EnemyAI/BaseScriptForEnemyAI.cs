using UnityEngine;

using Model;

public class BaseScriptForEnemyAI : MonoBehaviour
{
	public ReputationData playerReputation; // Player's reputation
	public Transform player;
	public Rigidbody rb;
	protected Vector3 roamTarget;

	public AIBehavior behavior;

	public float detectionRadius = 25f;
	public float roamRadius = 150f;
	//public float roamSpeed = 3f;
	public float avoidanceForce = 5f; // Force to steer away from obstacles and ships

	public float obstacleAvoidanceRange = 10f; // Range to detect obstacles
	public LayerMask obstacleLayer; // Layer for obstacles (e.g., asteroids)
	public float obstacleAvoidanceWeight = 5f; // Strength of avoidance

	//public float attackRange = 10f; // Attack range
	public float attackCooldown = 2f; // Cooldown between attacks
	public float lastAttackTime; // Time of the last attack
	public GameObject laserPrefab; // Laser prefab for attacks
	public GameObject firePoint; // Point from which lasers are fired

	protected enum AIState { Roaming, Seeking, Fleeing, AllyAssisting }
	protected AIState currentState = AIState.Roaming;

	[Header("Layer Masks")]
	public LayerMask shipLayer;

	public virtual void Start()
	{
		if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
		if (rb == null) rb = GetComponent<Rigidbody>();
		if (behavior == null)
		{
			Debug.Log("AIBehavior is not assigned to " + gameObject.name);
		}
		SetNewRoamTarget();
	}

	void FixedUpdate()
	{
		if (player == null || rb == null) return;

		// Determine state
		if (ShouldAllyWithPlayer())
		{
			currentState = AIState.AllyAssisting;
		}
		else if (IsPlayerInRange())
		{
			currentState = AIState.Seeking;
		}
		else
		{
			currentState = AIState.Roaming;
		}

		// Execute behavior
		switch (currentState)
		{
			case AIState.Roaming:
				Roam();
				break;
			case AIState.Seeking:
				SeekPlayer();
				break;
			case AIState.AllyAssisting:
				AllyAssist();
				break;
		}
	}



	public void SetBehavior(AIBehavior newBehavior)
	{
		behavior = newBehavior; // Set the behavior
	}

	public bool ShouldAttackPlayer()
	{
		if (behavior == null || playerReputation == null) return false;

		// Always prioritize ally status
		if (ShouldAllyWithPlayer()) return false;

		// Check job conditions
		Job currentJob = JobController.Inst.currJob;
		if (currentJob != null)
		{
			// Example: If defending Faction1 and this is a Faction1 ship, don't attack
			if (currentJob.jobType == JobType.Defend &&
				currentJob.jobTarget == JobTarget.Faction1 &&
				behavior.faction == AIBehavior.Faction.Faction1)
				return false;

			// Example: If hunting Pirates and this is a Pirate ship, attack
			if (currentJob.jobType == JobType.Hunt &&
				currentJob.jobTarget == JobTarget.Pirate &&
				behavior.faction == AIBehavior.Faction.Pirates)
				return true;

			// Example: If defending Faction1 and this is a Faction2 ship, attack
			if (currentJob.jobType == JobType.Defend &&
			currentJob.jobTarget == JobTarget.Faction1 &&
			behavior.faction == AIBehavior.Faction.Faction2)
			{
				return true; // Faction2 ships attack during "Defend Faction1" missions
			}
		}

		// Default: Attack if reputation is low
		return playerReputation.GetReputation(behavior.faction) <= behavior.reputationThreshold;
	}

	protected void RotateTowardTarget(Vector3 direction, float rotationSpeed = 5f)
	{
		if (direction != Vector3.zero)
		{
			Quaternion targetRotation = Quaternion.LookRotation(direction);
			transform.rotation = Quaternion.Slerp(
				transform.rotation,
				targetRotation,
				Time.deltaTime * rotationSpeed
			);
		}
	}

	protected void SetNewRoamTarget()
	{
		// Set a random target within the roam radius
		roamTarget = transform.position + Random.insideUnitSphere * roamRadius;
		roamTarget.y = 0; // Keep the target on the same horizontal plane
	}

	public bool IsPlayerInRange()
	{
		if (player == null) return false;
		float distanceToPlayer = Vector3.Distance(transform.position, player.position);
		return distanceToPlayer < detectionRadius;
	}

	protected void Roam()
	{
		if (rb == null || roamTarget == null) return;

		// Move toward the roam target
		Vector3 direction = (roamTarget - transform.position).normalized;
		Vector3 avoidance = ComputeObstacleAvoidance(direction);

		if (avoidance != Vector3.zero)
		{
			direction = (direction + avoidance).normalized;
		}

		rb.velocity = direction * behavior.roamSpeed;

		// Rotate toward roam target
		RotateTowardTarget(direction);

		// If close to the target, set a new one
		if (Vector3.Distance(transform.position, roamTarget) < 5f)
		{
			SetNewRoamTarget();
		}
	}

	protected bool ShouldAllyWithPlayer()
	{
		if (behavior == null || playerReputation == null) return false;
		return playerReputation.GetReputation(behavior.faction) >= behavior.allyReputationThreshold;
	}

	protected void AllyAssist()
	{
		if (player == null || behavior == null) return;

		// Move toward the player to assist
		Vector3 direction = (player.position - transform.position).normalized;
		Vector3 avoidance = ComputeObstacleAvoidance(direction);

		if (avoidance != Vector3.zero)
		{
			direction = (direction + avoidance).normalized;
		}

		RotateTowardTarget(direction, behavior.rotationSpeed);
		rb.velocity = direction * behavior.chaseSpeed;

		// Attack enemies near the player
		AttackEnemiesNearPlayer();
	}

	protected virtual void AttackEnemiesNearPlayer()
	{
		if (player == null || behavior == null) return;

		// Find enemies near the player
		Collider[] nearbyShips = Physics.OverlapSphere(
		player.position,
		behavior.allyAssistRange,
		shipLayer
		);


		foreach (Collider ship in nearbyShips)
		{
			BaseScriptForEnemyAI enemyAI = ship.GetComponent<BaseScriptForEnemyAI>();
			if (enemyAI != null && enemyAI.ShouldAttackPlayer())
			{
				// Rotate and attack the hostile ship
				Vector3 targetDirection = (ship.transform.position - transform.position).normalized;
				RotateTowardTarget(targetDirection, behavior.rotationSpeed);

				if (Time.time > lastAttackTime + behavior.attackCooldown)
				{
					Attack();
				}
			}
		}
	}

	public Vector3 ComputeObstacleAvoidance(Vector3 currentDirection)
	{
		Ray ray = new Ray(transform.position, currentDirection);
		if (Physics.Raycast(ray, out RaycastHit hit, obstacleAvoidanceRange, obstacleLayer))
		{
			Debug.Log("Avoiding obstacle");
			Vector3 avoidDir = hit.normal;

			if (avoidDir.sqrMagnitude < 0.001f)
			{
				avoidDir = Vector3.Cross(currentDirection, Vector3.up);
			}

			avoidDir.Normalize();

			Vector3 steering = avoidDir * obstacleAvoidanceWeight;
			return steering;
		}

		return Vector3.zero;
	}

	public virtual void SeekPlayer()
	{
		Vector3 direction = (player.position - transform.position).normalized;
		Vector3 avoidance = ComputeObstacleAvoidance(direction);
		if (avoidance != Vector3.zero)
		{
			direction = (direction + avoidance).normalized;
		}

		RotateTowardTarget(direction, behavior.rotationSpeed);

		float distanceToPlayer = Vector3.Distance(transform.position, player.position);

		if (distanceToPlayer > behavior.attackRange)
		{
			rb.velocity = transform.forward * behavior.chaseSpeed;
		}
		else
		{
			rb.velocity = Vector3.zero; // Stop moving when close to attack range
		}

		if (distanceToPlayer < behavior.attackRange && Time.time > lastAttackTime + attackCooldown)
		{
			Attack();
		}
	}

	public virtual void Attack()
	{
		if (Time.time < lastAttackTime + attackCooldown) return; // Cooldown check

		if (laserPrefab == null || firePoint == null)
		{
			Debug.LogError("Laser prefab or fire point is not assigned!");
			return;
		}

		GameObject laser = Instantiate(laserPrefab, firePoint.transform.position, firePoint.transform.rotation);
		Rigidbody laserRb = laser.GetComponent<Rigidbody>();
		if (laserRb == null)
		{
			Debug.LogError("Laser prefab is missing a Rigidbody component!");
			return;
		}
		laserRb.velocity = firePoint.transform.forward * 20f;

		Collider laserCollider = laser.GetComponent<Collider>();
		Collider enemyCollider = GetComponent<Collider>();
		if (laserCollider != null && enemyCollider != null)
		{
			Physics.IgnoreCollision(laserCollider, enemyCollider);
		}

		lastAttackTime = Time.time; // Update last attack time
		Debug.Log("Laser fired! Next attack in " + attackCooldown + " seconds.");
	}
}
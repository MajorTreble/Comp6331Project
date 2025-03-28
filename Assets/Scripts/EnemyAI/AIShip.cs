using UnityEngine;

using AI.BehaviorTree;
using Controller; // remove this
using Model.AI.BehaviorTree;

namespace Model.AI
{
	//LIMITATIONS! Some things are not yet implemented
	/*
	 * Single-Target Focus: Only tracks player as potential ally
		No Ship-to-Ship Relations: Doesn't check if ships should attack each other
		Limited Mission Awareness: Only knows about direct player relations 
	 */

	public class AIShip : Ship
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

		public AIState currentState = AIState.None;
		public AIState requestState = AIState.Idle;

		[Header("Layer Masks")]
		public LayerMask shipLayer;

		protected AIShipBehaviorTree BT = new AIShipBehaviorTree();

		public virtual void Start()
		{
			this.player = GameObject.FindGameObjectWithTag("Player").transform;
			this.rb = GetComponent<Rigidbody>();

			BT.aiShip = this;
			BT.Initialize();
			if (laserPrefab != null && laserPrefab.GetComponent<LaserProjectile>() == null)
			{
				laserPrefab.AddComponent<LaserProjectile>();
			}
		}

		protected virtual void Update()
		{
			if (player == null || rb == null)
			{
				return;
			}

			UpdateDecision();
			UpdateState();
		}

		protected virtual void UpdateDecision()
		{
			BT.Evaluate();

			// Mission-specific state transitions
			if (JobController.Inst.currJob != null)
			{
				if (IsMissionTarget() && currentState != AIState.Flee)
				{
					requestState = AIState.Flee;
				}
				else if (IsMissionAlly() && currentState != AIState.AllyAssist)
				{
					requestState = AIState.AllyAssist;
				}
			}

			// Existing decision logic
			if (ShouldAllyWithPlayer())
			{
				requestState = AIState.AllyAssist;
			}
		}

		protected bool IsMissionTarget()
		{
			if (JobController.Inst.currJob == null) return false;

			return behavior.faction switch
			{
				AIBehavior.Faction.Faction1 when JobController.Inst.currJob.jobTarget == JobTarget.Faction1 => true,
				AIBehavior.Faction.Faction2 when JobController.Inst.currJob.jobTarget == JobTarget.Faction2 => true,
				AIBehavior.Faction.Pirates when JobController.Inst.currJob.jobTarget == JobTarget.Pirate => true,
				_ => false
			};
		}

		protected bool IsMissionAlly()
		{
			if (JobController.Inst.currJob == null) return false;

			return behavior.faction switch
			{
				AIBehavior.Faction.Faction1 when JobController.Inst.currJob.rewardType == RepType.Faction1 => true,
				AIBehavior.Faction.Faction2 when JobController.Inst.currJob.rewardType == RepType.Faction2 => true,
				AIBehavior.Faction.Pirates when JobController.Inst.currJob.rewardType == RepType.Pirate => true,
				_ => false
			};
		}

		protected void UpdateState()
		{

			if (requestState != currentState)
			{
				switch (currentState)
				{
					case AIState.Idle:
						ExitIdle();
						break;
					case AIState.Roam:
						ExitRoam();
						break;
					case AIState.Seek:
						ExitSeek();
						break;
					case AIState.Flee:
						ExitFlee();
						break;
					case AIState.AllyAssist:
						ExitAllyAssist();
						break;
				}

				switch (requestState)
				{
					case AIState.Idle:
						EnterIdle();
						break;
					case AIState.Roam:
						EnterRoam();
						break;
					case AIState.Seek:
						EnterSeek();
						break;
					case AIState.Flee:
						EnterSeek();
						break;
					case AIState.AllyAssist:
						EnterAllyAssist();
						break;
				}

				currentState = requestState;
			}

			// Execute behavior
			switch (currentState)
			{
				case AIState.Idle:
					break;
				case AIState.Roam:
					UpdateRoam();
					break;
				case AIState.Seek:
					UpdateSeek();
					break;
				case AIState.Flee:
					UpdateFlee();
					break;
				case AIState.AllyAssist:
					UpdateAllyAssist();
					break;
			}
		}

		public void SetBehavior(AIBehavior newBehavior)
		{
			behavior = newBehavior; // Set the behavior
		}

		public virtual bool ShouldAttackPlayer()
		{
			if (behavior == null || playerReputation == null) return false;

			// Always prioritize ally status
			if (ShouldAllyWithPlayer()) return false;

			// Get current job context
			Job currentJob = JobController.Inst.currJob;
			bool isMissionTarget = IsMissionTarget();
			bool isMissionAlly = IsMissionAlly();

			// Mission-based behavior
			if (currentJob != null)
			{
				// Strategic responses based on job type
				switch (currentJob.jobType)
				{
					case JobType.Hunt when isMissionTarget:
						// If we're the hunt target, become aggressive
						return true;

					case JobType.Defend when isMissionTarget:
						// If we're the defend target, protect ourselves
						return playerReputation.GetReputation(behavior.faction) < behavior.allyReputationThreshold;

					case JobType.Mine when behavior.faction == AIBehavior.Faction.Pirates:
						// Pirates attack mining operations
						return true;

					case JobType.Deliver when isMissionTarget:
						// Intercept delivery missions targeting our faction
						return true;
				}

				// Faction response to mission alignment
				if (isMissionAlly)
				{
					// Support player if we're the rewarded faction
					return false;
				}

				if (currentJob.rewardType == RepType.Pirate && behavior.faction == AIBehavior.Faction.Pirates)
				{
					// Pirates support player if mission benefits them
					return false;
				}
			}

			// Default reputation-based behavior
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

		protected bool ShouldAllyWithPlayer()
		{
			if (behavior == null || playerReputation == null) return false;
			return playerReputation.GetReputation(behavior.faction) >= behavior.allyReputationThreshold;
		}


		protected virtual void AttackEnemiesNearPlayer()
		{
			if (!player || !behavior) return;

			// Expand detection parameters
			Collider[] nearbyEnemies = Physics.OverlapSphere(
				player.position,
				behavior.allyAssistRange * 1.5f, // Increased range
				shipLayer  // Check multiple layers
			);

			foreach (Collider enemy in nearbyEnemies)
			{
				AIShip enemyAI = enemy.GetComponent<AIShip>();
				if (enemyAI && enemyAI.ShouldAttackPlayer())
				{
					Vector3 targetDirection = (enemy.transform.position - transform.position).normalized;
					RotateTowardTarget(targetDirection, behavior.rotationSpeed * 0.8f); // Smoother rotation

					if (Time.time > lastAttackTime + behavior.attackCooldown)
					{
						Attack();
						break; // Focus on one target at a time
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

		public virtual void Attack()
		{
			// 1.Cooldown check(unchanged)

			if (Time.time < lastAttackTime + behavior.attackCooldown) return;
			Debug.Log($"{gameObject.name} attacking at {Time.time}");

			// 2. Prefab validation (unchanged)
			if (laserPrefab == null || firePoint == null)
			{
				Debug.LogError("Laser prefab or fire point is not assigned!");
				return;
			}

			// 3. Projectile creation with additional safety checks
			GameObject laser = Instantiate(laserPrefab, firePoint.transform.position, firePoint.transform.rotation); //Currently the laser rotation is wrong, I will fix it later
			//GameObject laser = Instantiate(laserPrefab, firePoint.transform.position, Quaternion.Euler(-90, firePoint.transform.rotation.eulerAngles.y, firePoint.transform.rotation.eulerAngles.z));


			// 4. Enhanced physics setup
			Rigidbody laserRb = laser.GetComponent<Rigidbody>();
			if (laserRb == null)
			{
				laserRb = laser.AddComponent<Rigidbody>(); // Auto-add if missing
				Debug.LogWarning("Added Rigidbody to laser prefab at runtime");
			}
			laserRb.velocity = firePoint.transform.forward * 30f;

		
			lastAttackTime = Time.time;
		}


		public void TakeDamage(float damage)
		{
			health -= damage;
			health = Mathf.Clamp(health, 0, maxHealth); // Ensure health doesn't go below 0 or above maxHealth
		}

		// Idle //

		protected void EnterIdle()
		{

		}

		protected void ExitIdle()
		{

		}

		protected void UpdateIdle()
		{
		}

		// Roam //

		protected void EnterRoam()
		{
			SetNewRoamTarget();
		}

		protected void ExitRoam()
		{

		}

		protected void UpdateRoam()
		{
			if (roamTarget == null)
			{
				return;
			}

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

		// Seek //

		protected void EnterSeek()
		{

		}

		protected void ExitSeek()
		{

		}

		public virtual void UpdateSeek()
		{
			Vector3 direction = (player.position - transform.position).normalized;

			// If we're a faction ship on defense mission, maintain distance
			if (JobController.Inst.currJob?.jobType == JobType.Defend &&
				IsMissionAlly() &&
				Vector3.Distance(transform.position, player.position) < behavior.attackRange)
			{
				direction = -direction; // Back away
			}

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

		// Flee //

		protected void EnterFlee()
		{

		}

		protected void ExitFlee()
		{

		}

		protected void UpdateFlee()
		{
			Vector3 fleeDirection = (transform.position - player.position).normalized;
			Vector3 avoidance = ComputeObstacleAvoidance(fleeDirection);
			if (avoidance != Vector3.zero)
			{
				fleeDirection = (fleeDirection + avoidance).normalized;
			}

			RotateTowardTarget(fleeDirection, behavior.rotationSpeed * 1.5f); // Faster rotation while fleeing
			rb.velocity = transform.forward * behavior.chaseSpeed * 1.5f; // Faster movement while fleeing
		}

		// Flee //

		protected void EnterAllyAssist()
		{

		}

		protected void ExitAllyAssist()
		{

		}

		protected void UpdateAllyAssist()
		{
			// Move toward the player to assist
			//Vector3 direction = (player.position - transform.position).normalized;
			//Vector3 avoidance = ComputeObstacleAvoidance(direction);

			//if (avoidance != Vector3.zero)
			//{
			//	direction = (direction + avoidance).normalized;
			//}

			//RotateTowardTarget(direction, behavior.rotationSpeed);
			//rb.velocity = direction * behavior.chaseSpeed;

			// Attack enemies near the player
			//AttackEnemiesNearPlayer();

			// Maintain distance from player
			float desiredDistance = 15f;
			Vector3 playerDirection = (player.position - transform.position).normalized;
			Vector3 targetPosition = player.position - playerDirection * desiredDistance;

			// Smooth movement
			Vector3 direction = (targetPosition - transform.position).normalized;
			direction = Vector3.Lerp(transform.forward, direction, 0.1f);
			Vector3 avoidance = ComputeObstacleAvoidance(direction);

			if (avoidance != Vector3.zero)
			{
				direction = (direction + avoidance).normalized;
			}

			// Smooth rotation
			RotateTowardTarget(direction, behavior.rotationSpeed * 0.8f);

			// Use physics-based movement
			rb.AddForce(direction * behavior.chaseSpeed * Time.deltaTime, ForceMode.VelocityChange);

			// Attack enemies near player
			AttackEnemiesNearPlayer();



		}
	}
}
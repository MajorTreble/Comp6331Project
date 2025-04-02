using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

using AI.BehaviorTree;
using AI.Steering;
using Controller;
using Manager;
using Model.AI.BehaviorTree;
using static Model.Faction;
using UnityEngine.Assertions.Must;

namespace Model.AI
{
	public enum Relationship { Enemy, Neutral }

	public class AIShip : Ship
	{
		protected Vector3 roamTarget;
		protected Transform target;

		public Faction faction = null;
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

		// Define AI states for finite state machine
		public AIState currentState = AIState.Idle;
		public AIState requestedState = AIState.Roam;

		[Header("Layer Masks")]
		public LayerMask shipLayer;

		protected AIShipBehaviorTree BT = new AIShipBehaviorTree();

		public Ship targetShip = null;

		// ---------- New fields for fuzzy logic and group behavior ----------
		protected Relationship relationship = Relationship.Neutral;

		// For group behavior:
		public bool isInGroup = false;     // flag to indicate this ship is part of a formation
		public bool isLeader = false;      // flag indicating if this ship is the leader
		public AIShip groupLeader = null;
		private List<AIShip> groupFollowers = new List<AIShip>(); // Followers if this ship is a leader.
		private Vector3 formationOffset;
		private Transform currentTarget = null;  // The current enemy target this AIShip is engaging (could be player or another ship).
		private bool isEngagingTarget = false;   // Whether currently in combat behavior (used for formation followers to copy leader).
		private bool hostile;

		public override void Start()
		{
			base.Start();

			BT.aiShip = this;
			BT.Initialize();

			currentState = AIState.Idle;
			requestedState = AIState.Roam;

			if (laserPrefab != null && laserPrefab.GetComponent<LaserProjectile>() == null)
			{
				laserPrefab.AddComponent<LaserProjectile>();
			}
		}

		public void InitializeShip(AIShip designatedLeader = null)
		{
			if (behavior.groupMode == AIBehavior.GroupMode.Formation)
			{
				isInGroup = true;
				if (designatedLeader == null)
				{
					// This ship is the leader.
					groupLeader = this;
					isLeader = true;
					groupFollowers = new List<AIShip>();
				}
				else
				{
					// This ship is a follower. Its leader is provided.
					groupLeader = designatedLeader;
					isLeader = false;
					// Compute a simple formation offset (e.g., based on count of followers).
					float offsetDistance = 10f;
					int followerIndex = designatedLeader.groupFollowers.Count + 1;
					formationOffset = new Vector3(offsetDistance * followerIndex, 0, -offsetDistance * followerIndex);
					designatedLeader.groupFollowers.Add(this);
				}
			}
			else
			{
				isInGroup = false;
			}
		}

		private void Update()
		{
			Debug.Assert(faction != null && behavior != null);
			if (!(faction != null && behavior != null))
			{
				enabled = false;
				return;
			}

			UpdateDecision();
			EvaluateCombatTarget();
			requestedState = AIState.Seek;
			UpdateStateMachine();

			steeringAgent.Update();
		}

		protected virtual void UpdateDecision()
		{
			// Formation override: if in formation and not the leader, follow the leader.
			if (behavior.groupMode == AIBehavior.GroupMode.Formation && isInGroup && groupLeader != null && groupLeader != this)
			{
				FollowFormationLeader();
				// Also mirror leader's combat if engaged.
				if (groupLeader.isEngagingTarget && groupLeader.currentTarget != null)
				{
					currentTarget = groupLeader.currentTarget;
					EngageTarget();
				}
				else
				{
					currentTarget = null;
					isEngagingTarget = false;
				}
				return; // Skip independent behavior if following formation.
			}

			BT.Evaluate();

			// Mission overrides:
			if (JobController.Inst.currJob != null)
			{
				Debug.Log($"[{name}] Current Mission: Target = {JobController.Inst.currJob.jobTarget}, Reward = {JobController.Inst.currJob.allyFaction.factionType}");
			}
			else
			{
				// No mission override: use fuzzy logic.
				Relationship rel = AIHelper.EvaluateRelationship(faction);
				float aggressionLevel = AIHelper.EvaluateAggressionLevel(this);

				if (rel == Relationship.Enemy)
				{
					if (aggressionLevel > 0.7f)
						requestedState = AIState.Seek;
					else if (aggressionLevel < 0.3f)
						requestedState = AIState.Flee;
					else
						requestedState = AIState.Roam;
				}
				else
				{
					requestedState = AIState.Roam;
				}

			}
		}

		protected void UpdateStateMachine()
		{

			if (requestedState != currentState)
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
					case AIState.Attack:
						ExitAttack();
						break;
				}

				switch (requestedState)
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
					case AIState.Attack:
						EnterAttack();
						break;
				}

				currentState = requestedState;
			}

			// Execute state
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
				case AIState.Attack:
					UpdateAttack();
					break;
			}
		}

		public void TakeDamage(GameObject attacker)
		{
			Debug.Log($"[{name}] was hit by {attacker.name}");

			if (attacker.CompareTag("Player"))
			{
				currentTarget = attacker.transform;
				requestedState = AIState.Attack;
			}
			else
			{
				AIShip attackerAI = attacker.GetComponent<AIShip>();
				if (attackerAI != null)
				{
					Debug.Log($"[{name}] was hit by [{attackerAI.name}] from faction {attackerAI.faction}");
					if (AIHelper.IsEnemy(faction, attackerAI.faction))
					{
						currentTarget = attacker.transform;
						requestedState = AIState.Attack;
					}
				}
			}
		}

		private void FollowFormationLeader()
		{
			if (groupLeader == null) return;
			Vector3 desiredPos = groupLeader.transform.position + groupLeader.transform.TransformVector(formationOffset);
			Vector3 moveDir = (desiredPos - transform.position).normalized;
			RotateTowardTarget(moveDir, behavior.rotationSpeed);
			// Simple movement: directly interpolate position (for smooth formation movement, use proper steering).
			transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime);
		}

		private void EngageTarget()
		{
			if (currentTarget == null) return;
			isEngagingTarget = true;
			Debug.Log($"[{name}] Seeking target: {currentTarget.name}");

			float d = Vector3.Distance(transform.position, currentTarget.position);
			if (d > behavior.attackRange)
			{
				Vector3 dir = (currentTarget.position - transform.position).normalized;
				transform.position += dir * behavior.chaseSpeed * Time.deltaTime;
				Quaternion targetRot = Quaternion.LookRotation(dir);
				transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, behavior.rotationSpeed * Time.deltaTime);
			}
			else
			{
				AttackTarget();
			}
		}

		private void AttackTarget()
		{
			// Placeholder for actual attack behavior.
			Attack();
		}

		private void EvaluateCombatTarget()
		{
			Job job = JobController.Inst.currJob;
			GameObject player = GameObject.FindGameObjectWithTag("Player");
			if(player == null)
			{
				Debug.LogWarning("Player null");
				return;
			}

			// Fallback: Use reputation + fuzzy logic to decide about player
			if (player != null && behavior != null && AIHelper.ShouldAttackPlayer(this, player, job))
			{
				float dist = Vector3.Distance(transform.position, player.transform.position);
				if (dist <= behavior.detectionRadius)
				{
					Debug.Log($"[{name}] Player is enemy. Engaging.");
					currentTarget = player.transform;
					requestedState = AIState.Seek;
					return;
				}
			}

			// Look for other hostile AI ships
			AIShip enemyShip = FindNearestHostile();
			if (enemyShip != null)
			{
				Debug.Log($"[{name}] Found hostile target: {enemyShip.name}");
				currentTarget = enemyShip.transform;
				requestedState = AIState.Seek;
				return;
			}

			currentTarget = null;
			requestedState = AIState.Roam;
		}

		private AIShip FindNearestHostile()
		{
			AIShip nearest = null;
			float nearestDist = float.MaxValue;

			foreach (Ship ship in SpawningManager.Instance.shipList)
			{
				if (ship == this) continue;

				AIShip other = ship as AIShip;
				if (other == null) continue;

				Job job = JobController.Inst.currJob;

				hostile = AIHelper.IsMissionTarget(other) ||
				AIHelper.IsEnemy(faction, other.faction) ||
				(other.faction != this.faction && AIHelper.IsEnemy(faction, other.faction));


				if (!hostile) continue;
				if (hostile)
					Debug.Log($"[{name}] Found hostile: {other.name} due to mission logic.");

				float d = Vector3.Distance(transform.position, other.transform.position);
				if (d < nearestDist)
				{
					nearestDist = d;
					nearest = other;
				}
			}

			return nearest;
		}

		public void SetBehavior(AIBehavior newBehavior)
		{
			behavior = newBehavior; // Set the behavior
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
			roamTarget = transform.position + Random.insideUnitSphere * roamRadius;
			roamTarget.y = 0; 
		}

		protected virtual void UpdateGroupBehavior()
		{
			if (!isLeader && groupLeader != null)
			{
				// For simplicity, have followers move toward a position offset from the leader.
				Vector3 desiredOffset = (transform.position - groupLeader.transform.position).normalized * 10f;
				Vector3 formationTarget = groupLeader.transform.position + desiredOffset;
				Vector3 direction = (formationTarget - transform.position).normalized;
				Vector3 avoidance = ComputeObstacleAvoidance(direction);
				if (avoidance != Vector3.zero)
					direction = (direction + avoidance).normalized;
				RotateTowardTarget(direction, behavior.rotationSpeed);
				rb.velocity = direction * behavior.roamSpeed;
			}
		}

		protected virtual void SetTarget(Transform newTarget)
		{
			target = newTarget;
		}


		protected virtual void AttackEnemiesNearPlayer(GameObject player)
		{
			if (!player || !behavior) return;

			// Expand detection parameters
			Collider[] nearbyEnemies = Physics.OverlapSphere(
				player.transform.position,
				behavior.allyAssistRange * 1.5f, // Increased range
				shipLayer  // Check multiple layers
			);

			Job job = JobController.Inst.currJob;

			foreach (Collider enemy in nearbyEnemies)
			{
				AIShip enemyAI = enemy.GetComponent<AIShip>();
				if (enemyAI && AIHelper.ShouldAttackPlayer(this, player, job))
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

			if (Time.time < lastAttackTime + behavior.attackCooldown) return;
			if (laserPrefab == null || firePoint == null) return;

			// Ensure laserPrefab has EnemyLaser script
			GameObject laser = Instantiate(laserPrefab, firePoint.transform.position, firePoint.transform.rotation);

			Debug.Log($"[{name}] Engaging target: {currentTarget?.name ?? "None"}");

			EnemyLaser laserScript = laser.GetComponent<EnemyLaser>();
			if (laserScript != null)
			{
				laserScript.shooter = gameObject;
				laserScript.faction = faction;
			}

			// Just move the laser forward using physics or custom code
			Rigidbody laserRb = laser.GetComponent<Rigidbody>();
			if (laserRb == null)
			{
				laserRb = laser.AddComponent<Rigidbody>();
			}
			laserRb.velocity = firePoint.transform.forward * 30f;

			lastAttackTime = Time.time;
		}



		protected float GetPlayerReputation(AIBehavior.Faction faction)
		{
			if (GameManager.Instance.reputation != null)
			{
				var repEntry = GameManager.Instance.reputation.reputations.Find(r => r.fac.factionType.ToString() == faction.ToString());
				if (repEntry != null)
					return repEntry.value;
			}
			return 0f;
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
			steeringAgent.movements.Add(new Wander());
			steeringAgent.movements.Add(new LookWhereYouAreGoing());

			CollisionAvoidance avoidance = new CollisionAvoidance();
			avoidance.weight = 2.0f;
			foreach (Ship ship in SpawningManager.Instance.shipList)
			{
				SteeringAgent agent = ship.steeringAgent;
				if (agent != null)
				{
					avoidance.avoidList.Add(agent);
				}
			}
			steeringAgent.movements.Add(avoidance);
		}

		protected void ExitRoam()
		{
			steeringAgent.movements.Clear();
		}

		protected void UpdateRoam()
		{
		}

		// Seek //

		protected void EnterSeek()
		{
			steeringAgent.movements.Add(new Pursue());
			steeringAgent.movements.Add(new LookWhereYouAreGoing());
		}

		protected void ExitSeek()
		{
			steeringAgent.movements.Clear();
		}

		public virtual void UpdateSeek()
		{
			if(currentTarget == null)
			{
				Debug.LogWarning("Current target is null, need to be checked");
				return;
			}

			float dist = Vector3.Distance(transform.position, currentTarget.position);
			if (dist <= behavior.attackRange)
			{
				requestedState = AIState.Attack;
				return;
			}
			if (dist > behavior.detectionRadius * 1.5f)
			{
				requestedState = AIState.Roam;
				return;
			}
		}

		// Flee //

		protected void EnterFlee()
		{
			steeringAgent.movements.Add(new FaceAway());
			steeringAgent.movements.Add(new Evade());
		}

		protected void ExitFlee()
		{
			steeringAgent.movements.Clear();
		}

		protected virtual void UpdateFlee()
		{
		}

		// Ally Assist //

		protected void EnterAllyAssist()
		{

		}

		protected void ExitAllyAssist()
		{

		}

		protected void UpdateAllyAssist()
		{

		}

		// Attack //

		protected void EnterAttack()
		{

		}

		protected void ExitAttack()
		{

		}

		protected virtual void UpdateAttack()
		{
			//if (player == null) return;
			if (currentTarget == null || behavior == null) return;

			//float dist = Vector3.Distance(transform.position, player.position);
			float dist = Vector3.Distance(transform.position, currentTarget.position);
			if (dist > behavior.attackRange)
			{
				requestedState = AIState.Seek;
				return;
			}

			//Vector3 direction = (player.position - transform.position).normalized;
			Vector3 direction = (currentTarget.position - transform.position).normalized;
			RotateTowardTarget(direction, behavior.rotationSpeed);

			if (Time.time >= lastAttackTime + behavior.attackCooldown)
			{
				Attack();
				lastAttackTime = Time.time;
			}
		}

	}
}
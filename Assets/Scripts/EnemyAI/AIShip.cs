using UnityEngine;

using AI.BehaviorTree;
using Controller; // remove this
using Model.AI.BehaviorTree;
using static UnityEngine.GraphicsBuffer;
using System.Collections.Generic;
using static AIBehavior;
using Manager;
using static Model.Faction;
using AI.Steering;

namespace Model.AI
{
	public enum Relationship { Enemy, Neutral }

	public class AIShip : Ship
	{
		public ReputationData playerReputation; // Player's reputation
		public Transform player;
		public Rigidbody rb;
		protected Vector3 roamTarget;
		protected Transform target;

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
		public FactionType factionType;


		//private LookWhereYouAreGoing lookWhereGoing;


		// Define AI states for finite state machine
		public enum AIState { Roaming, Seek, Attack, Flee }
		public AIState currentState = AIState.Roaming;
		public AIState requestedState = AIState.Roaming;

		[Header("Layer Masks")]
		public LayerMask shipLayer;

		protected AIShipBehaviorTree BT = new AIShipBehaviorTree();

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
		public float currentHealth = 100f;

		private SteeringAgent steeringAgent;
		private Wander wander;



		public virtual void Start()
		{
			rb = GetComponent<Rigidbody>();

			GameObject playerObj = GameObject.FindWithTag("Player");
			if (playerObj != null)
				player = playerObj.transform;

			if (wander == null)
				wander = new Wander();
			//lookWhereGoing = GetComponent<LookWhereYouAreGoing>();
			//if (lookWhereGoing == null)
			//	lookWhereGoing = new LookWhereYouAreGoing();
			if (steeringAgent == null)
				steeringAgent = new SteeringAgent(gameObject);

			steeringAgent.movements.Clear(); // Optional safety
			steeringAgent.movements.Add(wander);

			BT.aiShip = this;
			BT.Initialize();

			currentState = AIState.Roaming;
			requestedState = AIState.Roaming;

			SetNewRoamTarget();

			if (laserPrefab != null && laserPrefab.GetComponent<LaserProjectile>() == null)
			{
				laserPrefab.AddComponent<LaserProjectile>();
			}
		}

		public void InitializeShip(AIBehavior behaviorAsset, Faction.FactionType factionData, AIShip designatedLeader = null)
		{
			behavior = behaviorAsset;
			factionType = factionData;

			// Find player reference (assuming player tagged "Player").
			GameObject playerObj = GameObject.FindWithTag("Player");
			if (playerObj != null) player = playerObj.transform;

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

			Start(); // Optionally call Start() logic if not automatically done.
		}


		// Membership functions (linear, as suggested)
		protected float LeftShoulderMembership(float value, float x, float y)
		{
			if (value <= x) return 1f;
			if (value >= y) return 0f;
			return (y - value) / (y - x);
		}

		protected float RightShoulderMembership(float value, float x, float y)
		{
			if (value <= x) return 0f;
			if (value >= y) return 1f;
			return (value - x) / (y - x);
		}

		protected Relationship EvaluateRelationship()
		{
			float rep = playerReputation.GetReputation(behavior.faction); // 0 to 100
			float enemyThresholdLow = 30f;
			float enemyThresholdHigh = 50f;
			float friendlyThresholdLow = 50f;
			float friendlyThresholdHigh = 70f;

			float enemyMembership = LeftShoulderMembership(rep, enemyThresholdLow, enemyThresholdHigh);
			float friendlyMembership = RightShoulderMembership(rep, friendlyThresholdLow, friendlyThresholdHigh);

			if (enemyMembership > 0.5f)
				return Relationship.Enemy;
			else
				return Relationship.Neutral; // You could expand this to include Friendly later
		}


		private float EvaluateAggressionLevel()
		{
			int friendlyCount = CountNearbyFriendlies();
			int enemyCount = CountNearbyEnemies();
			float selfStrength = currentHealth / maxHealth;

			// Define thresholds for counts (these can be tuned)
			float friendHighThreshold = 5f;
			float enemyHighThreshold = 5f;
			float friendlyHigh = Mathf.Clamp01(friendlyCount / friendHighThreshold);
			float enemyHigh = Mathf.Clamp01(enemyCount / enemyHighThreshold);

			// Fuzzy rules:
			// If many enemies and few friendlies, be aggressive.
			float ruleAggressive = Mathf.Min(1f - friendlyHigh, enemyHigh);
			// Otherwise, if many friendlies and few enemies, be calm.
			float ruleCalm = Mathf.Min(friendlyHigh, 1f - enemyHigh);
			// Average in other cases.
			float ruleAverage = 1f - Mathf.Abs(enemyHigh - friendlyHigh);

			float calmValue = behavior.aggressionCalmValue;
			float avgValue = behavior.aggressionAverageValue;
			float aggressiveValue = behavior.aggressionAggressiveValue;

			float weightedSum = ruleCalm * calmValue + ruleAverage * avgValue + ruleAggressive * aggressiveValue;
			float totalWeight = ruleCalm + ruleAverage + ruleAggressive;
			float aggression = (totalWeight > 0f) ? weightedSum / totalWeight : avgValue;
			return aggression;
		}

		private int CountNearbyFriendlies()
		{
			int count = 0;
			float radius = 100f;
			foreach (AIShip other in SpawningManager.Instance.shipList)
			{
				if (other == this) continue;
				if (other.factionType == this.factionType)
				{
					float d = Vector3.Distance(transform.position, other.transform.position);
					if (d <= radius) count++;
				}
			}
			return count;
		}

		private int CountNearbyEnemies()
		{
			int count = 0;
			float radius = 100f;
			foreach (AIShip other in SpawningManager.Instance.shipList)
			{
				if (other == this) continue;
				if (other.factionType != this.factionType && IsHostile(factionType, other.factionType))
				{
					float d = Vector3.Distance(transform.position, other.transform.position);
					if (d <= radius) count++;
				}
			}
			if (player != null && IsPlayerEnemy())
			{
				float d = Vector3.Distance(transform.position, player.position);
				if (d <= radius) count++;
			}
			return count;
		}

		private void Update()
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

			// Otherwise, use the behavior tree to decide actions.
			BT.Evaluate();

			// Mission overrides:
			if (JobController.Inst.currJob != null)
			{
				Debug.Log($"[{name}] Current Mission: Target = {JobController.Inst.currJob.jobTarget}, Reward = {JobController.Inst.currJob.rewardType}");

				// If the mission targets our faction (we are being hunted), always attack.
				if (IsMissionTargetFaction())
				{
					Debug.Log($"[{name}] I am being hunted due to mission. Seeking Player.");
					requestedState = AIState.Seek;
				}
				// If the mission rewards our faction, then even if reputation is normally hostile, remain neutral.
				else if (IsMissionAllyFaction())
				{
					Debug.Log($"[{name}] My faction is supported by this mission. Roaming instead of attacking.");
					requestedState = AIState.Roaming;
				}
			}
			else
			{
				// No mission override: use fuzzy logic.
				Relationship rel = EvaluateRelationship();
				float aggressionLevel = EvaluateAggressionLevel();

				if (rel == Relationship.Enemy)
				{
					if (aggressionLevel > 0.7f)
						requestedState = AIState.Seek;
					else if (aggressionLevel < 0.3f)
						requestedState = AIState.Flee;
					else
						requestedState = AIState.Roaming;
				}
				else
				{
					requestedState = AIState.Roaming;
				}

			}
			EvaluateCombatTarget();
			UpdateStateMachine();
			steeringAgent?.Update();

		}


		public static bool IsHostile(Faction.FactionType a, Faction.FactionType b)
		{
			// Example logic:
			if (a == Faction.FactionType.Pirates && b != Faction.FactionType.Pirates) return true;
			if (b == Faction.FactionType.Pirates && a != Faction.FactionType.Pirates) return true;
			if (a == Faction.FactionType.Solo || b == Faction.FactionType.Solo) return false;
			// In your game, Colonial (faction) and Earth (faction) are friendly.
			return false;
		}

		private bool IsPlayerEnemy()
		{
			// For now, we assume if player's reputation with this faction is low, the player is enemy.
			return GetPlayerReputation(behavior.faction) < behavior.reputationThreshold;
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
					Debug.Log($"[{name}] was hit by [{attackerAI.name}] from faction {attackerAI.factionType}");
					if (IsHostile(this.factionType, attackerAI.factionType))
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
			Debug.Log($"[{name}] Seeking target: {player.name}");

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
			if (player != null && ShouldAttackPlayer())
			{
				float dist = Vector3.Distance(transform.position, player.position);
				if (dist <= behavior.detectionRadius)
				{
					currentTarget = player;
					requestedState = AIState.Seek;
					return;
				}
			}

			// Look for nearest enemy ship (excluding player)
			AIShip enemyShip = FindNearestHostile();
			if (enemyShip != null)
			{
				currentTarget = enemyShip.transform;
				requestedState = AIState.Seek;
				return;
			}

			currentTarget = null;
			requestedState = AIState.Roaming;
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

				bool hostile = (other.factionType != this.factionType && IsHostile(factionType, other.factionType));
				if (!hostile) continue;

				float d = Vector3.Distance(transform.position, other.transform.position);
				if (d < nearestDist)
				{
					nearestDist = d;
					nearest = other;
				}
			}
			return nearest;
		}

		private void UpdateStateMachine()
		{
			if (requestedState != currentState)
			{
				currentState = requestedState;
			}

			switch (currentState)
			{
				case AIState.Roaming:
					UpdateRoam();
					break;
				case AIState.Seek:
					UpdateSeek();
					break;
				case AIState.Attack:
					UpdateAttack();
					break;
				case AIState.Flee:
					UpdateFlee();
					break;
			}
		}

		protected virtual void UpdateDecision()
		{
			BT.Evaluate();

			// Mission-specific state transitions
			if (JobController.Inst.currJob != null)
			{
				if (IsMissionTarget() && currentState != AIState.Flee)
				{
					//requestState = AIState.Flee;
				}
				
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


		public virtual void UpdateSeek()
		{
			if (player == null || steeringAgent == null) return;

			if (!ShouldAttackPlayer())
			{
				requestedState = AIState.Roaming;
				return;
			}

			float dist = Vector3.Distance(transform.position, player.position);
			if (dist <= behavior.attackRange)
			{
				requestedState = AIState.Attack;
				return;
			}
			if (dist > behavior.detectionRadius * 1.5f)
			{
				requestedState = AIState.Roaming;
				return;
			}

			steeringAgent.UnTrackTarget();
			steeringAgent.targetPosition = player.position;
			Debug.Log($"[{name}] Seeking target: {player.name}");

			Seek seek = new Seek();
			var steering = seek.GetSteering(steeringAgent);
			rb.velocity += steering.linear * Time.deltaTime;
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

		protected virtual void UpdateFlee()
		{
			if (player == null || steeringAgent == null) return;

			if (!ShouldAttackPlayer())
			{
				requestedState = AIState.Roaming;
				return;
			}

			steeringAgent.UnTrackTarget();
			steeringAgent.targetPosition = player.position;

			Flee flee = new Flee();
			var steering = flee.GetSteering(steeringAgent);
			rb.velocity += steering.linear * Time.deltaTime;
		}

		public void SetBehavior(AIBehavior newBehavior)
		{
			behavior = newBehavior; // Set the behavior
		}

		public bool ShouldAttackPlayer()
		{
			if (player == null || behavior == null) return false;

			if (IsMissionTargetFaction()) return true;
			if (IsMissionAllyFaction()) return false;

			Relationship rel = EvaluateRelationship();
			return (rel == Relationship.Enemy);
		}

		protected bool IsMissionTargetFaction()
		{
			if (JobController.Inst.currJob == null) return false;
			// Map our factionType to JobTarget. Note: Colonial = Faction1, Earth = Faction2.
			switch (factionType)
			{
				case Faction.FactionType.Colonial:
					return JobController.Inst.currJob.jobTarget == JobTarget.Faction1;
				case Faction.FactionType.Earth:
					return JobController.Inst.currJob.jobTarget == JobTarget.Faction2;
				case Faction.FactionType.Pirates:
					return JobController.Inst.currJob.jobTarget == JobTarget.Pirate;
				case Faction.FactionType.Solo:
					return JobController.Inst.currJob.jobTarget == JobTarget.Solo;
				default:
					return false;
			}
		}

		protected bool IsMissionAllyFaction()
		{
			if (JobController.Inst.currJob == null) return false;
			// Map our factionType to RepType. Colonial = Faction1, Earth = Faction2.
			switch (factionType)
			{
				case Faction.FactionType.Colonial:
					return JobController.Inst.currJob.rewardType == RepType.Faction1;
				case Faction.FactionType.Earth:
					return JobController.Inst.currJob.rewardType == RepType.Faction2;
				case Faction.FactionType.Pirates:
					return JobController.Inst.currJob.rewardType == RepType.Pirate;
				case Faction.FactionType.Solo:
					return JobController.Inst.currJob.rewardType == RepType.Self;
				default:
					return false;
			}
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

		public bool IsPlayerInRange()
		{
			if (player == null) return false;
			float distanceToPlayer = Vector3.Distance(transform.position, player.position);
			return distanceToPlayer < detectionRadius;
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
			if (Time.time < lastAttackTime + behavior.attackCooldown) return;
			if (laserPrefab == null || firePoint == null) return;

			// Ensure laserPrefab has EnemyLaser script
			GameObject laser = Instantiate(laserPrefab, firePoint.transform.position, firePoint.transform.rotation);

			Debug.Log($"[{name}] Engaging target: {currentTarget?.name ?? "None"}");

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
			if (PlayerReputation.Inst != null)
			{
				var repEntry = PlayerReputation.Inst.reputations.Find(r => r.type.ToString() == faction.ToString());
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

			Vector3 direction = (roamTarget - transform.position).normalized;
			Vector3 avoidance = ComputeObstacleAvoidance(direction);

			if (avoidance != Vector3.zero)
			{
				direction = (direction + avoidance).normalized;
			}

			if (wander != null && steeringAgent != null)
			{
				var steer = wander.GetSteering(steeringAgent);
				rb.velocity += steer.linear * Time.deltaTime;
			}

			RotateTowardTarget(direction);
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


		// Flee //

		protected void EnterFlee()
		{

		}

		protected void ExitFlee()
		{

		}

		

		// Flee //

		protected void EnterAllyAssist()
		{

		}

		protected void ExitAllyAssist()
		{

		}

	}
}
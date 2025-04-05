using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

using AI;
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
        public AIBehavior behavior;

        public float detectionRadius = 100.0f;

        public LayerMask obstacleLayer; // Layer for obstacles (e.g., asteroids)

        //public float attackRange = 10f; // Attack range
        public float attackCooldown = 2f; // Cooldown between attacks
        public float lastAttackTime; // Time of the last attack
        public GameObject laserPrefab; // Laser prefab for attacks
        public GameObject firePoint; // Point from which lasers are fired

		protected Vector3 roamTarget;

        public float roamRadius = 150f;
        public float avoidanceForce = 5f; // Force to steer away from obstacles and ships

        public float obstacleAvoidanceRange = 5f; // Range to detect obstacles
        public float obstacleAvoidanceWeight = 5f; // Strength of avoidance

        public bool shouldPatrol = false;

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
        public AIGroup group = null;
        public List<LastKnownPosition> LKP;
        public List<Ship> hostileShips = new List<Ship>();
        public LastKnownPosition currentLKP = null;
        public Ship target = null;

        private Transform currentTarget = null;  // The current enemy target this AIShip is engaging (could be player or another ship).

        public void Awake()
        {
            LKP = new List<LastKnownPosition>();
        }

        public override void Start()
        {
            base.Start();
			rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
            BT.aiShip = this;
            BT.Initialize();

            currentState = AIState.Idle;
            requestedState = AIState.Roam;

            if (laserPrefab != null && laserPrefab.GetComponent<LaserProjectile>() == null)
            {
                laserPrefab.AddComponent<LaserProjectile>();
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

            UpdatePerception();
            UpdateDecision();
            EvaluateCombatTarget();
            UpdateStateMachine();

            steeringAgent.Update();
        }

        public void UpdatePerception()
        {
            foreach (LastKnownPosition lkp in LKP)
            {
                lkp.Update(Time.time);
            }

            foreach (Ship ship in SpawningManager.Instance.shipList)
            {
                if (ship == null || ship == this) continue;

                if (!AIHelper.IsHostile(this, ship))
                {
                    continue;
                }

                SetHostile(ship);

                float distance = Vector3.Distance(transform.position, ship.transform.position);
                LKPVisibility visibility = (distance < detectionRadius) ? LKPVisibility.Seen : LKPVisibility.NotSeen;

                if (visibility == LKPVisibility.Seen)
                {
                    RaycastHit hit;
                    Vector3 direction = (ship.transform.position - transform.position).normalized;
                    if (Physics.Raycast(transform.position, direction, out hit, detectionRadius, obstacleLayer))
                    {
                        if (!hit.collider.GetComponent<Ship>())
                        {
                            visibility = LKPVisibility.NotSeen;
                        }
                    }
                }

                LastKnownPosition lkpRecord = LKP.Find((x) => x.target == ship);
                if (visibility == LKPVisibility.Seen)
                {
                    if (lkpRecord == null)
                    {
                        lkpRecord = new LastKnownPosition(this, ship);
                        LKP.Add(lkpRecord);
                    }
                    lkpRecord.SetVisibility(ship, visibility, ship.transform.position, distance, Time.time);
                }
                else if (lkpRecord != null)
                {
                    lkpRecord.SetVisibility(ship, visibility);
                }
            }

            // Get best target
            if (AIGroup.GetBestTarget(this, ref currentLKP))
            {
                target = currentLKP.target;
            }
        }

		public Vector3 ComputeObstacleAvoidance(Vector3 currentDirection)
        {

          Vector3 avoidance = Vector3.zero;
          Collider[] hits = Physics.OverlapSphere(transform.position, obstacleAvoidanceRange, obstacleLayer); Debug.Log($"[{name}] Detected {hits.Length} obstacles");
          Debug.Log($"[{name}] Detected {hits.Length} obstacles");

          foreach (Collider hit in hits)
          {
            if (hit.gameObject == gameObject) continue; // Skip self

            Vector3 toObstacle = hit.transform.position - transform.position;
            float distance = toObstacle.magnitude;
            if (distance < obstacleAvoidanceRange)
            {
              avoidance += (-toObstacle.normalized) * (obstacleAvoidanceRange / distance);
            }
          }

          return avoidance.normalized * obstacleAvoidanceWeight;

        }

        protected virtual void UpdateDecision()
        {
            BT.Evaluate();
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
					case AIState.Patrol:
                        ExitPatrol();
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
                    case AIState.Formation:
                        ExitFormation();
                        break;
                }

                switch (requestedState)
                {
                    case AIState.Idle:
                        EnterIdle();
                        break;
					case AIState.Patrol:
                        EnterPatrol();
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
                    case AIState.Formation:
                        EnterFormation();
                        break;
                }

                currentState = requestedState;
            }

            // Execute state
            switch (currentState)
            {
                case AIState.Idle:
                    break;
				case AIState.Patrol:
                     UpdatePatrol();
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
                case AIState.Formation:
                    UpdateFormation();
                    break;
            }
        }

        public void SetHostile(Ship target)
        {
            if (group != null)
            {
                if (!group.hostileShips.Contains(target))
                {
                    group.hostileShips.Add(target);

                }
                return;
            }

            if (!hostileShips.Contains(target))
            {
                hostileShips.Add(target);
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

        private void EngageTarget()
        {
            if (currentTarget == null) return;
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
            if (player == null)
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

                bool hostile = AIHelper.IsMissionTarget(other) ||
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

        public void Fuzzy()
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
            }
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
            steeringAgent.maxSpeed = 10.0f;
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
            steeringAgent.maxSpeed = steeringAgent.initialMaxSpeed;
            steeringAgent.movements.Add(new Pursue());
            steeringAgent.movements.Add(new LookWhereYouAreGoing());
        }

        protected void ExitSeek()
        {
            steeringAgent.trackedTarget = null;
            steeringAgent.movements.Clear();
        }

        public virtual void UpdateSeek()
        {
            if (currentLKP == null)
            {
                return;
            }

            switch (currentLKP.visibility)
            {
                case LKPVisibility.Seen:
                    steeringAgent.trackedTarget = target.GetComponent<SteeringAgent>();
                    break;
                case LKPVisibility.SeenRecently:
                    steeringAgent.targetPosition = currentLKP.position;
                    steeringAgent.trackedTarget = null;
                    break;
                case LKPVisibility.NotSeen:
                    requestedState = AIState.Idle;
                    break;
            }

            if (currentTarget == null)
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

        // Formation //

        protected void EnterFormation()
        {
            steeringAgent.movements.Add(new Seek());
            steeringAgent.movements.Add(new LookWhereYouAreGoing());
        }

        protected void ExitFormation()
        {
            steeringAgent.movements.Clear();
        }

        protected void UpdateFormation()
        {
        }

		protected void EnterPatrol()
        {

          steeringAgent.movements.Clear();

          AIWaypointNavigator nav = GetComponent<AIWaypointNavigator>();
          if (nav != null && nav.path != null)
          {
            // Seek toward waypoints
            steeringAgent.movements.Add(new Seek());
            steeringAgent.movements.Add(new LookWhereYouAreGoing());

            // Collision avoidance for dynamic obstacles
            CollisionAvoidance avoidance = new CollisionAvoidance();
            avoidance.weight = 2.0f;
            avoidance.avoidList.AddRange(GetObstacles());
            steeringAgent.movements.Add(avoidance);

            // Separation behavior for same-faction ships
            Separation separation = new Separation();
            separation.Weight = 10.0f;
            separation.DesiredSeparation = 15.0f;
            steeringAgent.movements.Add(separation);
          }
          else
          {
            Debug.LogWarning($"[{name}] Patrol setup failed: No waypoints!");
            requestedState = AIState.Roam;
          }

        }

        private List<SteeringAgent> GetObstacles()
        {
          //To avoid ships of same faction
          List<SteeringAgent> obstacles = new List<SteeringAgent>();
          foreach (Ship ship in SpawningManager.Instance.shipList)
          {
            if (ship != this && ship.steeringAgent != null)
              obstacles.Add(ship.steeringAgent);
          }
          return obstacles;
        }

        protected void ExitPatrol()
        {
          steeringAgent.movements.Clear();
        }

        protected void UpdatePatrol()
        {

        }


        public void UpdatePatrolState(bool shouldPatrol)
        {
          this.shouldPatrol = shouldPatrol;
          if (shouldPatrol)
          {
            requestedState = AIState.Patrol;
          }
          else
          {
            requestedState = AIState.Roam;
          }
        }

        private void OnDrawGizmosSelected()
        {
          Gizmos.color = Color.yellow;
          Gizmos.DrawWireSphere(transform.position, obstacleAvoidanceRange);
        }

    }
}
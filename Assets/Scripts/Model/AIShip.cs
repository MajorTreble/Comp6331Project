using UnityEngine;

using AI.Steering;
using AI.BehaviorTree;
using Controller;
using Manager;
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
        public Faction faction = null;
        public AIBehavior behavior;

        public float detectionRadius = 25f;

        public LayerMask obstacleLayer; // Layer for obstacles (e.g., asteroids)

        //public float attackRange = 10f; // Attack range
        public float attackCooldown = 2f; // Cooldown between attacks
        public float lastAttackTime; // Time of the last attack

        public GameObject laserPrefab; // Laser prefab for attacks
        public GameObject firePoint; // Point from which lasers are fired

        public Ship target = null;

        public AIState currentState = AIState.None;
        public AIState requestState = AIState.Idle;

        [Header("Layer Masks")]
        public LayerMask shipLayer;

        protected AIShipBehaviorTree BT = new AIShipBehaviorTree();

        public override void Start()
        {
            base.Start();

            this.target = GameObject.FindGameObjectWithTag("Player").GetComponent<Ship>();

            BT.aiShip = this;
            BT.Initialize();
            if (laserPrefab != null && laserPrefab.GetComponent<LaserProjectile>() == null)
            {
                laserPrefab.AddComponent<LaserProjectile>();
            }
        }

        protected virtual void Update()
        {
            if (rb == null)
            {
                return;
            }

            UpdateDecision();
            UpdateState();
        }

        protected virtual void UpdateDecision()
        {
            BT.Evaluate();
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
            }
        }

        public override bool IsJobTarget()
        {
            if (JobController.Inst.currJob == null)
            {
                return false;
            }

            if (JobController.Inst.currJob.targetFaction == faction)
            {
                return true;
            }

            return base.IsJobTarget();
        }

        protected virtual void AttackEnemiesNearPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            // Expand detection parameters
            Collider[] nearbyEnemies = Physics.OverlapSphere(
                player.transform.position,
                behavior.allyAssistRange * 1.5f, // Increased range
                shipLayer  // Check multiple layers
            );

            foreach (Collider enemy in nearbyEnemies)
            {
                Job job = JobController.Inst.currJob;

                AIShip enemyAI = enemy.GetComponent<AIShip>();
                if (enemyAI && AIHelper.ShouldAttackPlayer(enemyAI, player, job, enemyAI.faction))
                {
                    Vector3 targetDirection = (enemy.transform.position - transform.position).normalized;
                    
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
            // 1.Cooldown check(unchanged)

            if (Time.time < lastAttackTime + behavior.attackCooldown) return;
            //Debug.Log($"{gameObject.name} attacking at {Time.time}");

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

        // Idle //

        protected void EnterIdle()
        {
            steering.Velocity = Vector3.zero;
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
            steering.movements.Add(new Wander());
            steering.movements.Add(new LookWhereYouAreGoing());

            CollisionAvoidance avoidance = new CollisionAvoidance();
            avoidance.weight = 2.0f;
            foreach (Ship ship in SpawningManager.Instance.shipList)
            {
                SteeringAgent agent = ship.steering;
                if (agent != null)
                {
                    avoidance.avoidList.Add(agent);
                }
            }
            steering.movements.Add(avoidance);
        }

        protected void ExitRoam()
        {
            steering.movements.Clear();
            steering.Velocity = Vector3.zero;
        }

        protected void UpdateRoam()
        {
        }

        // Seek //

        protected void EnterSeek()
        {
            steering.movements.Add(new Pursue());
            steering.movements.Add(new LookWhereYouAreGoing());
        }

        protected void ExitSeek()
        {
            steering.movements.Clear();
        }

        public virtual void UpdateSeek()
        {
            if (behavior == null)
            {

            }

            Transform player = GameObject.FindGameObjectWithTag("Player").transform;

            Vector3 direction = (player.position - transform.position).normalized;

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
            steering.movements.Add(new Flee());
            steering.movements.Add(new Evade());
        }

        protected void ExitFlee()
        {
            steering.movements.Clear();
            steering.Velocity = Vector3.zero;
        }

        protected void UpdateFlee()
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

            Transform player = GameObject.FindGameObjectWithTag("Player").transform;

            // Maintain distance from player
            float desiredDistance = 15f;
            Vector3 playerDirection = (player.position - transform.position).normalized;
            Vector3 targetPosition = player.position - playerDirection * desiredDistance;

            // Smooth movement
            Vector3 direction = (targetPosition - transform.position).normalized;

            // Use physics-based movement
            rb.AddForce(direction * behavior.chaseSpeed * Time.deltaTime, ForceMode.VelocityChange);

            // Attack enemies near player
            AttackEnemiesNearPlayer();
        }
    }
}
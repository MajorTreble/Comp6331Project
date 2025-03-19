using UnityEngine;


public enum AIState { Roaming, Seeking, Fleeing }


public class EnemyAI : BaseScriptForEnemyAI
{
	private AIState currentState = AIState.Roaming;
	public float health = 100f; // Current health
	public float maxHealth = 100f;
	public float attackRange = 10f;
	public float attackCooldown = 2f;
	public float lastAttackTime;
	public GameObject laserPrefab;
	public GameObject firePoint;

	private FactionBehavior factionBehavior; // Reference to faction-specific behavior

	void Start()
	{
		currentState = AIState.Roaming;
		if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
		if (rb == null) rb = GetComponent<Rigidbody>();

		if (player == null)
			Debug.LogError("Player not found! Make sure the player has the 'Player' tag.");
		if (rb == null)
			Debug.LogError("Rigidbody not found! Make sure the enemy has a Rigidbody component.");

		// Initialize faction-specific behavior
		if (factionBehavior == null) InitializeFactionBehavior();
		SetNewRoamTarget();
	}

	void FixedUpdate()
	{
		if (player == null || rb == null) return;

		if (factionBehavior == null)
		{
			Debug.LogError("EnemyAI: factionBehavior is null! InitializeFactionBehavior() might have failed.");
			return;
		}

		// Update state based on faction behavior
		currentState = factionBehavior.DetermineState();

		// Execute behavior based on state
		switch (currentState)
		{
			case AIState.Roaming:
				Roam();
				break;
			case AIState.Seeking:
				SeekPlayer();
				break;
			case AIState.Fleeing:
				Flee();
				break;
		}
	}

	void InitializeFactionBehavior()
	{

		if (behavior == null)
		{
			Debug.LogError("EnemyAI: 'behavior' is not assigned! Make sure it is set in the Inspector.");
			return;
		}

		switch (behavior.faction)
		{
			case AIBehavior.Faction.Faction1:
				factionBehavior = new Faction1Behavior(this);
				break;
			case AIBehavior.Faction.Faction2:
				factionBehavior = new Faction2Behavior(this);
				break;
			case AIBehavior.Faction.Pirates:
				factionBehavior = new PirateBehavior(this);
				break;
			case AIBehavior.Faction.Solo:
				factionBehavior = new SoloBehavior(this);
				break;
			default:
				factionBehavior = new Faction1Behavior(this); // Default behavior
				break;
		}
	}

	void SeekPlayer()
	{
		factionBehavior.SeekPlayer();
	}

	void Flee()
	{
		factionBehavior.Flee();
	}

	public void Attack()
	{
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

		Debug.Log("Laser fired! Next attack in " + attackCooldown + " seconds.");
	}
}

// Base class for faction-specific behavior
public abstract class FactionBehavior
{
	protected EnemyAI enemyAI;

	public FactionBehavior(EnemyAI enemyAI)
	{
		this.enemyAI = enemyAI;
	}

	public abstract AIState DetermineState();
	public abstract void SeekPlayer();
	public abstract void Flee();
}

// Faction 1: Organized, coordinated tactics
public class Faction1Behavior : FactionBehavior
{
	private Vector3 cutOffPosition; // Position to cut off the player's path

	public Faction1Behavior(EnemyAI enemyAI) : base(enemyAI) { }

	public override AIState DetermineState()
	{
		if (enemyAI.IsPlayerInRange())
		{
			return AIState.Seeking;
		}
		return AIState.Roaming;
	}

	public override void SeekPlayer()
	{
		// Check if this ship is the "hunter" or the "cutter"
		bool isHunter = Random.Range(0, 2) == 0; // Randomly assign roles (50% chance)

		if (isHunter)
		{
			// Hunter: Chase the player directly
			Vector3 direction = (enemyAI.player.position - enemyAI.transform.position).normalized;
			enemyAI.AvoidCollisions(ref direction);
			enemyAI.RotateTowardTarget(direction, enemyAI.behavior.rotationSpeed);

			float distanceToPlayer = Vector3.Distance(enemyAI.transform.position, enemyAI.player.position);

			if (distanceToPlayer > enemyAI.attackRange)
			{
				enemyAI.rb.velocity = enemyAI.transform.forward * enemyAI.behavior.chaseSpeed * Mathf.Clamp01(distanceToPlayer / enemyAI.attackRange);
			}
			else
			{
				enemyAI.rb.velocity = Vector3.zero; // Stop moving when close to attack range
			}

			if (distanceToPlayer < enemyAI.attackRange && Time.time > enemyAI.lastAttackTime + enemyAI.attackCooldown)
			{
				enemyAI.Attack();
				enemyAI.lastAttackTime = Time.time;
			}
		}
		else
		{
			// Cutter: Move to cut off the player's path
			Vector3 playerDirection = (enemyAI.player.position - enemyAI.transform.position).normalized;
			Vector3 playerVelocity = enemyAI.player.GetComponent<Rigidbody>().velocity;

			// Predict the player's future position
			cutOffPosition = enemyAI.player.position + playerVelocity * 2f; // Adjust multiplier for prediction

			// Move toward the predicted position
			Vector3 cutOffDirection = (cutOffPosition - enemyAI.transform.position).normalized;
			enemyAI.AvoidCollisions(ref cutOffDirection);
			enemyAI.RotateTowardTarget(cutOffDirection, enemyAI.behavior.rotationSpeed);

			enemyAI.rb.velocity = enemyAI.transform.forward * enemyAI.behavior.chaseSpeed;
		}
	}

	public override void Flee()
	{
		// Faction 1 ships do not flee
	}
}


// Faction 2: Organized, role-based behavior
public class Faction2Behavior : FactionBehavior
{
	private enum Role { Tank, FastShip, Attacker }
	private Role currentRole;

	public Faction2Behavior(EnemyAI enemyAI) : base(enemyAI)
	{
		// Assign a random role to the ship
		currentRole = (Role)Random.Range(0, 3); // Randomly assign one of the three roles
	}

	public override AIState DetermineState()
	{
		if (enemyAI.IsPlayerInRange())
		{
			return AIState.Seeking;
		}
		return AIState.Roaming;
	}

	public override void SeekPlayer()
	{
		switch (currentRole)
		{
			case Role.Tank:
				TankBehavior();
				break;
			case Role.FastShip:
				FastShipBehavior();
				break;
			case Role.Attacker:
				AttackerBehavior();
				break;
		}
	}

	private void TankBehavior()
	{
		// Tank: Stay in front of the player and absorb damage
		Vector3 direction = (enemyAI.player.position - enemyAI.transform.position).normalized;
		enemyAI.AvoidCollisions(ref direction);
		enemyAI.RotateTowardTarget(direction, enemyAI.behavior.rotationSpeed);

		// Move toward the player but maintain a safe distance
		float distanceToPlayer = Vector3.Distance(enemyAI.transform.position, enemyAI.player.position);
		if (distanceToPlayer > enemyAI.attackRange * 0.8f) // Stay closer than attack range
		{
			enemyAI.rb.velocity = enemyAI.transform.forward * enemyAI.behavior.chaseSpeed * 0.5f; // Slower movement
		}
		else
		{
			enemyAI.rb.velocity = Vector3.zero; // Stop moving when close enough
		}
	}

	private void FastShipBehavior()
	{
		// Fast Ship: Flank the player and evade attacks
		Vector3 flankDirection = (enemyAI.player.position - enemyAI.transform.position).normalized;
		flankDirection = Quaternion.Euler(0, 90, 0) * flankDirection; // Flank to the side

		enemyAI.AvoidCollisions(ref flankDirection);
		enemyAI.RotateTowardTarget(flankDirection, enemyAI.behavior.rotationSpeed * 1.5f); // Faster rotation

		// Move quickly to flank the player
		enemyAI.rb.velocity = enemyAI.transform.forward * enemyAI.behavior.chaseSpeed * 1.5f; // Faster movement
	}

	private void AttackerBehavior()
	{
		// Attacker: Stay at a distance and deal damage
		Vector3 direction = (enemyAI.player.position - enemyAI.transform.position).normalized;
		enemyAI.AvoidCollisions(ref direction);
		enemyAI.RotateTowardTarget(direction, enemyAI.behavior.rotationSpeed);

		float distanceToPlayer = Vector3.Distance(enemyAI.transform.position, enemyAI.player.position);

		if (distanceToPlayer > enemyAI.attackRange)
		{
			// Move toward the player but maintain attack range
			enemyAI.rb.velocity = enemyAI.transform.forward * enemyAI.behavior.chaseSpeed * 0.8f; // Moderate speed
		}
		else
		{
			enemyAI.rb.velocity = Vector3.zero; // Stop moving when in attack range
		}

		if (distanceToPlayer < enemyAI.attackRange && Time.time > enemyAI.lastAttackTime + enemyAI.attackCooldown)
		{
			enemyAI.Attack();
			enemyAI.lastAttackTime = Time.time;
		}
	}

	public override void Flee()
	{
		// Faction 2 ships do not flee
	}
}

// Pirates: Individualistic, selfish behavior
public class PirateBehavior : FactionBehavior
{
	private float fleeHealthThreshold = 0.3f; // Flee if health is below 30%
	private bool isFleeing = false;

	public PirateBehavior(EnemyAI enemyAI) : base(enemyAI) { }

	public override AIState DetermineState()
	{
		// Check if health is low
		if (enemyAI.health <= enemyAI.maxHealth * fleeHealthThreshold)
		{
			isFleeing = true;
			return AIState.Fleeing;
		}

		if (enemyAI.IsPlayerInRange())
		{
			return AIState.Seeking;
		}
		return AIState.Roaming;
	}

	public override void SeekPlayer()
	{
		// Individualistic behavior: Prioritize self-interest
		Vector3 direction = (enemyAI.player.position - enemyAI.transform.position).normalized;
		enemyAI.AvoidCollisions(ref direction);
		enemyAI.RotateTowardTarget(direction, enemyAI.behavior.rotationSpeed);

		float distanceToPlayer = Vector3.Distance(enemyAI.transform.position, enemyAI.player.position);

		if (distanceToPlayer > enemyAI.attackRange)
		{
			enemyAI.rb.velocity = enemyAI.transform.forward * enemyAI.behavior.chaseSpeed * Mathf.Clamp01(distanceToPlayer / enemyAI.attackRange);
		}
		else
		{
			enemyAI.rb.velocity = Vector3.zero; // Stop moving when close to attack range
		}

		if (distanceToPlayer < enemyAI.attackRange && Time.time > enemyAI.lastAttackTime + enemyAI.attackCooldown)
		{
			enemyAI.Attack();
			enemyAI.lastAttackTime = Time.time;
		}
	}

	public override void Flee()
	{
		// Pirates flee if their health is low
		Vector3 fleeDirection = (enemyAI.transform.position - enemyAI.player.position).normalized;
		enemyAI.AvoidCollisions(ref fleeDirection);
		enemyAI.RotateTowardTarget(fleeDirection, enemyAI.behavior.rotationSpeed * 1.5f); // Faster rotation while fleeing

		enemyAI.rb.velocity = enemyAI.transform.forward * enemyAI.behavior.chaseSpeed * 1.5f; // Faster movement while fleeing

		// Stop fleeing if health is restored or player is out of range
		if (enemyAI.health > enemyAI.maxHealth * fleeHealthThreshold && !enemyAI.IsPlayerInRange())
		{
			isFleeing = false;
		}
	}
}

// Solo: Selfish, unorganized behavior
public class SoloBehavior : FactionBehavior
{
	public SoloBehavior(EnemyAI enemyAI) : base(enemyAI) { }

	public override AIState DetermineState()
	{
		if (enemyAI.IsPlayerInRange())
		{
			return AIState.Seeking;
		}
		return AIState.Roaming;
	}

	public override void SeekPlayer()
	{
		// Direct, unplanned attacks
		Vector3 direction = (enemyAI.player.position - enemyAI.transform.position).normalized;
		enemyAI.AvoidCollisions(ref direction);
		enemyAI.RotateTowardTarget(direction, enemyAI.behavior.rotationSpeed);

		float distanceToPlayer = Vector3.Distance(enemyAI.transform.position, enemyAI.player.position);

		if (distanceToPlayer > enemyAI.attackRange)
		{
			enemyAI.rb.velocity = enemyAI.transform.forward * enemyAI.behavior.chaseSpeed * Mathf.Clamp01(distanceToPlayer / enemyAI.attackRange);
		}
		else
		{
			enemyAI.rb.velocity = Vector3.zero; // Stop moving when close to attack range
		}

		if (distanceToPlayer < enemyAI.attackRange && Time.time > enemyAI.lastAttackTime + enemyAI.attackCooldown)
		{
			enemyAI.Attack();
			enemyAI.lastAttackTime = Time.time;
		}
	}

	public override void Flee()
	{
		// Solo ships flee when health is low or outnumbered
		Vector3 fleeDirection = (enemyAI.transform.position - enemyAI.player.position).normalized;
		enemyAI.rb.velocity = fleeDirection * enemyAI.behavior.chaseSpeed;
	}
}
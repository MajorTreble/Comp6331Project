using UnityEngine;

public class BaseScriptForEnemyAI : MonoBehaviour
{
	public float health = 100f;
	public float speed = 5f;
	public float energy = 100f;
	public float energyRechargeRate = 1f;
	protected float detectionRadius = 20f;
	public float roamRadius = 150f;
	public float roamSpeed = 3f;
	public float avoidanceForce = 5f; // Force to steer away from obstacles and ships

	protected Rigidbody rb;
	protected Transform player;
	protected Vector3 roamTarget;

	void Start()
	{
		Debug.Log("Initializing " + gameObject.name);

		// Initialize Rigidbody
		rb = GetComponent<Rigidbody>();
		if (rb == null)
		{
			Debug.LogError("Rigidbody component is missing on " + gameObject.name);
			enabled = false; // Disable the script if Rigidbody is missing
			return;
		}
		else
		{
			Debug.Log("Rigidbody found on " + gameObject.name);
		}

		// Find the player
		player = GameObject.FindGameObjectWithTag("Player")?.transform;
		if (player == null)
		{
			Debug.LogError("Player not found! Make sure the player has the 'Player' tag.");
			enabled = false; // Disable the script if player is missing
			return;
		}
		else
		{
			Debug.Log("Player found: " + player.name);
		}

		// Set initial roaming target
		SetNewRoamTarget();
		Debug.Log("Roam target set for " + gameObject.name);
	}

	void Update()
	{
		RechargeEnergy();
	}

	public virtual void RechargeEnergy()
	{
		if (energy < 100f)
		{
			energy += energyRechargeRate * Time.deltaTime;
		}
	}

	public void TakeDamage(float damage)
	{
		health -= damage;
		if (health <= 0)
		{
			Die();
		}
	}

	void Die()
	{
		Destroy(gameObject);
	}

	protected bool IsPlayerInRange()
	{
		if (player == null) return false;
		float distanceToPlayer = Vector3.Distance(transform.position, player.position);
		return distanceToPlayer < detectionRadius;
	}

	protected void SetNewRoamTarget()
	{
		// Set a random target within the roam radius
		roamTarget = transform.position + Random.insideUnitSphere * roamRadius;
		roamTarget.y = 0; // Keep the target on the same horizontal plane (optional)
	}

	protected virtual void Roam()
	{
		if (rb == null || roamTarget == null)
		{
			Debug.LogError("Rigidbody or roamTarget is null in Roam method.");
			return;
		}

		// Move toward the roam target
		Vector3 direction = (roamTarget - transform.position).normalized;
		AvoidCollisions(ref direction); // Avoid collisions with obstacles and ships
		rb.velocity = direction * roamSpeed;

		// Rotate toward roam target (optional)
		RotateTowardTarget(direction);

		// If close to the target, set a new target
		if (Vector3.Distance(transform.position, roamTarget) < 5f)
		{
			SetNewRoamTarget();
		}
	}

	protected void RotateTowardTarget(Vector3 direction, float rotationSpeed = 5f)
	{
		if (direction != Vector3.zero) // Ensure we don't get NaN errors
		{
			Quaternion targetRotation = Quaternion.LookRotation(direction);
			transform.rotation = Quaternion.Slerp(
				transform.rotation,
				targetRotation,
				Time.deltaTime * rotationSpeed // Use the provided rotation speed
			);
		}
	}

	protected virtual void AvoidCollisions(ref Vector3 direction)
	{
		// Detect all nearby objects within a certain radius
		Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, detectionRadius);

		Vector3 avoidanceDirection = Vector3.zero;
		int avoidCount = 0;

		foreach (Collider collider in nearbyObjects)
		{
			if (collider.gameObject != gameObject && // Skip self
				(collider.CompareTag("Asteroid") ||
				 collider.CompareTag("PirateShip") ||
				 collider.CompareTag("SoloShip") ||
				 collider.CompareTag("Faction1") ||
				 collider.CompareTag("Faction2")))
			{
				// Calculate direction away from the nearby object
				Vector3 awayFromObject = (transform.position - collider.transform.position).normalized;
				avoidanceDirection += awayFromObject;
				avoidCount++;
			}
		}

		// If there are objects to avoid, adjust the movement direction
		if (avoidCount > 0)
		{
			avoidanceDirection /= avoidCount; // Average the avoidance direction
			direction = (direction + avoidanceDirection * avoidanceForce).normalized;
		}
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, detectionRadius);
	}
}
using UnityEngine;

public class BaseScriptForEnemyAI : MonoBehaviour
{
	public float health = 100f;
	public float speed = 5f;
	public float energy = 100f;
	public float energyRechargeRate = 1f;
	public float detectionRadius = 20f;
	public float roamRadius = 150f;
	public float roamSpeed = 3f;

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

	void RechargeEnergy()
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

	protected void Roam()
	{
		if (rb == null || roamTarget == null)
		{
			Debug.LogError("Rigidbody or roamTarget is null in Roam method.");
			return;
		}

		// Move toward the roam target
		Vector3 direction = (roamTarget - transform.position).normalized;
		rb.velocity = direction * roamSpeed;

		// If close to the target, set a new target
		if (Vector3.Distance(transform.position, roamTarget) < 5f)
		{
			SetNewRoamTarget();
		}
	}

	protected void AvoidObstacles(ref Vector3 direction)
	{
		RaycastHit hit;

		// Check for obstacles in front of the enemy
		if (Physics.Raycast(transform.position, transform.forward, out hit, detectionRadius))
		{
			if (hit.collider.CompareTag("Asteroid")) // Ensure obstacles have the "Obstacle" tag
			{
				// Steer away from the obstacle
				Vector3 avoidanceDirection = Vector3.Reflect(transform.forward, hit.normal).normalized;
				direction = (direction + avoidanceDirection).normalized;
			}
		}
	}

	//protected void Evade()
	//{
		// Check if the player is shooting (you can use a flag or detect projectiles)
	//	if (PlayerIsShooting()) // Implement this method based on your game
	//	{
	//		// Move sideways or backward to evade
		//	Vector3 evadeDirection = Vector3.Cross(transform.forward, Vector3.up).normalized;
	//		rb.velocity += evadeDirection * speed * 0.5f; // Adjust strength as needed
	//	}
	//}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, detectionRadius);
	}
}
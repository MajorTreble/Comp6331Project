using UnityEngine;

public class BaseScriptForEnemyAI : MonoBehaviour
{
	public AIBehavior behavior; // Faction-specific behavior
	public ReputationData playerReputation; // Player's reputation

	public Transform player;
	public Rigidbody rb;
	protected Vector3 roamTarget;

	public float detectionRadius = 25f;
	public float roamRadius = 150f;
	public float roamSpeed = 3f;
	public float avoidanceForce = 5f; // Force to steer away from obstacles and ships

	void Start()
	{
		if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;
		if (rb == null) rb = GetComponent<Rigidbody>();
		SetNewRoamTarget();
	}

	protected bool ShouldAttackPlayer()
	{
		// Check reputation
		if (playerReputation.GetReputation(behavior.faction) > behavior.reputationThreshold)
			return false; // Don't attack if reputation is high

		// Check job conditions
		JobModel currentJob = JobController.Inst.currJob;
		if (currentJob != null)
		{
			// Example: If the job is to defend Faction1 and this is a Faction1 ship, don't attack
			if (currentJob.jobType == JobType.Defend && currentJob.jobTarget == JobTarget.Faction1 && behavior.faction == AIBehavior.Faction.Faction1)
				return false;

			// Example: If the job is to hunt Pirates and this is a Pirate ship, attack
			if (currentJob.jobType == JobType.Hunt && currentJob.jobTarget == JobTarget.Pirate && behavior.faction == AIBehavior.Faction.Pirates)
				return true;
		}

		return false;
	}

	public void RotateTowardTarget(Vector3 direction, float rotationSpeed = 5f)
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
		AvoidCollisions(ref direction);
		rb.velocity = direction * roamSpeed;

		// Rotate toward roam target
		RotateTowardTarget(direction);

		// If close to the target, set a new one
		if (Vector3.Distance(transform.position, roamTarget) < 5f)
		{
			SetNewRoamTarget();
		}
	}

	public void AvoidCollisions(ref Vector3 direction)
	{
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
}
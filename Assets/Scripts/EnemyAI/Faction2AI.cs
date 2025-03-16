using UnityEngine;

public class Faction2AI : BaseScriptForEnemyAI
{
	public enum Role { Tank, Evader, Attacker }
	public Role role = Role.Tank;
	private Transform formationLeader;

	[Header("Combat Settings")]
	public float attackCooldown = 2f;
	private float lastAttackTime;
	public GameObject laserPrefab;
	public Transform firePoint;

	[Header("Formation Settings")]
	public float separationDistance = 5f;
	public float separationForce = 2f;
	public float formationInfluence = 0.7f;
	public float rotationSpeed = 8f;

	public float formationSpacing = 5f;
	public float frontOffset = 10f; // How far ahead of the player the tank stays

	private enum AIState { Roaming, Seeking }
	private AIState currentState = AIState.Roaming;

	public void SetFormationLeader(Transform leader)
	{
		formationLeader = leader;
	}

	void FixedUpdate()
	{
		if (player == null || rb == null) return;

		if (IsPlayerInRange())
		{
			SeekPlayer();
		}
		else if (formationLeader != null)
		{
			FollowFormationLeader();
		}
		else
		{
			Roam(); // Wander when idle
		}
	}


	void FollowFormationLeader()
	{
		if (formationLeader == null) return;

		Vector3 targetPosition = GetFormationPosition();
		Vector3 direction = (targetPosition - transform.position).normalized;
		float distanceToLeader = Vector3.Distance(transform.position, targetPosition);

		// Stop moving if too close to avoid stacking
		if (distanceToLeader > 3f) // Adjust threshold as needed
		{
			rb.velocity = direction * roamSpeed;
			RotateTowardTarget(direction, rotationSpeed);
		}
		else
		{
			rb.velocity = Vector3.zero;
		}
	}



	private Vector3 GetFormationPosition()
	{
		if (formationLeader == null) return transform.position;

		Vector3 leaderForward = formationLeader.forward;
		Vector3 leaderRight = formationLeader.right;
		float offsetMultiplier = 1.5f; // Adjust for better spacing

		return role switch
		{
			Role.Tank => formationLeader.position + leaderForward * 12f,
			Role.Evader => formationLeader.position + (leaderRight * 7f * offsetMultiplier) - leaderForward * 12f,
			Role.Attacker => formationLeader.position + (-leaderRight * 7f * offsetMultiplier) - leaderForward * 12f,
			_ => formationLeader.position
		};
	}




	void SeekPlayer()
	{
		Vector3 moveDirection = (player.position - transform.position).normalized;
		float distanceToPlayer = Vector3.Distance(transform.position, player.position);

		if (distanceToPlayer < 7f) // Prevent stacking
		{
			// Move away slightly to create space
			moveDirection = -moveDirection * 1.5f;
		}
		else
		{
			AvoidCollisions(ref moveDirection);
		}

		rb.velocity = moveDirection * speed;
		RotateTowardTarget(moveDirection, rotationSpeed);
	}




	Vector3 GetFormationDirection()
	{
		Collider[] ships = Physics.OverlapSphere(transform.position, detectionRadius);
		Vector3 avgPosition = Vector3.zero;
		int count = 0;

		foreach (Collider ship in ships)
		{
			if (ship.CompareTag("Faction2") && ship.gameObject != gameObject)
			{
				avgPosition += ship.transform.position;
				count++;
			}
		}
		return count > 0 ? (avgPosition / count - transform.position).normalized : Vector3.zero;
	}

	Vector3 GetSeparationDirection()
	{
		Collider[] neighbors = Physics.OverlapSphere(transform.position, separationDistance);
		Vector3 separation = Vector3.zero;
		int count = 0;

		foreach (Collider ship in neighbors)
		{
			if ((ship.CompareTag("Faction1") || ship.CompareTag("Faction2")) && ship.gameObject != gameObject)
			{
				Vector3 awayFromShip = (transform.position - ship.transform.position).normalized;
				separation += awayFromShip;
				count++;
			}
		}

		return count > 0 ? separation / count : separation;
	}


	void Attack()
	{
		GameObject laser = Instantiate(laserPrefab, firePoint.position, firePoint.rotation);
		laser.GetComponent<Rigidbody>().velocity = firePoint.forward * 20f;
	}

	void OnDrawGizmosSelected()
	{
		if (player == null) return;

		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(GetFormationPosition(), 1f);
	}
}
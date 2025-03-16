using UnityEngine;

public class Faction1AI : BaseScriptForEnemyAI
{
	public enum Role { Chaser, Flanker, Blocker }
	public Role role = Role.Chaser;

	[Header("Combat Settings")]
	public float flankDistance = 10f;
	public float pullPushForce = 5f;
	public float pullPushCooldown = 5f;
	private float lastPullPushTime;

	[Header("Formation Settings")]
	public float separationDistance = 5f;
	public float separationForce = 2f;
	public float rotationSpeed = 8f;
	private Transform formationLeader;

	private enum AIState { Roaming, Seeking }
	private AIState currentState = AIState.Roaming;
	public float formationSpacing = 5f; // Space between ships

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
			Role.Chaser => formationLeader.position - leaderForward * 12f,
			Role.Flanker => formationLeader.position + (leaderRight * 7f * offsetMultiplier) - leaderForward * 12f,
			Role.Blocker => formationLeader.position + (-leaderRight * 7f * offsetMultiplier) - leaderForward * 12f,
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


	void UsePullPushWeapon()
	{
		Vector3 direction = (player.position - transform.position).normalized;
		player.GetComponent<Rigidbody>().AddForce(direction * pullPushForce, ForceMode.Impulse);
	}

	void OnDrawGizmosSelected()
	{
		if (player == null) return;

		Gizmos.color = Color.cyan;
		Gizmos.DrawSphere(GetFormationPosition(), 1f);
	}
}
using UnityEngine;

using Model;
using Model.AI;

public class Faction2Script : AIShip
{
	private enum Role { Tank, FastShip, Attacker }
	private Role currentRole;

	public override void Start()
	{
		base.Start();

		//currentState = AIState.Roaming;
		
		if (behavior == null)
		{
			behavior = Resources.Load<AIBehavior>("ScriptableObjects/Faction2Behavior");
		}

		// Assign a random role to the ship
		currentRole = (Role)Random.Range(0, 3); // Randomly assign one of the three roles

		SetNewRoamTarget();
	}

	public override void UpdateSeek()
	{
		//switch (currentRole)
		//{
		//	case Role.Tank:
		//	TankBehavior();
		//break;
		//case Role.FastShip:
		//FastShipBehavior();
		//break;
		//	case Role.Attacker:
		//	AttackerBehavior();
		//break;
		//}


		// If mission benefits our faction, fight harder
		if (AIHelper.IsMissionAlly(faction))
		{
			switch (currentRole)
			{
				case Role.Tank:
					behavior.chaseSpeed *= 0.7f;
					behavior.attackRange *= 0.8f;
					break;
				case Role.Attacker:
					behavior.attackCooldown *= 0.5f;
					break;
			}
		}

		base.UpdateSeek();

	}

	private void TankBehavior()
	{
		Transform player = GameObject.FindGameObjectWithTag("Player").transform;

		// Tank: Stay in front of the player and absorb damage
		Vector3 direction = (player.position - transform.position).normalized;
		Vector3 avoidance = ComputeObstacleAvoidance(direction);
		if (avoidance != Vector3.zero)
		{
			direction = (direction + avoidance).normalized;
		}
		RotateTowardTarget(direction, behavior.rotationSpeed);

		// Move toward the player but maintain a safe distance
		float distanceToPlayer = Vector3.Distance(transform.position, player.position);
		if (distanceToPlayer > behavior.attackRange * 0.8f) // Stay closer than attack range
		{
			rb.velocity = transform.forward * behavior.chaseSpeed * 0.5f; // Slower movement
		}
		else
		{
			rb.velocity = Vector3.zero; // Stop moving when close enough
		}
	}

	private void FastShipBehavior()
	{
		Transform player = GameObject.FindGameObjectWithTag("Player").transform;

		// Fast Ship: Flank the player and evade attacks
		Vector3 flankDirection = (player.position - transform.position).normalized;
		flankDirection = Quaternion.Euler(0, 90, 0) * flankDirection; // Flank to the side

		Vector3 avoidance = ComputeObstacleAvoidance(flankDirection);
		if (avoidance != Vector3.zero)
		{
			flankDirection = (flankDirection + avoidance).normalized;
		}
		RotateTowardTarget(flankDirection, behavior.rotationSpeed * 1.5f); // Faster rotation

		// Move quickly to flank the player
		rb.velocity = transform.forward * behavior.chaseSpeed * 1.5f; // Faster movement
	}

	private void AttackerBehavior()
	{
		Transform player = GameObject.FindGameObjectWithTag("Player").transform;

		// Attacker: Stay at a distance and deal damage
		Vector3 direction = (player.position - transform.position).normalized;
		Vector3 avoidance = ComputeObstacleAvoidance(direction);
		if (avoidance != Vector3.zero)
		{
			direction = (direction + avoidance).normalized;
		}
		RotateTowardTarget(direction, behavior.rotationSpeed);

		float distanceToPlayer = Vector3.Distance(transform.position, player.position);

		if (distanceToPlayer > behavior.attackRange)
		{
			// Move toward the player but maintain attack range
			rb.velocity = transform.forward * behavior.chaseSpeed * 0.8f; // Moderate speed
		}
		else
		{
			rb.velocity = Vector3.zero; // Stop moving when in attack range
		}

		if (distanceToPlayer < behavior.attackRange && Time.time > lastAttackTime + attackCooldown)
		{
			//Attack();
			lastAttackTime = Time.time;
		}
	}

}
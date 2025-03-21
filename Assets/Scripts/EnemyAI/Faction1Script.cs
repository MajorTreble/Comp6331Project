using UnityEngine;

public class Faction1Script : BaseScriptForEnemyAI
{
	private Vector3 cutOffPosition; // Position to cut off the player's path

	public override void  Start()
	{
		base.Start();

		//currentState = AIState.Roaming;
		
		if (behavior == null)
		{
			behavior = Resources.Load<AIBehavior>("ScriptableObjects/Faction1Behavior");
		}

		SetNewRoamTarget();
	}

	void FixedUpdate()
	{
		if (player == null || rb == null) return;

		// Update state based on player proximity
		currentState = IsPlayerInRange() ? AIState.Seeking : AIState.Roaming;
		if (IsPlayerInRange())
		{
			SeekPlayer();
		}
		else
		{
			Roam();
		}
	}

	public override void SeekPlayer()
	{
		// Check if this ship is the "hunter" or the "cutter"
		bool isHunter = Random.Range(0, 2) == 0; // Randomly assign roles (50% chance)

		if (isHunter)
		{
			// Hunter: Chase the player directly
			base.SeekPlayer(); // Use the base SeekPlayer() method
		}
		else
		{
			// Cutter: Move to cut off the player's path
			Vector3 playerDirection = (player.position - transform.position).normalized;
			Vector3 playerVelocity = player.GetComponent<Rigidbody>().velocity;

			// Predict the player's future position
			cutOffPosition = player.position + playerVelocity * 2f; // Adjust multiplier for prediction

			// Move toward the predicted position
			Vector3 cutOffDirection = (cutOffPosition - transform.position).normalized;
			Vector3 avoidance = ComputeObstacleAvoidance(cutOffDirection);
			if (avoidance != Vector3.zero)
			{
				cutOffDirection = (cutOffDirection + avoidance).normalized;
			}
			RotateTowardTarget(cutOffDirection, behavior.rotationSpeed);

			rb.velocity = transform.forward * behavior.chaseSpeed;
		}
	}


	protected override void AttackEnemiesNearPlayer()
	{
		if (player == null || behavior == null) return;

		// Prioritize attacking enemies targeting the player
		Collider[] nearbyEnemies = Physics.OverlapSphere(
			player.position,
			behavior.allyAssistRange,
			LayerMask.GetMask("Enemy")
		);

		foreach (Collider enemy in nearbyEnemies)
		{
			BaseScriptForEnemyAI enemyAI = enemy.GetComponent<BaseScriptForEnemyAI>();
			if (enemyAI != null && enemyAI.ShouldAttackPlayer())
			{
				// Rotate and attack the hostile enemy
				Vector3 targetDirection = (enemy.transform.position - transform.position).normalized;
				RotateTowardTarget(targetDirection, behavior.rotationSpeed);

				if (Time.time > lastAttackTime + behavior.attackCooldown)
				{
					Attack();
				}
			}
		}
	}


}
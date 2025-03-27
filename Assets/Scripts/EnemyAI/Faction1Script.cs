using UnityEngine;

using Model.AI;
using Controller;

public class Faction1Script : AIShip
{
	private float temporaryChaseSpeed;
	private float temporaryAttackCooldown;

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

	public override void UpdateSeek()
	{
		// Mission-specific behavior
		if (IsMissionTarget() && JobController.Inst.currJob.jobType == JobType.Hunt)
		{
			// Become more aggressive when being hunted
			float tempSpeed = behavior.chaseSpeed * 1.3f;
			rb.AddForce(transform.forward * tempSpeed * Time.deltaTime,
					   ForceMode.Acceleration);
		}
		else
		{
			base.UpdateSeek();
		}

		// Reset modified values after use
		//if (IsMissionTarget())
		//{
			//behavior.chaseSpeed = originalChaseSpeed;
			//behavior.attackCooldown = originalAttackCooldown;
		//}
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
			AIShip enemyAI = enemy.GetComponent<AIShip>();
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
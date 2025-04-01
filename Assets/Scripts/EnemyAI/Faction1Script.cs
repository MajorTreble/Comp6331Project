using UnityEngine;

using Model;
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
	}

	public override void UpdateSeek()
	{
		// Mission-specific behavior
		if (AIHelper.IsMissionTarget(target) && JobController.Inst.currJob.jobType == JobType.Hunt)
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
		Transform player = GameObject.FindGameObjectWithTag("Player").transform;

		if (player == null || behavior == null) return;

		// Prioritize attacking enemies targeting the player
		Collider[] nearbyEnemies = Physics.OverlapSphere(
			player.position,
			behavior.allyAssistRange,
			LayerMask.GetMask("Enemy")
		);

		foreach (Collider enemy in nearbyEnemies)
		{
			
			Job job = JobController.Inst.currJob;
			AIShip enemyAI = enemy.GetComponent<AIShip>();
			if (enemyAI != null && AIHelper.ShouldAttackPlayer(enemyAI, player.gameObject, job, enemyAI.faction))
			{

				if (Time.time > lastAttackTime + behavior.attackCooldown)
				{
					Attack();
				}
			}
		}
	}


}
using UnityEngine;
using UnityEngine.UI;

using Model.AI;
using Controller;

public class PirateShipScript : AIShip
{
	private float originalDetectionRadius;
	private float fleeHealthThreshold = 0.3f; // Flee if health is below 30%
	//private bool isFleeing = false;

	public override void Start()
	{
		base.Start();
		originalDetectionRadius = behavior.detectionRadius;
	}


	protected override void UpdateDecision()
	{
		base.UpdateDecision();

		// Pirates become more aggressive during mining missions
		if (JobController.Inst.currJob?.jobType == JobType.Mine)
		{
			detectionRadius *= 1.2f;
		}

		// Update state based on health and player proximity
		if (health <= maxHealth * fleeHealthThreshold)
		{
			requestedState = AIState.Flee;
		}
	}

}
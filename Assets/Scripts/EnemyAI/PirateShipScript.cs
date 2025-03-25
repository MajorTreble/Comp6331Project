using UnityEngine;
using UnityEngine.UI;

using Model.AI;

public class PirateShipScript : AIShip
{
	private float fleeHealthThreshold = 0.3f; // Flee if health is below 30%
	//private bool isFleeing = false;

	protected override void UpdateDecision()
	{
		base.UpdateDecision();

		// Update state based on health and player proximity
		if (health <= maxHealth * fleeHealthThreshold)
		{
			requestState = AIState.Flee;
		}
	}

}
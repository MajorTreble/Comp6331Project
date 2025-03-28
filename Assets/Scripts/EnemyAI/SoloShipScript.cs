using UnityEngine;

using Model.AI;

public class SoloShipScript : AIShip
{
	[Header("Solo Behavior")]
	[SerializeField] private float randomDecisionInterval = 3f;
	[SerializeField] private float aggressionProbability = 0.4f;
	private float nextDecisionTime;

	//protected override void UpdateDecision()
	//{
		//base.UpdateDecision();

		// just some code to Make random decisions at intervals, if not needed then remove
		
		//if (Time.time > nextDecisionTime)
		//{
			//nextDecisionTime = Time.time + randomDecisionInterval;

			// Randomly choose between attack or flee
			//if (Random.value < aggressionProbability && ShouldAttackPlayer())
			//{
				//requestState = AIState.Seek;
			//}
			//else
			//{
				//requestState = ShouldAllyWithPlayer() ? AIState.AllyAssist : AIState.Flee;
			//}
		//}

		// Solo ships don't coordinate - override any group logic
		//if (currentState == AIState.AllyAssist && !ShouldAllyWithPlayer())
		//{
			//requestState = AIState.Roam;
		//}
	//}

}
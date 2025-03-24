using UnityEngine;

public class SoloShipScript : BaseScriptForEnemyAI
{
	//private AIState currentState = AIState.Roaming;
	
	public override void Start()
	{
		base.Start();

		//currentState = AIState.Roaming;
		
		if (behavior == null)
		{
			behavior = Resources.Load<AIBehavior>("ScriptableObjects/SoloShipBehavior");
		}

		SetNewRoamTarget();
	}

	void FixedUpdate()
	{
		if (player == null || rb == null) return;

		// Update state based on player proximity
		currentState = IsPlayerInRange() ? AIState.Seeking : AIState.Roaming;

		// Execute behavior based on state
		if (IsPlayerInRange())
		{
			SeekPlayer();
		}
		else
		{
			Roam();
		}
	}

}
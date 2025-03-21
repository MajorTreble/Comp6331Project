using UnityEngine;
using UnityEngine.UI;

public class PirateShipScript : BaseScriptForEnemyAI
{
	//private AIState currentState = AIState.Roaming;
	public float health = 100f; // Current health
	public float maxHealth = 100f;
	

	public Slider healthSlider; // Reference to the health slider

	private float fleeHealthThreshold = 0.3f; // Flee if health is below 30%
	private bool isFleeing = false;

	void Start()
	{
		base.Start();

		//currentState = AIState.Roaming;
		
		if (behavior == null)
		{
			behavior = Resources.Load<AIBehavior>("ScriptableObjects/PiratesBehavior");
		}

		SetNewRoamTarget();
	}

	void Update()
	{
		//// Slider COde is not working now!! FIX IT LATER
		if (healthSlider != null)
		{
			healthSlider.value = health / maxHealth; // Normalize health to a value between 0 and 1
		}

		// Example: Reduce health for testing (remove this in the final game)
		//if (Input.GetKeyDown(KeyCode.Space))
		//{
		//	TakeDamage(10f); // Simulate taking damage
		//}
	}

	public void TakeDamage(float damage)
	{
		health -= damage;
		health = Mathf.Clamp(health, 0, maxHealth); // Ensure health doesn't go below 0 or above maxHealth
	}


	void FixedUpdate()
	{
		if (player == null || rb == null) return;

		// Update state based on health and player proximity
		if (health <= maxHealth * fleeHealthThreshold)
		{
			Flee();
		}
		else if (IsPlayerInRange())
		{
			SeekPlayer();
		}
		else
		{
			Roam();
		}

	}


	void Flee()
	{
		Vector3 fleeDirection = (transform.position - player.position).normalized;
		Vector3 avoidance = ComputeObstacleAvoidance(fleeDirection);
		if (avoidance != Vector3.zero)
		{
			fleeDirection = (fleeDirection + avoidance).normalized;
		}

		RotateTowardTarget(fleeDirection, behavior.rotationSpeed * 1.5f); // Faster rotation while fleeing
		rb.velocity = transform.forward * behavior.chaseSpeed * 1.5f; // Faster movement while fleeing
	}

}
using Model.AI;
using UnityEngine;
using static Model.Faction;

public class EnemyLaser : MonoBehaviour
{
	public float speed = 20f;
	public float damage = 10f;
	private int playerHitCount = 0;
	public GameObject shooter;
	public FactionType shooterFaction;

	void Start()
	{
		Destroy(gameObject, 3f);
	}

	void FixedUpdate()
	{
		transform.Translate(Vector3.forward * speed * Time.deltaTime);
	}

	void OnTriggerEnter(Collider other)
	{
		// Ignore collision with self
		if (other.gameObject == shooter) return;

		if (other.CompareTag("Player") || other.CompareTag("SpaceshipComponent"))
		{
			Debug.Log($"Laser from [{shooter?.name}] hit the Player!");
			playerHitCount++;

			Destroy(gameObject); // Destroy the laser on hit
		}

		// Check for AI ship hit
		AIShip targetAI = other.GetComponent<AIShip>();
		if (targetAI != null)
		{
			// Skip friendly fire
			if (targetAI.factionType == shooterFaction) return;

			Debug.Log($"Laser from [{shooter?.name}] hit [{targetAI.name}]");
			targetAI.TakeDamage(shooter);
			Destroy(gameObject);
		}
	}
}
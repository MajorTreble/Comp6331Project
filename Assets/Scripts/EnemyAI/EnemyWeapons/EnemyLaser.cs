using Model.AI;
using UnityEngine;
using static Model.Faction;

using Model;

public class EnemyLaser : MonoBehaviour
{
	public Ship shooter;

	public float speed = 20f;
	public float damage = 10f;
	private int playerHitCount = 0;
	public Faction faction;

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
			if (targetAI.faction == faction) return;

			Debug.Log($"Laser from [{shooter?.name}] hit [{targetAI.name}]");
			targetAI.TakeDamage(damage, shooter);
			Destroy(gameObject);
		}
	}
}
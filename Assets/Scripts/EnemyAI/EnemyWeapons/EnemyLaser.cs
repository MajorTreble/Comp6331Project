using UnityEngine;

public class EnemyLaser : MonoBehaviour
{
	public float speed = 20f;
	public float damage = 10f;
	private int playerHitCount = 0;

	void Start()
	{
		//Debug.Log("Laser spawned at: " + transform.position);

		// Destroy the laser after 3 seconds
		Destroy(gameObject, 3f);
	}

	void FixedUpdate()
	{
		//Debug.Log("Laser moving at speed: " + speed);

		// Move the laser forward
		transform.Translate(Vector3.forward * speed * Time.deltaTime);
	}

	void OnTriggerEnter(Collider other)
	{
		//Debug.Log("Laser collided with: " + other.name);

		if (other.CompareTag("Player") || other.CompareTag("SpaceshipComponent"))
		{
			//Debug.Log("Laser hit player!");
			playerHitCount++;

			// Damage the player
			//PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
			//if (playerHealth != null)
			//{
			//	playerHealth.TakeDamage(damage);
			//}

			Destroy(gameObject); // Destroy the laser on hit
		}
	}
}
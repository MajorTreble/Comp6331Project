using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLaser : MonoBehaviour
{
	public float speed = 20f;
	public float damage = 10f;
	int PlayerHitCount = 0;

	void Start()
	{
		// Destroy the laser after 3 seconds
		Destroy(gameObject, 3f);
	}

	void FixedUpdate()
	{
		// Move the laser forward
		transform.Translate(Vector3.forward * speed * Time.deltaTime);
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			
			// Damage the player
			//other.GetComponent<PlayerHealth>().TakeDamage(damage);
			Debug.Log("Player Hit");
			PlayerHitCount++;
			Destroy(gameObject); // Destroy the laser on hit
		}
	}
}

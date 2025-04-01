using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model;
using Model.AI;

public class LaserProjectile : MonoBehaviour
{
	public int damage = 10;
	public float lifetime = 3f;
	public GameObject shooter;

	void Start()
	{
		Destroy(gameObject, lifetime);
	}

	void OnCollisionEnter(Collision collision)
	{
		Debug.Log($"[Laser] Hit: {collision.gameObject.name}");

		if (shooter == null)
		{
			Debug.LogWarning("Shooter is null on laser!");
			return;
		}

		if (collision.gameObject == shooter)
		{
			Debug.Log("Laser hit its own shooter. Ignoring.");
			return;
		}

		AIShip aiShip = collision.gameObject.GetComponent<AIShip>();
		if (aiShip != null)
		{
			aiShip.TakeDamage(shooter);
		}

		if (collision.gameObject.CompareTag("Player"))
		{
			Debug.Log("Laser hit the player!");
		}

		Destroy(gameObject);
	}



}

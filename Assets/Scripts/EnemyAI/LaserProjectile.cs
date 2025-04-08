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
	public float speed = 30f;
	Rigidbody rb;
	public GameObject impactEffect;
	public Model.Faction faction;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		rb.velocity = transform.forward * speed;

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
			if (aiShip.faction == this.faction)
			{
				Debug.Log("Laser hit ally — ignoring.");
				return;
			}

			aiShip.TakeDamage(shooter);
		}


		PlayerShip player = collision.gameObject.GetComponent<PlayerShip>();
		if (player != null)
		{
			player.TakeDamage(damage);
			Debug.Log("Laser hit the player!");

		}

		if (impactEffect != null)
		{
			Instantiate(impactEffect, transform.position, Quaternion.identity);
		}

		Destroy(gameObject);
	}



}

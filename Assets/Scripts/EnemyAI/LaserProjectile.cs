using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model;

public class LaserProjectile : MonoBehaviour
{
	public int damage = 10;
	public float lifetime = 3f;

	void Start()
	{
		Destroy(gameObject, lifetime);
	}

	void OnCollisionEnter(Collision collision)
	{
		Ship ship = collision.gameObject.GetComponent<Ship>();
		if (ship != null)
		{
			ship.TakeDamage(damage);
		}
		Destroy(gameObject);
	}
}

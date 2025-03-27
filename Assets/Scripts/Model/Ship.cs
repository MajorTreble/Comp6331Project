using UnityEngine;

namespace Model
{

	public class Ship : MonoBehaviour
	{
		public int ammo = 0;
		public float health = 100.0f;
		public float maxHealth = 100.0f;
		public float shields = 100.0f;
		public float maxShields = 100.0f;


		// Asma Added this method for allowing ships to take Damage
		public virtual void TakeDamage(float damageAmount)
		{
			// First reduce shields
			if (shields > 0)
			{
				shields -= damageAmount;
				if (shields < 0)
				{
					// If damage exceeds shields, carry over to health
					health += shields; // shields is negative here
					shields = 0;
				}
			}
			else
			{
				// Direct health damage when shields are down
				health -= damageAmount;
			}

			// Clamp values
			health = Mathf.Clamp(health, 0, maxHealth);
			shields = Mathf.Clamp(shields, 0, maxShields);

			Debug.Log($"{gameObject.name} took {damageAmount} damage. Health: {health}, Shields: {shields}");

			// Optional: Add destruction logic when health reaches 0
			if (health <= 0)
			{
				Destroy(gameObject);
			}
		}


	}

}
using UnityEngine;

using Controller;
using System;
namespace Model
{

	public class Ship : MonoBehaviour
	{
		public int ammo = 0;
		public float health = 100.0f;
		public float maxHealth = 100.0f;
		public float shields = 100.0f;
		public float shieldRegen = 10;
		public float maxShields = 100.0f;

		public virtual void SetStats()
		{
			health = maxHealth;
			shields = maxShields;
		}

		public virtual void ShieldRecover()
		{
			shields += shieldRegen;
			shields = Mathf.Clamp(shields, 0, maxShields);
		}

		public virtual bool TakeDamage(float _dmg)
		{
			health -= _dmg;

			return CheckDestroyed();
		}

		public bool CheckDestroyed()
		{
			if(health > 0)
				return false;

				
			JobController.Inst.OnObjDestroyed(transform.tag);

			this.gameObject.SetActive(false);

			return true;	
		}

		public virtual void Leave()
		{
			JobController.Inst.OnObjLeave(transform.tag);
			this.gameObject.SetActive(false);
		}
	}	
}
using System.Runtime.InteropServices;
using Controller;
using Model.Environment;
using UnityEngine;

namespace Model
{

	public class PlayerShip : Ship
	{
		public GameObject laserPrefab = null;
		public GameObject laser = null;

		public Transform weapon_1;

		
		public float laserDist;
		float laserDmg = 100;

      	public void Awake()
		{
			GetWeapon();
		}

        public void Update()
        {
            ShieldRecover();
        }

        void GetWeapon()
		{
			weapon_1 = Utils.FindChildByName(this.transform, "Weapon1");

			if(weapon_1 == null)
			{
				Debug.Log("weapon not found, again");
				Invoke("GetWeapon", 0.3f);
				return;
			}

			SpawnLaser();
		}

        public void SpawnLaser()
		{
			laserDist = laserDist == 0 ? 60 : laserDist;

			laser = GameObject.Instantiate(laserPrefab, weapon_1.transform);
			laser.transform.localScale = new Vector3(0.3f, 0.3f, laserDist);
			laser.transform.rotation = Quaternion.Euler(0,0,0);
			Vector3 laserPos = Vector3.zero;
			laserPos.z += (float)(laserDist/2) ;
			laser.transform.localPosition = laserPos;
		}

		public void ShowLaser(bool isVisible)
		{
			if(laser == null) return;
			//isVisible = true; //For tests
			laser.SetActive(isVisible);
			
			if(isVisible)
			{
				RaycastHit hit;
				Vector3 direction = laser.transform.forward;

				if (Physics.Raycast(weapon_1.transform.position, direction, out hit, laserDist))
				{
					if(hit.transform.CompareTag(this.transform.tag)) return;

					Ship ship = hit.transform.gameObject.GetComponent<Ship>();
					if(ship != null)
					{
						if(ship.TakeDamage(laserDmg*Time.deltaTime))
						{
							Debug.Log("DESTROYED BY LASER");
						}
					}

					BreakableAsteroid ba = hit.transform.gameObject.GetComponent<BreakableAsteroid>();
					if(ba != null)
					{
						if(ba.ReceiveDamage(laserDmg*Time.deltaTime))
						{
							Debug.Log("DESTROYED BY LASER");
						}
					}
				}

				Debug.DrawRay(weapon_1.transform.position, direction * laserDist, Color.yellow);
			}
		}

		public override bool TakeDamage(float damageAmount)
		{
			bool destroyed = base.TakeDamage(damageAmount); // Use base ship behavior

			// Just to check if player ship is getting hit
			Debug.Log("Player took damage!");

			return destroyed;
		}

		public override void Leave()
		{
			base.Leave(); 
			JobController.Inst.LeaveMap();
		}
	}
}

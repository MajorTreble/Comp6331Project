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
			laser = GameObject.Instantiate(laserPrefab, weapon_1.transform);
			laser.transform.localScale = new Vector3(0.3f, 0.02f, laserDist);
			laser.transform.rotation = Quaternion.Euler(0,90,0);
			Vector3 laserPos = Vector3.zero;
			laserPos.z += (float)(laserDist/2) ;
			laser.transform.localPosition = laserPos;
		}

		public void ShowLaser(bool isVisible)
		{
			if(laser == null) return;
			isVisible = true;
			laser.SetActive(isVisible);
			
			//Is here the best place to put this?!
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
						if(ship.ReceiveDamage(laserDmg*Time.deltaTime))
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
	}
}
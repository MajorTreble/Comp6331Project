using UnityEngine;

namespace Model
{

	public class PlayerShip : Ship
	{
		public GameObject laser = null;

		public Vector3 laserPosition = new Vector3(1.0f, -1.0f, 20.0f);
		public Quaternion laserRotation;

		public void SpawnLaser(GameObject preFab)
		{
			laser = GameObject.Instantiate(preFab, laserPosition, laserRotation);
			laser.transform.parent = gameObject.transform;
		}

		public void ShowLaser(bool isVisible)
		{
			laser.SetActive(isVisible);
		}
	}

}      
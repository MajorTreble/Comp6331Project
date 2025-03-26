using UnityEngine;

using Model;
using Model.AI;

namespace Manager
{

	public class SpawnParams
	{
		public GameObject prefab = null;
		public Vector3 position = Vector3.zero;
		public Quaternion rotation = Quaternion.identity;

		public ShipType shipType = ShipType.Light;
		public Faction faction = null;

		public SpawnParams() { }

		public SpawnParams(GameObject prefab, Vector3 position, Quaternion rotation)
		{
			this.prefab = prefab;
			this.position = position;
			this.rotation = rotation;
		}

		public GameObject Prefab
		{
			get => prefab != null ? prefab : faction.prefabs[(int)shipType];
		}

		public void Setup(GameObject ship)
		{
			if (ship == null)
			{
				return;
			}

			if (faction != null)
			{
				AIShip aiShip = ship.GetComponent<AIShip>();
				if (aiShip != null)
				{
					aiShip.behavior = faction.behavior;
				}
			}
		}
	}

}
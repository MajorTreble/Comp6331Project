using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{

	[CreateAssetMenu(fileName = "SpaceObjectGroup", menuName = "ScriptableObjects/SpaceObjectGroup", order = 1)]
	public class SpaceObjectGroup : ScriptableObject
	{
		public Vector3 position = Vector3.zero;
		public Quaternion rotation = Quaternion.identity;
		public float radius = 10.0f;

		public GameObject prefab = null;

		public Vector3 SpawnPosition()
		{
			Vector3 randomPosition = Random.insideUnitSphere * radius;
			randomPosition.y = 0.0f;
			randomPosition += position;

			return randomPosition;
		}
	}

}
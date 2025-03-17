using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controller
{

	public class SpaceEnvironmentController : MonoBehaviour
	{
		[SerializeField]
		public GameObject[] spaceObjectPrefabs;
		public Transform player; 
		public float spawnRadius = 100f;
		public int maxObjects = 30;
		public float despawnDistance = 200f;

		private List<GameObject> activeObjects = new List<GameObject>();

		void Start()
		{
			for (int i = 0; i < maxObjects; i++)
			{
				SpawnObject();
			}
		}

		void Update()
		{
			RecycleOrDespawnObjects();
		}

		void SpawnObject()
		{
			if (activeObjects.Count >= maxObjects) return;

			Vector3 spawnPos = player.position + Random.onUnitSphere * spawnRadius;
			GameObject obj = Instantiate(spaceObjectPrefabs[Random.Range(0, spaceObjectPrefabs.Length)], spawnPos, Random.rotation);
			activeObjects.Add(obj);
		}

		void RecycleOrDespawnObjects()
		{
			for (int i = activeObjects.Count - 1; i >= 0; i--)
			{
				if (Vector3.Distance(player.position, activeObjects[i].transform.position) > despawnDistance)
				{
					RecycleObject(activeObjects[i]);
				}
			}
		}

		void RecycleObject(GameObject obj)
		{
			obj.transform.position = player.position + Random.onUnitSphere * spawnRadius;
			obj.transform.rotation = Random.rotation;
		}
	}

}
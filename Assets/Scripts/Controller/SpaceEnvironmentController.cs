using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controller
{
    public class SpaceEnvironmentController : MonoBehaviour
    {
        [System.Serializable]
        public class SpaceObject
        {
            public GameObject prefab;
            public int weight; // Higher value = More frequent spawning
        }

        [SerializeField]
        public SpaceObject[] spaceObjects; // List of objects with their spawn weight

        public Transform player;
        public float spawnRadius = 100f;
        public int maxObjects = 30;
        public float despawnDistance = 200f;
        public float cameraSpawnDistance = 50f;

        private List<GameObject> activeObjects = new List<GameObject>();
        private int totalWeight;

        void Start()
        {
            if (player == null)
            {
                GameObject foundPlayer = GameObject.FindWithTag("Player");
                if (foundPlayer != null)
                {
                    player = foundPlayer.transform;
                    //Debug.Log("[SpaceEnvironmentController] Player found by tag.");
                }
                else
                {
                    Debug.LogWarning("[SpaceEnvironmentController] Player not found! Using camera for spawning.");
                }
            }

            CalculateTotalWeight();
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

            GameObject selectedPrefab = GetWeightedRandomObject(); // Select object based on weight

            Vector3 spawnPos;

            if (player != null)
            {
                spawnPos = player.position + Random.onUnitSphere * spawnRadius;
            }
            else
            {
                Camera mainCam = Camera.main;
                if (mainCam == null)
                {
                    Debug.LogError("Main Camera not found! Cannot spawn objects.");
                    return;
                }

                Vector3 forwardDirection = mainCam.transform.forward;
                Vector3 randomOffset = Random.insideUnitSphere * spawnRadius * 0.5f;

                spawnPos = mainCam.transform.position + forwardDirection * cameraSpawnDistance + randomOffset;
            }

            GameObject org = GameObject.Find("Org_"+selectedPrefab.name);
            if(org == null)
            {
                org = new GameObject("Org_"+selectedPrefab.name);
            }

            GameObject obj = Instantiate(selectedPrefab, spawnPos, Random.rotation, org.transform);
            activeObjects.Add(obj);
        }

        void RecycleOrDespawnObjects()
        {
            Vector3 referencePoint = player != null ? player.position : Camera.main.transform.position;

            for (int i = activeObjects.Count - 1; i >= 0; i--)
            {
                if(activeObjects[i] == null) continue;
                
                if (Vector3.Distance(referencePoint, activeObjects[i].transform.position) > despawnDistance)
                {
                    RecycleObject(activeObjects[i]);
                }
            }
        }

        void RecycleObject(GameObject obj)
        {
            Vector3 newPos;
            if (player != null)
            {
                // Recycle around the player
                newPos = player.position + Random.onUnitSphere * spawnRadius;
            }
            else
            {
                // Recycle in front of the main camera
                Camera mainCam = Camera.main;
                if (mainCam == null) return;

                newPos = mainCam.transform.position + mainCam.transform.forward * cameraSpawnDistance;
            }

            obj.transform.position = newPos;
            obj.transform.rotation = Random.rotation;
        }

        void CalculateTotalWeight()
        {
            totalWeight = 0;
            foreach (var obj in spaceObjects)
            {
                totalWeight += obj.weight;
            }
        }

        // Select an object based on its weight (higher weight = higher chance)
        GameObject GetWeightedRandomObject()
        {
            int randomValue = Random.Range(0, totalWeight);
            int cumulativeWeight = 0;

            foreach (var obj in spaceObjects)
            {
                cumulativeWeight += obj.weight;
                if (randomValue < cumulativeWeight)
                {
                    return obj.prefab;
                }
            }

            return spaceObjects[0].prefab;
        }
    }
}

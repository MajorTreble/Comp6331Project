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
        public float cameraSpawnDistance = 50f; // Distance in front of the camera

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

            Vector3 spawnPos;

            if (player != null)
            {
                // Spawn around the player
                spawnPos = player.position + Random.onUnitSphere * spawnRadius;
            }
            else
            {
                // Spawn in front of the main camera with random offset
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

            GameObject obj = Instantiate(spaceObjectPrefabs[Random.Range(0, spaceObjectPrefabs.Length)], spawnPos, Random.rotation);
            activeObjects.Add(obj);
        }

        void RecycleOrDespawnObjects()
        {
            for (int i = activeObjects.Count - 1; i >= 0; i--)
            {
                Vector3 referencePoint = player != null ? player.position : Camera.main.transform.position;

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
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controller
{
    public class SpaceEnvironmentController : MonoBehaviour
    {
        public static SpaceEnvironmentController Instance { get; private set; }


        [System.Serializable]
        public class SpaceObject
        {
            public GameObject prefab;
            public int weight; // Higher value = More frequent spawning
        }

        [Header("Space Objects")]
        [SerializeField]
        public SpaceObject[] spaceObjects; // List of objects with their spawn weight
        public GameObject mineableAsteroid;

        public Transform player;
        public float spawnRadius = 150f;
        public int maxObjects = 30;
        public float despawnDistance = 400f;
        public float cameraSpawnDistance = 50f;

        public List<GameObject> activeObjects = new List<GameObject>();
        public List<GameObject> activeMineableAsteroids = new List<GameObject>();
        private int totalWeight;

        private Rigidbody playerRb;

        [Header("Special Asteroid Spawns")]
        public Vector3 magneticAsteroidSpawnPosition = new Vector3(40, 15, 150);

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }            
            Instance = this;
        }

        void Start()
        {
            if (player == null)
            {
                GameObject foundPlayer = GameObject.FindWithTag("Player");
                if (foundPlayer != null)
                {
                    player = foundPlayer.transform;
                    playerRb = player.GetComponent<Rigidbody>();
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

            if (JobController.Inst != null && JobController.Inst.currJob != null &&
                JobController.Inst.currJob.jobType == JobType.Mine)
            {
                SpawnMineableAsteroids(JobController.Inst.currJob.quantity);
                ForceSpawnMagneticAndBreakable();
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
                spawnPos = player.position + (player.forward * spawnRadius) + (Random.onUnitSphere * spawnRadius);
                //player + fowardpos + randompos

                float dynamicDistance = spawnRadius;

                if (playerRb != null)
                {
                    float speed = playerRb.velocity.magnitude;
                    dynamicDistance = Mathf.Clamp(spawnRadius + speed * 2f, spawnRadius, despawnDistance);
                    spawnPos = player.position + (player.forward * dynamicDistance) + (Random.onUnitSphere * dynamicDistance);
                    Debug.Log($"[SpaceEnvironmentController] Speed: {playerRb.velocity.magnitude}, Spawn Distance: {dynamicDistance}");
                }
            }
            else
            {
                Debug.LogError("should always have a player");

                Camera mainCam = Camera.main;
                if (mainCam == null)
                {
                    Debug.LogError("[SpaceEnvironmentController] Main Camera not found! Cannot spawn objects.");
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
                newPos = player.position + (player.forward * spawnRadius) + (Random.onUnitSphere * spawnRadius);
                float dynamicDistance = spawnRadius;
                
                if (playerRb != null)
                {
                    float speed = playerRb.velocity.magnitude;
                    dynamicDistance = Mathf.Clamp(spawnRadius * 2 + speed * 2f, spawnRadius, spawnRadius * 3f);
                    newPos = player.position + (player.forward * dynamicDistance) + (Random.onUnitSphere * dynamicDistance);
                    Debug.Log($"[SpaceEnvironmentController] Speed: {playerRb.velocity.magnitude}, Spawn Distance: {dynamicDistance}");
                }
                obj.transform.position = newPos;
                obj.transform.rotation = Random.rotation;
            } 
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

        void SpawnMineableAsteroids(int count)
        {
            if (mineableAsteroid == null)
            {
                Debug.LogWarning("[SpaceEnvironmentController] Mineable asteroid prefab is not assigned.");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPos;

                if (player != null)
                {
                    Vector3 forward = player.forward;
                    Vector3 randomOffset = Random.insideUnitSphere * spawnRadius * 5f;
                    spawnPos = player.position + forward * 300 + randomOffset;
                }
                else
                {
                    Debug.LogWarning("[SpaceEnvironmentController] No player found for spawn position.");
                    continue;
                }
                GameObject org = GameObject.Find("Org_" + mineableAsteroid.name);
                if (org == null)
                {
                    org = new GameObject("Org_" + mineableAsteroid.name);
                }
                GameObject obj = Instantiate(mineableAsteroid, spawnPos, Random.rotation, org.transform);

                Debug.Log($"[SpaceEnvironmentController] Spawned mineable asteroid at {spawnPos}");
                activeMineableAsteroids.Add(obj);
            }

            Debug.Log($"[SpaceEnvironmentController] Spawned {count} mineable asteroids.");
        }

        // Force blackhole situation, only in mine job
        void ForceSpawnMagneticAndBreakable()
        {
            GameObject magneticAsteroidPrefab = null;
            GameObject breakableAsteroidPrefab = null;
            foreach (var obj in spaceObjects)
            {
                if (obj.prefab.CompareTag("MagneticAsteroid"))
                {
                    magneticAsteroidPrefab = obj.prefab;
                }

                if (obj.prefab.CompareTag("Asteroid"))
                {
                    breakableAsteroidPrefab = obj.prefab;
                }
            }

            if (magneticAsteroidPrefab == null || breakableAsteroidPrefab == null)
            {
                Debug.LogWarning("[SpaceEnvironmentController] No prefab tagged 'MagneticAsteroid'|'BreakableAsteroid' found in spaceObjects!");
                return;
            }

            GameObject magnetic = Instantiate(
                magneticAsteroidPrefab,
                magneticAsteroidSpawnPosition,
                Quaternion.identity);
            activeObjects.Add(magnetic);

            if (breakableAsteroidPrefab != null)
            {
                Vector3 offset = Vector3.right * 6f;
                GameObject breakable = Instantiate(
                    breakableAsteroidPrefab,
                    magneticAsteroidSpawnPosition + offset,
                    Quaternion.identity);
                activeObjects.Add(breakable);
            }
            else
            {
                Debug.LogWarning("[SpaceEnvironmentController] breakableAsteroidPrefab not assigned.");
            }
        }
    }
}

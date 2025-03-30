using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Debug only

using Model;

namespace Manager
{

    public class SpawningManager : MonoBehaviour
    {
        public static SpawningManager Instance { get; private set; }

        public Vector3 portalPosition = new Vector3(25, 25, 25);

        public List<Ship> shipList = new List<Ship>();

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded; // Debug
            SceneManager.sceneUnloaded += OnSceneUnloaded; // Debug
        }

        public GameObject Spawn(SpawnParams spawnParams)
        {
            GameObject shipObject = Instantiate(spawnParams.Prefab, spawnParams.position, spawnParams.rotation, spawnParams.parent);
            spawnParams.Setup(shipObject);

            Ship ship = shipObject.GetComponent<Ship>();
            if (ship != null)
            {
                shipList.Add(ship);
            }

            return shipObject;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "MainMenu" || scene.name == "Harbor")
            {
                return;
            }
            // Replace with scenario

            int numberOfEnemies = 10; // Number of enemies to spawn
            float spawnRadius = 100f; // Spawn radius

            GameObject org = new GameObject();
            org.transform.name = "Org_Pirates";

            for (int i = 0; i < numberOfEnemies; i++)
            {
                // Random position within spawn radius
                Vector3 randomPosition = Random.insideUnitSphere * spawnRadius;
                randomPosition.y = 0.0f;
                randomPosition += transform.position;

                SpawnParams spawnParams = new SpawnParams();
                spawnParams.position = randomPosition;
                spawnParams.shipType = ShipType.Light;
				//spawnParams.shipType = (ShipType)Random.Range(0, 2);
				List<Faction> AllFactions = new List<Faction>
                {
	                Resources.Load<Faction>("Scriptable/Faction/Pirate Faction"),
	                Resources.Load<Faction>("Scriptable/Faction/Solo Faction"),
	                Resources.Load<Faction>("Scriptable/Faction/Colonial Federation"),
	                Resources.Load<Faction>("Scriptable/Faction/Earth Alliance")
                };
				spawnParams.faction = AllFactions[Random.Range(0, AllFactions.Count)];

                spawnParams.parent = org.transform;

                //PirateShipScript

                Spawn(spawnParams);
            }
        }

        void OnSceneUnloaded(Scene scene)
        {
            shipList.Clear();
        }

    }
}
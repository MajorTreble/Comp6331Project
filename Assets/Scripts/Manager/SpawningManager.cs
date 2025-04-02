using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Debug only

using Model;
using Model.AI;
using Controller;

namespace Manager
{

    public class SpawningManager : MonoBehaviour
    {
        public static SpawningManager Instance { get; private set; }

        public List<Ship> shipList = new List<Ship>();
		private Dictionary<Faction.FactionType, AIShip> factionLeaders = new Dictionary<Faction.FactionType, AIShip>();

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

			AIShip aiShip = shipObject.GetComponent<AIShip>();
			if (aiShip != null)
			{
				// Randomly choose between targetFaction and rewardFaction to patrol
				Faction factionToPatrol = ChooseRandomFaction(JobController.Inst.currJob.targetFaction, JobController.Inst.currJob.allyFaction);
				aiShip.shouldPatrol = (aiShip.faction == factionToPatrol);

				// Assign patrol behavior to the randomly selected faction
				if (aiShip.faction == factionToPatrol)
				{
					aiShip.AssignPatrolBehavior(true);  // Assign patrol behavior
				}
				else
				{
					aiShip.AssignPatrolBehavior(false); // Do not patrol
				}

				// Check if this faction already has a leader
				AIShip designatedLeader = null;

				if (aiShip.behavior != null && aiShip.behavior.groupMode == AIBehavior.GroupMode.Formation)
				{
					if (!factionLeaders.TryGetValue(aiShip.factionType, out designatedLeader))
					{
						// Assign as leader
						factionLeaders[aiShip.factionType] = aiShip;
					}
				}

				aiShip.InitializeShip(aiShip.behavior, aiShip.factionType, designatedLeader);
			}

			return shipObject;
		}


		private Faction ChooseRandomFaction(Faction targetFaction, Faction allyFaction)
		{
			if (targetFaction == null || allyFaction == null)
			{
				Debug.LogError("Missing faction reference in job!");
				return null;
			}

			int randomIndex = Random.Range(0, 2);
			return randomIndex == 0 ? targetFaction : allyFaction;
		}

		public void SpawnScenario(Scenario scenario)
        {
            foreach (UnitGroup unitGroup in scenario.unitGroups)
            {
                GameObject orgFaction = new GameObject();
                orgFaction.transform.name = "Org_" + unitGroup.faction.name;

                foreach (ShipType type in unitGroup.shipTypes)
                {
                    SpawnParams spawnParams = new SpawnParams();
                    spawnParams.position = unitGroup.SpawnPosition();
                    spawnParams.rotation = unitGroup.rotation;
                    spawnParams.faction = unitGroup.faction;
                    spawnParams.shipType = type;
                    spawnParams.parent = orgFaction.transform;

                    Spawn(spawnParams);
                }
            }

            GameObject orgSpaceObject = new GameObject();
            orgSpaceObject.transform.name = "Org_Space Object";
            foreach (SpaceObjectGroup spaceObjectGroup in scenario.spaceObjectGroups)
            {
                SpawnParams spawnParams = new SpawnParams();
                spawnParams.position = spaceObjectGroup.SpawnPosition();
                spawnParams.rotation = spaceObjectGroup.rotation;
                spawnParams.prefab = spaceObjectGroup.prefab;
                spawnParams.parent = orgSpaceObject.transform;

                Spawn(spawnParams);
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("Check After Build");
        }

        void OnSceneUnloaded(Scene scene)
        {
            shipList.Clear();
        }

    }
}

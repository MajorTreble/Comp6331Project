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
		//private Faction selectedPatrolFaction;
		private Dictionary<Faction, AI_Waypoints> factionPaths = new Dictionary<Faction, AI_Waypoints>();

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
                bool isDefendFaction = JobController.Inst.currJob != null && aiShip.faction == JobController.Inst.currJob.allyFaction;

				// Set patrol: Defend faction always patrols; others use AIBehavior.isPatrol
				aiShip.shouldPatrol = isDefendFaction || aiShip.behavior.isPatrol; 
				
				Debug.Log($"Ship {aiShip.name} (Faction: {aiShip.faction.name}) - Should Patrol: {aiShip.shouldPatrol}");

				AIWaypointNavigator nav = shipObject.GetComponent<AIWaypointNavigator>();

				if (aiShip.shouldPatrol)
				{
					// Determine which faction's path to use
					Faction patrolFaction = isDefendFaction ?
						JobController.Inst.currJob.allyFaction :
						aiShip.faction;

					Debug.Log($"[SPAWN] {aiShip.name} assigned to patrol. Setting state to Patrol.");
					if (!factionPaths.ContainsKey(patrolFaction))
					{
						CreateFactionPath(patrolFaction, spawnParams.Prefab);
					}
					if (nav != null && factionPaths.ContainsKey(patrolFaction))
					{
						nav.path = factionPaths[patrolFaction];
						aiShip.UpdatePatrolState(true);
					}
				}
				else
				{
					Debug.Log($"[SPAWN] {aiShip.name} not patrolling. State: {aiShip.currentState}");
					aiShip.UpdatePatrolState(false);
					if (nav != null) nav.ClearPath();
				}
                // Check if this faction already has a leader
                AIShip designatedLeader = null;

                if (aiShip.behavior != null)
                {
                    if (!factionLeaders.TryGetValue(aiShip.faction.factionType, out designatedLeader))
                    {
                        // Assign as leader
                        factionLeaders[aiShip.faction.factionType] = aiShip;
                    }
                }
            }
            return shipObject;
        }

		private void CreateFactionPath(Faction faction, GameObject shipPrefab)
		{
			AIWaypointNavigator prefabNav = shipPrefab.GetComponent<AIWaypointNavigator>();
			if (prefabNav == null)
			{
				//Debug.LogError($"Prefab {shipPrefab.name} has no AIWaypointNavigator!");
				return;
			}
			if (prefabNav.path == null)
			{
				//Debug.LogError($"Prefab {shipPrefab.name}'s AIWaypointNavigator has no path assigned!");
				return;
			}

			// Rest of your existing instantiation code
			AI_Waypoints pathInstance = Instantiate(prefabNav.path, Vector3.zero, Quaternion.identity);
			pathInstance.name = $"{faction.name}_PatrolPath";
			factionPaths.Add(faction, pathInstance);
			Debug.Log($"Created path for {faction.name} with {pathInstance.WaypointCount} waypoints");
		}

		public void SpawnScenario(Scenario scenario)
        {
			GameObject portal = GameManager.Instance.portal;
            if (portal != null)
            {
                portal.transform.position = scenario.portalPosition;
                //portal.SetActive(false);
            }

            foreach (UnitGroup unitGroup in scenario.unitGroups)
            {

				GameObject orgFaction = new GameObject();
                orgFaction.transform.name = "Org_" + unitGroup.faction.name;

                AIGroup group = new AIGroup();
                group.groupMode = unitGroup.groupMode;

                foreach (ShipType type in unitGroup.shipTypes)
                {
                    SpawnParams spawnParams = new SpawnParams();
                    spawnParams.position = unitGroup.SpawnPosition();
                    spawnParams.rotation = unitGroup.rotation;
                    spawnParams.faction = unitGroup.faction;
                    spawnParams.shipType = type;
                    spawnParams.group = group;
                    spawnParams.parent = orgFaction.transform;

                    Spawn(spawnParams);
                }

                if (group.ships.Count > 0)
                {
                    group.leader = group.ships[0];
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


        void OnSceneUnloaded(Scene scene)
        {
            shipList.Clear();
        }

    }
}

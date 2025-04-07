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

            SceneManager.sceneUnloaded += OnSceneUnloaded;
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

        public void SpawnScenario(Scenario scenario, Faction jobAlly, Faction jobTarget)
        {
            GameObject portal = GameManager.Instance.portal;
            if (portal != null)
            {
                portal.transform.position = scenario.portalPosition;
                //portal.SetActive(false);
            }

            foreach (UnitGroup unitGroup in scenario.unitGroups)
            {
                SpawnUnitGroup(unitGroup, jobAlly, jobTarget);
            }

            if (scenario.defendUnitGroup)
            {
                SpawnUnitGroup(scenario.defendUnitGroup, jobAlly, jobTarget);
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

        public void SpawnUnitGroup(UnitGroup unitGroup, Faction jobAlly, Faction jobTarget)
        {
            if (unitGroup == null || jobAlly == null || jobTarget == null)
            {
                return;
            }

            Faction faction = unitGroup.jobFaction == JobFaction.JobAlly ? jobAlly : jobTarget;

            GameObject orgFaction = new GameObject();
            orgFaction.transform.name = "Org_" + faction.name;

            AIGroup group = new AIGroup();
            group.groupMode = unitGroup.groupMode;

            AI_Waypoints patrol = null;
            if (unitGroup.patrolPrefab)
            {
                SpawnParams spawnParamsPatrol = new SpawnParams(unitGroup.patrolPrefab, Vector3.zero, Quaternion.identity);
                GameObject patrolObject = Spawn(spawnParamsPatrol);
                patrol = patrolObject.GetComponent<AI_Waypoints>();
            }
            SpawnParams spawnParams = new SpawnParams();


            foreach (ShipType type in unitGroup.shipTypes)
            {
                SpawnParams spawnParamsShip = new SpawnParams();
                spawnParamsShip.position = unitGroup.SpawnPosition();
                spawnParamsShip.rotation = unitGroup.rotation;
                spawnParamsShip.faction = faction;
                spawnParamsShip.shipType = type;
                spawnParamsShip.group = group;
                spawnParamsShip.patrol = patrol;
                spawnParamsShip.parent = orgFaction.transform;

                Spawn(spawnParamsShip);
            }

            if (group.ships.Count > 0)
            {
                group.leader = group.ships[0];
            }

        }

        void OnSceneUnloaded(Scene scene)
        {
            shipList.Clear();
        }

    }
}

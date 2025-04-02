using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Debug only

using Model;
using Model.AI;

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

        public void SpawnScenario(Scenario scenario)
        {
            GameObject portal = GameManager.Instance.portal;
            if (portal != null)
            {
                portal.transform.position = scenario.portalPosition;
                portal.SetActive(false);
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

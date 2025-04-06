using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{

    public enum JobFaction { JobAlly, JobTarget }
    public enum GroupMode { None, Formation }

    [CreateAssetMenu(fileName = "UnitGroup", menuName = "ScriptableObjects/UnitGroup", order = 1)]
    public class UnitGroup : ScriptableObject
    {
        public Vector3 position = Vector3.zero;
        public Quaternion rotation = Quaternion.identity;
        public float radius = 10.0f;

        public JobFaction jobFaction = JobFaction.JobTarget;
        public List<ShipType> shipTypes = new List<ShipType>();

        [Header("Group Behavior")]
        public GroupMode groupMode = GroupMode.None;

        [Header("Patrol Path")]
        public GameObject patrolPrefab = null;

        public Vector3 SpawnPosition()
        {
            Vector3 randomPosition = Random.insideUnitSphere * radius;
            randomPosition.y = 0.0f;
            randomPosition += position;

            return randomPosition;
        }
    }

}
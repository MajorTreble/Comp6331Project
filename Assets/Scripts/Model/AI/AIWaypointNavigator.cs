using AI.Steering;
using Model.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model.AI
{

    public class AIWaypointNavigator : MonoBehaviour
    {
        public AI_Waypoints path;
        public float stoppingDistance = 1.5f;
        public float waypointRadius = 30f;
        private int currentWaypointIndex = 0;

        private SteeringAgent steeringAgent;

        private void Start()
        {
            steeringAgent = GetComponent<SteeringAgent>();
        }

        private void Update()
        {
            if (path == null) return;

            // Update target to current waypoint
            Vector3 targetPos = path.GetWaypoint(currentWaypointIndex).position;

            steeringAgent.TargetPosition = targetPos;

            // Check distance to waypoint
            float dist = Vector3.Distance(transform.position, targetPos);
            if (dist < waypointRadius)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % path.WaypointCount;
            }
        }

        public void ClearPath()
        {
            path = null;
            currentWaypointIndex = 0;
        }
    }
}
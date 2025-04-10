using Manager;
using Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Steering
{
    public class CollisionAvoidance : Movement
    {
        public List<SteeringAgent> avoidList = new List<SteeringAgent>();
        public float avoidDistance = 11.5f;
        public float maxSeeDistance = 5.0f; 

        public override SteeringOutput GetKinematic(SteeringAgent agent)
        {
            var output = base.GetKinematic(agent);
            return output;
        }

        public override SteeringOutput GetSteering(SteeringAgent agent)
        {
			// To Refresh avoid list
			if (avoidList.Count == 0 && SpawningManager.Instance != null)
			{
				foreach (Ship ship in SpawningManager.Instance.shipList)
				{
					if (ship != null && ship != agent.GetComponent<Ship>() && ship.steeringAgent != null)
					{
						avoidList.Add(ship.steeringAgent);
					}
				}
			}

			var output = base.GetSteering(agent);

            // Find closest agent that will collide
            SteeringAgent closestAgent = null;
            float closestTime = 999999.0f;

            Vector3 closestDirection = Vector3.zero;
            Vector3 closestVelocity = Vector3.zero;
            float closestSpeed = 0;
            float closestSafeDistance = 0;
            float closestCollisionDistance = 0;

            foreach (SteeringAgent avoidAgent in avoidList)
            {
                if(avoidAgent.gameObject.activeSelf == false) continue;

                Vector3 direction = agent.transform.position - avoidAgent.transform.position;
                Vector3 velocity = agent.Velocity - avoidAgent.Velocity;

                float speed = velocity.magnitude;
                if (speed == 0)
				{
					//Debug.LogWarning($"[{agent.name}] Skipping {avoidAgent.name} due to zero relative speed.");
					continue;
				}

                //float time = (direction - velocity) / (speed * speed);
                float time = -1.0f * Vector3.Dot(direction, velocity) / (speed * speed);
                if (time >= 50.0f)
				{
					//Debug.LogWarning($"[{agent.name}] Skipping {avoidAgent.name}, time to collision too far: {time:F2}");
					continue;
				}

                float safeDistance = agent.radius + avoidAgent.radius + avoidDistance;
                float collisionDistance = (direction + velocity * time).magnitude;
                if (collisionDistance > safeDistance)
				{
					//Debug.LogWarning($"[{agent.name}] Skipping {avoidAgent.name}, collisionDistance {collisionDistance:F2} > safeDistance {safeDistance:F2}");
					continue;
				}

                 if (time > 0 && time < closestTime)
				{
                    closestTime = time;
                    closestAgent = avoidAgent;
                    closestDirection = direction;
                    closestVelocity = velocity;
                    closestSpeed = speed;
                    closestSafeDistance = safeDistance;
                    closestCollisionDistance = collisionDistance;
                }
            }

            if (closestAgent == null)
			{
                return output;
			}


            // Avoid the agent
            Vector3 desiredDirection;
            if (closestCollisionDistance <= 0 || closestDirection.magnitude < closestSafeDistance)
            {
                desiredDirection = closestDirection;
            }
            else
            {
                desiredDirection = closestDirection + closestVelocity * closestTime;
            }

            /* Avoid the target */
            desiredDirection.Normalize();
            desiredDirection *= agent.maxSpeed * 1.5f;

            DrawDebug(agent, closestAgent, desiredDirection, closestDirection, Color.red);

            output.linear = desiredDirection;
            return output;
        }

        private void DrawDebug(SteeringAgent agent, SteeringAgent closestAgent, Vector3 direction, Vector3 closestDirection, Color color)
        {
            if (!debug)
			{
                return;
			}

            Vector3 pos = agent.transform.position;
            pos.z += 0.5f;
            Vector3 close = closestAgent.transform.position;
            close.z += 0.5f;
            DebugUtil.DrawCircle(pos, agent.transform.up, color, agent.radius);
            DebugUtil.DrawCircle(close, closestAgent.transform.up, color, closestAgent.radius);
            Debug.DrawRay(pos, close, color);

            pos.z += 0.1f;

            Debug.DrawRay(pos, closestDirection, Color.blue);
            Debug.DrawRay(pos, direction, Color.black);
        }
    }
}

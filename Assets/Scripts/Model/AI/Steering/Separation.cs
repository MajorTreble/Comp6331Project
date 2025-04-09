using UnityEngine;

using AI.Steering;
using Manager;
using Model.AI;
using Model;

namespace Model.AI.Steering
{

    public class Separation : Movement
    {
        public float Weight = 3.0f;
        public float DesiredSeparation = 15.0f;

        public override SteeringOutput GetSteering(SteeringAgent agent)
        {
            SteeringOutput output = new SteeringOutput();
            Vector3 separationForce = Vector3.zero;
            int count = 0;

            foreach (Ship ship in SpawningManager.Instance.shipList)
            {
                if (ship == agent.owner.GetComponent<Ship>() || !(ship is AIShip otherShip))
                    continue;

                if (otherShip.faction != agent.owner.GetComponent<AIShip>().faction)
                    continue;

                float distance = Vector3.Distance(agent.transform.position, ship.transform.position);
                if (distance > 0 && distance < DesiredSeparation)
                {
                    Vector3 diff = agent.transform.position - ship.transform.position;
                    diff.Normalize();
                    diff /= distance;
                    separationForce += diff;
                    count++;
                }
            }

            if (count > 0)
            {
                separationForce /= count;
                separationForce.Normalize();
                output.linear = separationForce * Weight;
            }

            return output;
        }

    }
}
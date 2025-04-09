using UnityEngine;

namespace AI.Steering
{
    public class Pursue : Movement
    {
        public override SteeringOutput GetKinematic(SteeringAgent agent)
        {
            var output = base.GetKinematic(agent);

            Vector3 desiredVelocity = (agent.TargetPosition + agent.TargetVelocity) - agent.transform.position;
            //desiredVelocity.y = 0;
            desiredVelocity = desiredVelocity.normalized * agent.maxSpeed;
            output.linear = desiredVelocity;

            if (debug) Debug.DrawRay(agent.transform.position, output.linear, Color.cyan);

            return output;
        }

        public override SteeringOutput GetSteering(SteeringAgent agent)
        {
            var output = base.GetSteering(agent);

            output.linear = GetKinematic(agent).linear - agent.Velocity;

            if (debug) Debug.DrawRay(agent.transform.position + agent.Velocity, output.linear, Color.green);

            return output;
        }
    }
}

using UnityEngine;

namespace AI.Steering
{
    public class Flee : Movement
    {
        public override SteeringOutput GetKinematic(SteeringAgent agent)
        {
            var output = base.GetKinematic(agent);

            Vector3 desiredVelocity = agent.transform.position - agent.TargetPosition;
            desiredVelocity.y = 0;
            desiredVelocity = desiredVelocity.normalized * agent.maxSpeed;
            output.linear = desiredVelocity;

            return output;
        }

        public override SteeringOutput GetSteering(SteeringAgent agent)
        {
            var output = base.GetSteering(agent);

            output.linear = GetKinematic(agent).linear - agent.Velocity;

            return output;
        }
    }
}

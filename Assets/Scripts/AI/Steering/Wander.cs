using UnityEngine;

namespace AI.Steering
{
    public class Wander : Movement
    {
        public float wanderDegreesDelta = 45;
        [Min(0)] public float wanderInterval = 0.75f;
        protected float wanderTimer = 999;

        public float wanderRadius = 10;

        public Vector3 lastWanderDirection;
        private Vector3 lastWanderVelocity;

        Vector3 debugCenter = Vector3.zero;
        Vector3 debugDirection = Vector3.zero;

        public override SteeringOutput GetKinematic(SteeringAgent agent)
        {
            var output = base.GetKinematic(agent);
            wanderTimer += Time.deltaTime;

            Vector3 lastDisplacement = agent.Velocity;
            Vector3 desiredVelocity = lastWanderVelocity;

            if (lastWanderDirection == Vector3.zero)
                lastWanderDirection = agent.transform.forward.normalized * agent.maxSpeed;

            if (lastDisplacement == Vector3.zero)
                lastDisplacement = agent.transform.forward;

            //Vector3 desiredVelocity = lastDisplacement;
            if (wanderTimer > wanderInterval)
            {
                float angle = (Random.value - Random.value) * wanderDegreesDelta;
                Vector3 direction = Quaternion.AngleAxis(angle, Vector3.up) * lastWanderDirection.normalized;
                Vector3 circleCenter = agent.transform.position + lastDisplacement.normalized * agent.maxSpeed;
                Vector3 destination = circleCenter + direction.normalized * wanderRadius;
                desiredVelocity = destination - agent.transform.position;
                desiredVelocity.y = 0;
                desiredVelocity = desiredVelocity.normalized * agent.maxSpeed;

                lastWanderDirection = direction;
                lastWanderVelocity = desiredVelocity;
                debugDirection = direction;

                debugCenter = circleCenter;
                debugCenter.y += 10.0f;

                wanderTimer = 0;
            }

            output.linear = desiredVelocity;
			
            /*
			if (debug) 
            {
                Vector3 debugPos = agent.transform.position;
                debugPos.y += 10.0f;
                Debug.DrawRay(debugPos, output.linear, Color.cyan);

                DebugUtil.DrawCircle(debugCenter, agent.transform.up, Color.cyan, wanderRadius);
                Debug.DrawRay(debugCenter, debugDirection * wanderRadius, Color.cyan);
                Debug.DrawRay(debugCenter, lastWanderDirection, Color.red);
            }
            */

            return output;
        }

        public override SteeringOutput GetSteering(SteeringAgent agent)
        {
            var output = base.GetSteering(agent);

            output.linear = GetKinematic(agent).linear - agent.Velocity;

            if (debug)
            {
                Vector3 debugPos = agent.transform.position;
                debugPos.y += 10.0f;
                Debug.DrawRay(debugPos + agent.Velocity, output.linear, Color.green);
            }

            return output;
        }
    }
}

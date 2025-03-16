using UnityEngine;

namespace AI.Steering
{
    public abstract class Movement
    {
        public bool debug;
        public float weight = 1.0f;

        public SteeringAgent overrideTarget = null;

        public virtual SteeringOutput GetKinematic(SteeringAgent agent)
        {
            return new SteeringOutput { angular = agent.transform.rotation };
        }

        public virtual SteeringOutput GetSteering(SteeringAgent agent)
        {
            return new SteeringOutput { angular = Quaternion.identity };
        }

        public Vector3 TargetPosition(SteeringAgent agent)
		{
            if (overrideTarget != null)
			{
                return overrideTarget.transform.position;

            }

            return agent.TargetPosition;
		}

        public Vector3 TargetVelocity(SteeringAgent agent)
        {
            if (overrideTarget != null)
            {
                return overrideTarget.Velocity;

            }

            return agent.TargetVelocity;
        }
    }
}

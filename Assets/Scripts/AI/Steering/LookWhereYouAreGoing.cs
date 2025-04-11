using UnityEngine;

namespace AI.Steering
{
    public class LookWhereYouAreGoing : Movement
    {
        public override SteeringOutput GetKinematic(SteeringAgent agent)
        {
            var output = base.GetKinematic(agent);

            // TODO: calculate angular component
            if (agent.Velocity == Vector3.zero)
                return output;

            if (agent.Velocity != Vector3.zero)
                output.angular = Quaternion.LookRotation(agent.Velocity);

            return output;
        }

        public override SteeringOutput GetSteering(SteeringAgent agent)
        {
            var output = base.GetSteering(agent);

            if (agent.lockY)
            {
                // get the rotation around the y-axis
                Vector3 from = Vector3.ProjectOnPlane(agent.transform.forward, Vector3.up);
                Vector3 to = GetKinematic(agent).angular * Vector3.forward;
                float angleY = Vector3.SignedAngle(from, to, Vector3.up);
                output.angular = Quaternion.AngleAxis(angleY, Vector3.up);
            }
            else
                output.angular = Quaternion.FromToRotation(agent.transform.forward, GetKinematic(agent).angular * Vector3.forward);
                //output.angular = GetKinematic(agent).angular;
                
            if (debug) Debug.DrawRay(agent.transform.position + agent.Velocity, output.linear, Color.yellow);



            return output;
        }
    }
}
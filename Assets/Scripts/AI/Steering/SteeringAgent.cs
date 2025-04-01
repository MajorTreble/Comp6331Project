using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Steering
{
    public class SteeringAgent
    {
        public GameObject owner;
        public Rigidbody rigidBody;

        public float initialMaxSpeed;
        public float maxSpeed = 15;
        public float rotationSpeed = 1.0f;
        public float radius = 1.0f;
        public bool lockY = true;
        public bool debug = false;

        public enum EBehaviorType { Kinematic, Steering }
        public EBehaviorType behaviorType = EBehaviorType.Steering;

        public List<Movement> movements = new List<Movement>();


        public SteeringAgent trackedTarget;
        public Vector3 targetPosition;

        public Vector3 TargetPosition
        {
            get => trackedTarget != null ? trackedTarget.transform.position : targetPosition;
			set => targetPosition = value;
		}
        public Vector3 TargetForward
        {
            get => trackedTarget != null ? trackedTarget.transform.forward : Vector3.forward;
        }
        public Vector3 TargetVelocity
        {
            get
            {
                Vector3 v = Vector3.zero;
                if (trackedTarget != null)
                {
                    v = trackedTarget.Velocity;
                }

                return v;
            }
        }

        public Vector3 Velocity { get; set; }

        public Transform transform
        {
            get => owner.transform;
        }

        public void TrackTarget(SteeringAgent target)
        {
            trackedTarget = target;
        }

        public void UnTrackTarget()
        {
            trackedTarget = null;
        }

        public SteeringAgent(GameObject owner, Rigidbody rigidBody)
        {
            this.owner = owner;
            this.rigidBody = rigidBody;
        }

        public void Update()
        {
            if (!owner)
            {
                return;
            }

            if (debug)
            {
                Debug.DrawRay(transform.position, Velocity, Color.magenta);
            }

            if (behaviorType == EBehaviorType.Kinematic)
            {
                Vector3 kinematicAvg;
                Quaternion rotation;
                GetKinematicAvg(out kinematicAvg, out rotation);

                Velocity = Vector3.ClampMagnitude(kinematicAvg, maxSpeed);

                transform.rotation = rotation;
            }
            else
            {
                Vector3 steeringForce;
                Quaternion rotation;
                GetSteeringSum(out steeringForce, out rotation);
                Velocity += steeringForce * Time.deltaTime;
                Velocity = Vector3.ClampMagnitude(Velocity, maxSpeed);

                //transform.rotation = transform.rotation * rotation;
                transform.rotation = Quaternion.Slerp(transform.rotation, transform.rotation * rotation, rotationSpeed * Time.deltaTime);
            }

            if (rigidBody)
            {
                rigidBody.transform.position += Velocity * Time.deltaTime;
            }
            else
            {
                transform.position += Velocity * Time.deltaTime;
            }
        }

        private void GetKinematicAvg(out Vector3 kinematicAvg, out Quaternion rotation)
        {
            kinematicAvg = Vector3.zero;
            Vector3 eulerAvg = Vector3.zero;

            int count = 0;
            foreach (Movement movement in movements)
            {
                kinematicAvg += movement.GetKinematic(this).linear;
                eulerAvg += movement.GetKinematic(this).angular.eulerAngles;

                ++count;
            }

            if (count > 0)
            {
                kinematicAvg /= count;
                eulerAvg /= count;
                rotation = Quaternion.Euler(eulerAvg);
            }
            else
            {
                kinematicAvg = Velocity;
                rotation = transform.rotation;
            }
        }

        private void GetSteeringSum(out Vector3 steeringForceSum, out Quaternion rotation)
        {
            steeringForceSum = Vector3.zero;
            rotation = Quaternion.identity;

            foreach (Movement movement in movements)
            {
                steeringForceSum += movement.GetSteering(this).linear * movement.weight;
                rotation *= movement.GetSteering(this).angular;
            }
        }
    }
}
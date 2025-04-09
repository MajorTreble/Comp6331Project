using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model.Environment
{
    public class SpaceStorm : MonoBehaviour
    {
        public float stormRadiusX = 75f;
        public float stormRadiusY = 300f;
        public float stormRadiusZ = 45f;
        public float turbulenceStrength = 50f;
        public float objectPushForce = 20f;
        public float forceUpdateInterval = 0.5f;
        public LayerMask affectedLayers;

        private List<Rigidbody> affectedObjects = new List<Rigidbody>();
        private HashSet<Rigidbody> lastFrameObjects = new HashSet<Rigidbody>();

        void Start()
        {
            InvokeRepeating(nameof(FindAffectedObjects), 0f, forceUpdateInterval);
        }

        void FindAffectedObjects()
        {
            Vector3 boxSize = new Vector3(stormRadiusX, stormRadiusY, stormRadiusZ);
            Vector3 boxCenter = transform.position + transform.up * (stormRadiusY / 2);

            Collider[] nearbyObjects = Physics.OverlapBox(boxCenter, boxSize / 2, transform.rotation, affectedLayers);

            HashSet<Rigidbody> currentFrameObjects = new HashSet<Rigidbody>();
            affectedObjects.Clear();

            foreach (Collider col in nearbyObjects)
            {
                if (col.CompareTag("SpaceStorm")) continue;

                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    affectedObjects.Add(rb);
                    currentFrameObjects.Add(rb);
                }
            }

            ApplyStormEffects();

            foreach (Rigidbody rb in lastFrameObjects)
            {
                if (rb == null) continue;

                if (!currentFrameObjects.Contains(rb))
                {
                    OnObjectExitStorm(rb);
                }
            }

            lastFrameObjects = currentFrameObjects;
        }

        void ApplyStormEffects()
        {
            if (affectedObjects.Count == 0)
            {
                return;
            }

            foreach (Rigidbody rb in affectedObjects)
            {
                if (rb == null || rb.CompareTag("SpaceStorm")) continue;

                Vector3 randomForce = Random.insideUnitSphere * objectPushForce;
                rb.AddForce(randomForce, ForceMode.Acceleration);

                if (rb.CompareTag("Player"))
                {
                    CameraFollow.Inst?.StartShake(0.3f);
                    Utils.DebugLog($"[SpaceStorm] Applying turbulence to Player.");
                }
            }
        }

        void OnObjectExitStorm(Rigidbody rb)
        {
            if (rb == null || rb.CompareTag("SpaceStorm")) return;

            Utils.DebugLog($"[SpaceStorm] {rb.gameObject.name} ({rb.tag}) LEFT the storm.");

            if (rb.CompareTag("Player"))
            {
                Utils.DebugLog("[SpaceStorm] Resetting Player turbulence effect.");
                CameraFollow.Inst?.StopShake();
                return;
            }

            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 0.2f);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 boxSize = new Vector3(stormRadiusX, stormRadiusY, stormRadiusZ);
            Vector3 boxCenter = transform.position + transform.up * (stormRadiusY / 2);

            Gizmos.matrix = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, boxSize);
        }
    }
}

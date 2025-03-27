using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model.Environment {
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
            //Debug.Log("[SpaceStorm] Storm Initialized.");
            InvokeRepeating(nameof(FindAffectedObjects), 0f, forceUpdateInterval);
        }

        void FindAffectedObjects()
        {
            Vector3 boxSize = new Vector3(stormRadiusX, stormRadiusY, stormRadiusZ);
            Vector3 boxCenter = transform.position + transform.up * (stormRadiusY / 2);

            Collider[] nearbyObjects = Physics.OverlapBox(boxCenter, boxSize / 2, transform.rotation, affectedLayers);

            HashSet<Rigidbody> currentFrameObjects = new HashSet<Rigidbody>();
            affectedObjects.Clear();

            //Debug.Log($"[SpaceStorm] Checking at {boxCenter} (Size: {boxSize}, Rotation: {transform.rotation.eulerAngles})");
            //Debug.Log($"[SpaceStorm] Found {nearbyObjects.Length} objects.");

            foreach (Collider col in nearbyObjects)
            {
                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    affectedObjects.Add(rb);
                    currentFrameObjects.Add(rb);

                    //Debug.Log($"[SpaceStorm] {col.name} tag : {col.tag} detected at {col.transform.position}");
                }
            }

            ApplyStormEffects();

            // Check for objects that LEFT the storm
            foreach (Rigidbody rb in lastFrameObjects)
            {
                if(rb==null) continue;
                
                if (!currentFrameObjects.Contains(rb))
                {
                    OnObjectExitStorm(rb);
                }
            }

            lastFrameObjects = currentFrameObjects; // Update tracking for next frame
        }

        void ApplyStormEffects()
        {
            if (affectedObjects.Count == 0)
            {
                //Debug.Log("[SpaceStorm] No objects inside storm.");
                return;
            }

            foreach (Rigidbody rb in affectedObjects)
            {
                if (rb == null) continue;

                Vector3 randomForce = Random.insideUnitSphere * objectPushForce;
                rb.AddForce(randomForce, ForceMode.Acceleration);
                //Debug.Log($"[SpaceStorm] Applying force {randomForce} to {rb.gameObject.name}");

                if (rb.CompareTag("Player"))
                {
                    Vector3 turbulence = Random.insideUnitSphere * turbulenceStrength;
                    rb.AddForce(turbulence, ForceMode.Acceleration);
                    //Debug.Log($"[SpaceStorm] Applying turbulence {turbulence} to Player.");
                }
            }
        }

        // Reset Effects When Object Leaves Storm
        void OnObjectExitStorm(Rigidbody rb)
        {
            Debug.Log($"[SpaceStorm] {rb.gameObject.name} LEFT the storm.");

            if (rb.CompareTag("Player"))
            {
                Debug.Log("[SpaceStorm] Resetting Player turbulence effect.");
                rb.velocity = Vector3.zero;
            }
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
using UnityEngine;
using System.Collections.Generic;
using Model.AI.Environment;

namespace Model.Environment {
    public class MagneticAsteroid : MonoBehaviour
    {
        public float spinSpeed = 30f;
        public float pullRadius = 30f;
        public float captureRadius = 10f;
        public float pullForce = 15f;
        public float orbitSpeed = 5f;
        public LayerMask pullableLayers;

        private List<Rigidbody> orbitingObjects = new List<Rigidbody>();

        public GameObject fragmentPrefab;
        public int numberOfFragments = 5;
        public float breakForce = 3f;
        public MagneticFragmentGroupAI fragmentGroupPrefab;

        private bool hasBroken = false;


        void Update()
        {
            // Spin the magnetic asteroid
            transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime);

            // Pull nearby objects
            Collider[] colliders = Physics.OverlapSphere(transform.position, pullRadius, pullableLayers);

            foreach (Collider col in colliders)
            {
                Rigidbody rb = col.attachedRigidbody;

                if (col.CompareTag("IceAsteroid")) continue;

                if (rb != null && !orbitingObjects.Contains(rb))
                {
                    orbitingObjects.Add(rb);
                }
            }

            for (int i = orbitingObjects.Count - 1; i >= 0; i--)
            {
                Rigidbody obj = orbitingObjects[i];
                if (obj == null)
                {
                    orbitingObjects.RemoveAt(i);
                    continue;
                }

                Vector3 toCenter = transform.position - obj.position;
                float distance = toCenter.magnitude;
                Vector3 directionToCenter = toCenter.normalized;

                obj.AddForce(directionToCenter * pullForce * Time.deltaTime, ForceMode.Acceleration);

                if (distance < captureRadius)
                {
                    Vector3 orbitDirection = Vector3.Cross(Vector3.up, directionToCenter).normalized;
                    obj.velocity = orbitDirection * orbitSpeed;
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (hasBroken) return;

            if (collision.gameObject.CompareTag("PlayerLaser") || collision.gameObject.CompareTag("SpaceshipComponent"))
            {
                Debug.Log("[MagneticAsteroid] Breaking Magnetic Asteroid.");
                BreakApart();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasBroken) return;

            if (other.gameObject.CompareTag("PlayerLaser") || other.gameObject.CompareTag("SpaceshipComponent"))
            {
                Debug.Log("[MagneticAsteroid] Breaking Magnetic Asteroid.");
                BreakApart();
            }
        }



        public void BreakApart()
        {
            hasBroken = true;

            // Group manager
            MagneticFragmentGroupAI fragmentGroup = Instantiate(fragmentGroupPrefab, transform.position, Quaternion.identity);

            for (int i = 0; i < numberOfFragments; i++)
            {
                Vector3 randomOffset = Random.insideUnitSphere * 2f;
                Quaternion randomRot = Random.rotation;

                GameObject fragment = Instantiate(fragmentPrefab, transform.position + randomOffset, randomRot);
                fragment.transform.localScale = transform.localScale * 0.5f;

                MagneticFragment fragScript = fragment.GetComponent<MagneticFragment>();
                if (fragScript != null)
                {
                    fragScript.AssignGroup(fragmentGroup);
                }

                Rigidbody rb = fragment.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = GetComponent<Rigidbody>()?.velocity ?? Vector3.zero;
                    rb.AddExplosionForce(breakForce, transform.position, 5f);
                }
            }

            Destroy(gameObject);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, pullRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, captureRadius);
        }
    }


}
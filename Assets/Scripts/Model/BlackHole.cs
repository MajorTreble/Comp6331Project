using UnityEngine;

namespace Model {
    public class BlackHole : MonoBehaviour
    {
        public float gravityStrength = 10f;
        public float pullRadius = 50f;
        public float eventHorizonRadius = 5f;
        public float maxPullSpeed = 5f;

        void FixedUpdate()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, pullRadius);

            foreach (Collider col in colliders)
            {
                if (col.CompareTag("Player") || col.CompareTag("Asteroid"))
                {
                    Rigidbody rb = col.GetComponent<Rigidbody>();

                    if (rb != null)
                    {
                        Vector3 direction = (transform.position - col.transform.position).normalized;
                        float distance = Vector3.Distance(transform.position, col.transform.position);

                        float pullForce = gravityStrength / Mathf.Pow(distance, 1.5f);

                        if (rb.velocity.magnitude < maxPullSpeed)
                        {
                            rb.AddForce(direction * pullForce * Time.deltaTime, ForceMode.Acceleration);
                        }

                        if (distance < eventHorizonRadius)
                        {
                            Debug.Log(col.name + " entered the event horizon! Destroying...");
                            Destroy(col.gameObject);
                        }
                    }
                }
            }

        }

        void OnDrawGizmosSelected()
        {
            // gravity field in Scene view
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, pullRadius);

            // event horizon
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, eventHorizonRadius);
        }
    }

}
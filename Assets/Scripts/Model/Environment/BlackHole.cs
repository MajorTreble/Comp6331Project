using System.Collections;
using UnityEngine;

namespace Model.Environment {
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
                        if (col.CompareTag("Player") || col.CompareTag("SpaceshipComponent"))
                        {
                            Debug.Log("[BlackHole] Player reached event horizon. Initiating teleport...");
                            StartCoroutine(RespawnPlayer(col.gameObject, rb));
                        }
                        else {
                            Debug.Log(col.name + " entered the event horizon! Destroying...");
                            IDamagable damageable = col.GetComponent<IDamagable>();
                            damageable?.TakeDamage(10000, null);
                            //Destroy(col.gameObject);
                        }
                    }
                }            
            }
        }

        private IEnumerator RespawnPlayer(GameObject playerObj, Rigidbody rb)
        {
            if (CameraFollow.Inst != null)
                CameraFollow.Inst.StartShake(0.2f);

            yield return new WaitForSeconds(0.5f);
            if (CameraFollow.Inst != null)
                CameraFollow.Inst.StopShake();

            IDamagable damagable = playerObj.GetComponent<IDamagable>();
            if (damagable != null)
            {
                damagable.TakeDamage(100000, null);
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
using Model.AI.Environment;
using UnityEngine;

namespace Model.Environment
{
    public class MagneticFragment : MonoBehaviour
    {
        public float baseMagneticRadius = 10f;
        public float pullForce = 5f;
        public LayerMask magneticLayers;

        private MagneticFragmentGroupAI group;

        void Start()
        {
            if (group != null)
            {
                group.RegisterFragment(this);
            }
        }

        void FixedUpdate()
        {
            float multiplier = 1f;
            float radius = baseMagneticRadius;

            if (group != null)
            {
                group.RegisterFragment(this);
                int groupSize = group.fragments.Count;

                multiplier = Mathf.Pow(groupSize, 1.2f);
                radius = baseMagneticRadius * Mathf.Lerp(1f, 2f, groupSize / 10f);
            }

            Collider[] hits = Physics.OverlapSphere(transform.position, radius, magneticLayers);
            foreach (Collider hit in hits)
            {
                Rigidbody rb = hit.attachedRigidbody;
                if (rb == null || rb.gameObject == this.gameObject) continue;
                if (rb.CompareTag("IceAsteroid")) continue;

                Vector3 dir = (transform.position - rb.position).normalized;
                rb.AddForce(dir * pullForce * multiplier * Time.fixedDeltaTime, ForceMode.Acceleration);

                group?.RegisterOrbitingObject(rb);
            }

            transform.Rotate(Vector3.up * 60f * Time.deltaTime);
        }

        public void AssignGroup(MagneticFragmentGroupAI g)
        {
            group = g;
        }

        void OnDestroy()
        {
            group?.RemoveFragment(this);
        }

        void OnDrawGizmos()
        {
            float dynamicRadius = baseMagneticRadius;

            if (group != null)
            {
                int groupSize = group.fragments.Count;
                dynamicRadius = baseMagneticRadius * Mathf.Lerp(1f, 2f, groupSize / 10f);
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, dynamicRadius);
        }
    }
}

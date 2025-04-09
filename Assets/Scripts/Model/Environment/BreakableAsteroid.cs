using System.Collections;
using UnityEngine;


namespace Model.Environment
{
    public class BreakableAsteroid : Asteroid
    {
        public bool canBreak = true;
        public float minSize = 0.2f;
        public int numFragments = 7;
        public float explosionForce = 3f;
        public float speedMultiplier = 1;
        private Rigidbody rb;

        private bool canBreakAgain = false;
        private float breakCooldown = 0.5f;


        void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.velocity = Random.onUnitSphere * Random.Range(0.5f, 2f) * speedMultiplier;
            rb.angularVelocity = Random.insideUnitSphere * Random.Range(0.2f, 1f);

            StartCoroutine(EnableBreakingAfterDelay());
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.Sleep();

            }
            if (Input.GetKeyDown(KeyCode.K))
            {

                rb.ResetInertiaTensor();
            }

        }

        IEnumerator EnableBreakingAfterDelay()
        {
            yield return new WaitForSeconds(breakCooldown);
            canBreakAgain = true;
        }

        protected void OnCollisionStay(Collision collision)
        {
            //Debug.Log("Asteroid is touching: " + collision.gameObject.name);
            if (collision.gameObject.tag != "Asteroid" && canBreak && transform.localScale.x > minSize)
            {
                Debug.Log("Asteroid is breaking..");
                DestroyAsteroid();
            }
        }

        protected override void DestroyAsteroid()
        {

            if (!canBreak || transform.localScale.x <= minSize)
            {
                base.DestroyAsteroid();
                return;
            }

            if (canBreak && canBreakAgain)
            {
                for (int i = 0; i < numFragments; i++)
                {
                    Vector3 randomOffset = Random.insideUnitSphere * 0.3f;
                    GameObject fragment = Instantiate(gameObject, transform.position + randomOffset, Random.rotation, transform.parent);
                    fragment.tag = "Asteroid_Debri";

                    float newSize = transform.localScale.x * 0.5f;
                    fragment.transform.localScale = new Vector3(newSize, newSize, newSize);

                    Rigidbody fragRb = fragment.GetComponent<Rigidbody>();
                    if (fragRb != null)
                    {
                        fragRb.velocity = rb.velocity + Random.insideUnitSphere * 1.5f;
                        fragRb.angularVelocity = Random.insideUnitSphere * 2f;
                        fragRb.AddExplosionForce(explosionForce, transform.position, 2f);
                    }

                    BreakableAsteroid fragmentScript = fragment.GetComponent<BreakableAsteroid>();
                    if (fragmentScript != null)
                    {
                        fragmentScript.SetSize(newSize);
                    }
                }

                SpawnBreakEffect();

                base.DestroyAsteroid();
            }
        }


        private void SpawnBreakEffect()
        {
            Utils.DebugLog("Spawn Break Effect [Animation and Sound]");
        }

        public void SetSize(float newSize)
        {
            transform.localScale = new Vector3(newSize, newSize, newSize);
            if (newSize <= minSize)
            {
                canBreak = false; // Stops further breaking when too small
            }
        }
    }


}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model.Weapon
{
    public class LaserProjectile : MonoBehaviour
    {
        Ship shooter;

        public float speed = 300f;
        public float lifeTime = 3f;
        public float damage = 100f;

        public GameObject explosionEffect;

        protected Rigidbody rigidBody;

        protected bool isDestroyed = false;

        public void Setup(Ship shooter, float damage)
        {
            this.shooter = shooter;
            this.damage = damage;
        }

        private void Start()
        {
            rigidBody = GetComponent<Rigidbody>();

            Destroy(gameObject, lifeTime); // auto-destroy after some time
        }

        void FixedUpdate()
        {
            float speedPerFrame = speed * Time.deltaTime;
            rigidBody.MovePosition(transform.position + transform.forward * speedPerFrame);
            Debug.Log("Laser position " + transform.position);
        }

        void OnTriggerEnter(Collider other)
        {
            if (isDestroyed)
            {
                return;
            }

            IDamagable damagable = other.GetComponent<IDamagable>();
            if (damagable == null)
            {
                damagable = other.transform.root.gameObject.GetComponent<IDamagable>();
            }

            Debug.Log("Laser hit " + transform.position + $" {other.transform.name}");

            Vector3 hitPosition = other.bounds.center;
            Utils.DebugLog($"[LaserProjectile] hit by {other.tag}" + other.transform.name);
            //Debug.Log($"[PlayerLaserProjectile] hit by {other.tag}");

            if (damagable != null)
            {
                if (damagable.IsShooter(shooter))
                {
                    return;
                }

                SpawnExplosionEffect(hitPosition);
                damagable.TakeDamage(damage, shooter);
                Destroy(gameObject);
                isDestroyed = true;
                return;
            }

            // Just destroy if hit anything else, not destroyed by ice asteroid or storm
            if (!other.CompareTag("IceAsteroid") || !other.CompareTag("SpaceStorm"))
            {
                SpawnExplosionEffect(hitPosition);
                Destroy(gameObject);
                isDestroyed = true;
            }
        }

        void SpawnExplosionEffect(Vector3 hitPosition)
        {
            if (explosionEffect != null)
            {
                GameObject exp = Instantiate(explosionEffect, hitPosition, Quaternion.identity);
                Destroy(exp, 1f);
            }
        }

    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model.Environment;
using Controller;
using Manager;

namespace Model.Weapon
{
    public class LaserProjectile : MonoBehaviour
    {
        Ship shooter;

        public float speed = 300f;
        public float lifeTime = 3f;
        public float damage = 100f;

        private GameObject explosionEffect;

        public void Setup(Ship shooter, float damage)
        {
            this.shooter = shooter;
            this.damage = damage;
        }

        private void Start()
        {
            explosionEffect = GameManager.Instance.explosionEffect;

            Destroy(gameObject, lifeTime); // auto-destroy after some time
        }

        void Update()
        {
            float finalSpeed = speed;
            transform.position += transform.forward * finalSpeed * Time.deltaTime;
        }

        void OnTriggerEnter(Collider other)
        {
            IDamagable damagable = other.GetComponent<IDamagable>();
            if (damagable == null)
            {
                damagable = other.transform.root.gameObject.GetComponent<IDamagable>();
            }

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
                return;
            }

            // Just destroy if hit anything else, not destroyed by ice asteroid or storm
            if (!other.CompareTag("IceAsteroid") || !other.CompareTag("SpaceStorm"))
            {
                SpawnExplosionEffect(hitPosition);
                Destroy(gameObject);
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
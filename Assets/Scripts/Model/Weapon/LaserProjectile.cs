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
            if (other == this) return;

            Vector3 hitPosition = other.bounds.center;
            Utils.DebugLog($"[LaserProjectile] hit by {other.tag}" + other.transform.name);
            //Debug.Log($"[PlayerLaserProjectile] hit by {other.tag}");

            IDamagable damagable = other.GetComponent<IDamagable>();
            if (damagable != null)
            {
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

        void OnCollisionEnter(Collision collision)
        {
            Debug.Log($"[Laser] Hit: {collision.gameObject.name}");

            if (shooter == null)
            {
                Debug.LogWarning("Shooter is null on laser!");
                return;
            }

            if (collision.gameObject == shooter)
            {
                Debug.Log("Laser hit its own shooter. Ignoring.");
                return;
            }

            Ship ship = collision.gameObject.GetComponent<Ship>();
            if (ship != null)
            {
                ship.TakeDamage(damage, shooter);
            }

            if (collision.gameObject.CompareTag("Player"))
            {
                Debug.Log("Laser hit the player!");
            }

            Destroy(gameObject);
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
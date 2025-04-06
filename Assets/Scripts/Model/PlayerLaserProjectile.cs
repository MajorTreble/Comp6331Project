using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model.Environment;
using Controller;
using Manager;

namespace Model
{
    public class PlayerLaserProjectile : MonoBehaviour
    {
        public float speed = 400f;
        public float lifeTime = 3f;
        public float damage = 100f;

        private float playerSpeed = 0;

        private GameObject explosionEffect;

        private void Start()
        {
            explosionEffect = GameManager.Instance.explosionEffect;
            Destroy(gameObject, lifeTime); // auto-destroy after some time
        }

        void Update()
        {
            float finalSpeed = speed + playerSpeed;
            transform.position += transform.forward * finalSpeed * Time.deltaTime;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") || other.CompareTag("PlayerComponent") || other.transform == this.transform) return; // Don't hit self

            Vector3 hitPosition = other.bounds.center;
            Utils.DebugLog($"[PlayerLaserProjectile] hit by {other.tag}" + other.transform.name);
            // Damage Ship
            Ship ship = other.GetComponent<Ship>();
            if (ship != null)
            {
                ship.TakeDamage(damage);
                spawnExplosionEffect(hitPosition);
                Destroy(gameObject);
                return;
            }

            // Damage Asteroid
            BreakableAsteroid asteroid = other.GetComponent<BreakableAsteroid>();
            if (asteroid != null)
            {
                asteroid.ReceiveDamage(damage);
                spawnExplosionEffect(hitPosition);
                Destroy(gameObject);
                return;
            }

            // Just destroy if hit anything else, not destroyed by ice asteroid or storm
            if (!other.CompareTag("IceAsteroid") || !other.CompareTag("SpaceStorm")) {
                spawnExplosionEffect(hitPosition);
                Destroy(gameObject);
            }
            
        }

        void spawnExplosionEffect(Vector3 hitPosition) {
            if (explosionEffect != null)
            {
                GameObject exp = Instantiate(explosionEffect, hitPosition, Quaternion.identity);
                Destroy(exp, 1f);
            }
        }

        public void setPlayerSpeed(float playerSpeed)
        {
            this.playerSpeed = playerSpeed;
        }

        public void SetPlayerDamage(float playerDamage)
        {
            this.damage = playerDamage;

        }
    }

}
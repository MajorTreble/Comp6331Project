using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model.Environment;

namespace Model
{
    public class PlayerLaserProjectile : MonoBehaviour
    {
        public float speed = 100f;
        public float lifeTime = 3f;
        public float damage = 100f;

        private float playerSpeed = 0;

        private void Start()
        {
            Destroy(gameObject, lifeTime); // auto-destroy after some time
        }

        void Update()
        {
            float finalSpeed = speed + playerSpeed;
            transform.position += transform.forward * finalSpeed * Time.deltaTime;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") || other.CompareTag("SpaceshipComponent")) return; // Don't hit self

            Debug.Log($"[PlayerLaserProjectile] hit by {other.tag}");
            // Damage Ship
            Ship ship = other.GetComponent<Ship>();
            if (ship != null)
            {
                ship.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }

            // Damage Asteroid
            BreakableAsteroid asteroid = other.GetComponent<BreakableAsteroid>();
            if (asteroid != null)
            {
                asteroid.ReceiveDamage(damage);
                Destroy(gameObject);
                return;
            }

            // Just destroy if hit anything else
            Destroy(gameObject);
        }

        public void setPlayerSpeed(float playerSpeed)
        {
            this.playerSpeed = playerSpeed;
        }
    }

}
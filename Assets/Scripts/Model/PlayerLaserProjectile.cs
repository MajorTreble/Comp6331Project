using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model.Environment;
using Controller;

namespace Model
{
    public class PlayerLaserProjectile : MonoBehaviour
    {
        Ship owner;

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
            if (other.CompareTag("Player") || other.CompareTag("PlayerComponent") || other.transform == this.transform) return; // Don't hit self
            
            Utils.DebugLog($"[PlayerLaserProjectile] hit by {other.tag}" + other.transform.name);
            //Debug.Log($"[PlayerLaserProjectile] hit by {other.tag}");

            // Damage Ship
            IDamagable damagable = other.GetComponent<IDamagable>();
            if (damagable != null)
            {
                damagable.TakeDamage(damage, owner);
                return;
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
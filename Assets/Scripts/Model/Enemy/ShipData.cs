using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    
    [CreateAssetMenu(fileName = "Ship", menuName = "ScriptableObjects/Ship", order = 0)]
    public class ShipData : ScriptableObject
    {
        public float maxHealth = 100.0f;
		public float maxShields = 100.0f;
        public int maxAmmo = 100;
		public float shieldRegen = 10;

        public float acc = 1;
        public float maxSpeed = 50;
        public float turnSpeed = 10;

        public float attackCooldown = 3;
        public float laserDamage = 15;

        public void Reset()
        {
            maxHealth = 0f;
            maxShields = 0f;
            maxAmmo = 0;
            shieldRegen = 0;

            acc = 0;
            maxSpeed = 0;
            turnSpeed = 0;

            attackCooldown = 0;
            laserDamage = 0;
        }
    }

}

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
		public float shieldRegen = 10;

        public float acc = 1;
        public float maxSpeed = 50;
        public float turnSpeed = 10;

        public float attackCooldown = 3;
    }

}

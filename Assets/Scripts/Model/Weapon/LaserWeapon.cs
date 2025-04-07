using UnityEngine;

using Model.AI;

namespace Model.Weapon
{
    public class LaserWeapon : MonoBehaviour
    {
        public GameObject projectilePrefab;

        protected Ship shooter;
        protected float damage;

        public void Setup(Ship shooter)
        {
            this.shooter = shooter;
        }

        public void Setup(Ship shooter, float damage)
        {
            this.shooter = shooter;
            this.damage = damage;
        }

        public void Fire()
        {
            GameObject laserProjectile = Instantiate(projectilePrefab, transform.position, transform.rotation);
            laserProjectile.GetComponent<LaserProjectile>().Setup(shooter, damage);
        }
    }
}
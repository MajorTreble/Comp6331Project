using UnityEngine;


using Controller;

namespace Model.Environment
{
    public class MineableAsteroid : MonoBehaviour, IDamagable
    {
        [Header("Spinning")]
        public float spinSpeed = 10f;

        private float health = 50;

        void Update()
        {
            transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime, Space.World);
        }

        public bool IsShooter(Ship shooter)
        {
            return false;
        }

        public bool TakeDamage(float damage, Ship shooter)
        {
            health -= damage;

            return CheckDestroyed();
        }
        public bool CheckDestroyed()
        {
            if (health > 0)
                return false;

            DestroyAsteroid();

            return true;
        }

        void DestroyAsteroid()
        {
            JobController.Inst.OnObjDestroyed(transform.tag);
            gameObject.SetActive(false);
        }
    }
}

using UnityEngine;
using UnityEngine.Bindings;
using UnityEngine.Events;


namespace Model.Environment
{
    public class Asteroid : MonoBehaviour, IDamagable
    {
        public static event UnityAction<Asteroid> OnDestroyed;

        public float health = 50;

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

        protected virtual void DestroyAsteroid()
        {
            gameObject.SetActive(false);

            //OnDestroyed?.Invoke(this);
        }
    }
}

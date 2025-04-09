using UnityEngine;

using AI.Steering;
using Controller;
using Model.Weapon;

namespace Model
{

    public class Ship : MonoBehaviour, IDamagable
    {
        public ShipData oriData;
        public Rigidbody rb;
        public SteeringAgent steeringAgent = null;

        public int ammo;
        public float health;
        public float shields;
        public float maxHealth = 100f;
        public float maxShields = 0f;

        public Faction faction = null;

        public virtual void Start()
        {
            this.rb = GetComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            this.steeringAgent = GetComponent<SteeringAgent>();
            this.steeringAgent.rigidBody = rb;
        }

        public virtual void SetStats()
        {
            health = oriData.maxHealth;
            shields = oriData.maxShields;
        }

        public virtual bool IsJobTarget()
        {
            return false;
        }

        public virtual void ShieldRecover()
        {
            shields += oriData.shieldRegen;
            shields = Mathf.Clamp(shields, 0, oriData.maxShields);
        }

        public bool IsShooter(Ship shooter)
        {
            return this == shooter;
        }

        public virtual bool TakeDamage(float damage, Ship shooter)
        {
            if(shields > 0)
                shields -= damage;
            else
                health -= (damage + shields);

            return CheckDestroyed();
        }

        public virtual bool CheckDestroyed()
        {
            if (health > 0)
                return false;

            JobController.Inst.OnObjDestroyed(transform.tag);

            this.gameObject.SetActive(false);

            return true;
        }

        public virtual void Leave()
        {
            JobController.Inst.OnObjLeave(transform.tag);
            this.gameObject.SetActive(false);
        }


        public float immoDuration = 1.0f;//immortality
        private float immoCd = 0.0f;

        void OnCollisionEnter(Collision collision)
        {
            if (Time.time < immoCd) return;

            immoCd = Time.time + immoDuration;

            float dmgMult = 0.3f;
            float relativeVel = collision.relativeVelocity.magnitude;
            float dmg = relativeVel * dmgMult;

            health -= dmg;

            //Debug.Log($"[Colision DMG] Speed: {dmg} - Transform Name: {collision.transform.name} - Health: {health}");

            CheckDestroyed();
        }
    }
}
using UnityEngine;

using AI.Steering;
using Controller;

namespace Model
{

    public class Ship : MonoBehaviour
    {
        public ShipData oriData;
        public Rigidbody rb;
        public SteeringAgent steeringAgent = null;

        public int ammo;
        public float health;
        public float shields;

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

        public virtual bool TakeDamage(float _dmg)
        {
            health -= _dmg;

            return CheckDestroyed();
        }

        public bool CheckDestroyed()
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
    }
}
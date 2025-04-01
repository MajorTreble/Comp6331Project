using UnityEngine;

using Controller;

namespace Model
{

    public class Ship : MonoBehaviour
    {
        public ShipData oriData;

        public int ammo;
        public float health;
        public float shields;

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
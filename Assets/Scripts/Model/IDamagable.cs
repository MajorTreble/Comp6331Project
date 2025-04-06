using UnityEngine;

namespace Model
{

    public interface IDamagable
    {
        public bool TakeDamage(float damage, Ship attacker);
    }

}
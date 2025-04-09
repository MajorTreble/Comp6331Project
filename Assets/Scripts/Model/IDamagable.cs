using UnityEngine;

namespace Model
{

    public interface IDamagable
    {
        public bool IsShooter(Ship shooter);
        public bool TakeDamage(float damage, Ship shooter);
    }

}
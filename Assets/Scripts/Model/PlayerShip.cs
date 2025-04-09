using System.Runtime.InteropServices;
using UnityEngine;

using Controller;
using Model.Environment;
using Model.Weapon;
using Manager;

namespace Model
{

    public class PlayerShip : Ship
    {
        public LaserWeapon weapon_1;
        public LaserWeapon weapon_2;

        private float fireCooldown = 0f;
        private bool w1Shoot = false;

        //Upgrade System, get curr values for the ship
        public float CurrMaxHealth { get { return oriData.maxHealth + UpgradeController.Inst.upgrData.maxHealth; } }
        public float CurrMaxShields { get { return oriData.maxShields + UpgradeController.Inst.upgrData.maxShields; } }
        public int CurrMaxAmmo { get { return oriData.maxAmmo + UpgradeController.Inst.upgrData.maxAmmo; } }
        public float CurrShieldRegen { get { return oriData.shieldRegen + UpgradeController.Inst.upgrData.shieldRegen; } }
        public float CurrAcc { get { return oriData.acc + UpgradeController.Inst.upgrData.acc; } }
        public float CurrMaxSpeed { get { return oriData.maxSpeed + UpgradeController.Inst.upgrData.maxSpeed; } }
        public float CurrTurnSpeed { get { return oriData.turnSpeed + UpgradeController.Inst.upgrData.turnSpeed; } }
        public float CurrLaserDamage { get { return oriData.laserDamage + UpgradeController.Inst.upgrData.laserDamage; } }

        public void Awake()
        {
            this.faction = new Faction();
            this.faction.factionType = Faction.FactionType.Solo;

            SetupWeapons();

            SetStats();
        }

        public void Update()
        {
            ShieldRecover();

        }

        void LateUpdate()
        {
            AimWeapons();
        }

        void SetupWeapons()
        {
            weapon_1 = Utils.FindChildByName(this.transform, "Weapon1").GetComponent<LaserWeapon>();
            weapon_2 = Utils.FindChildByName(this.transform, "Weapon2").GetComponent<LaserWeapon>();

            if (weapon_1 == null || weapon_2 == null)
            {
                Debug.Log("weapon not found, again");
                Invoke("SetupWeapons", 0.3f);
                return;
            }


            float damage = 15.0f;
            if (UpgradeController.Inst != null)
            {
                damage = CurrLaserDamage;
            }

            weapon_1.Setup(this, damage);
            weapon_2.Setup(this, damage);
        }

        void AimWeapons()
        {
            if (Camera.main == null)
            {
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float distance = 250;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, distance))
            {
                if (!hit.transform.CompareTag("Player"))
                    distance = hit.distance;
            }

            Vector3 point = ray.GetPoint(distance);

            weapon_1.gameObject.transform.LookAt(point);
            weapon_2.gameObject.transform.LookAt(point);
        }

        public override void SetStats()
        {
            if (UpgradeController.Inst == null)
            {
                return;
            }

            health = CurrMaxHealth;
            shields = CurrMaxShields;
            ammo = CurrMaxAmmo;
        }

        public override void ShieldRecover()
        {
            if (UpgradeController.Inst == null)
            {
                return;
            }

            shields += CurrShieldRegen * Time.deltaTime;
            shields = Mathf.Clamp(shields, 0, CurrMaxShields);
        }

        public void FireLaser(float playerSpeed = 0)
        {
            if (Time.time < fireCooldown) return;

            LaserWeapon weapon = w1Shoot ? weapon_1 : weapon_2;
            weapon.Fire();

            w1Shoot = !w1Shoot;

            fireCooldown = Time.time + oriData.attackCooldown;
            ammo--;
        }

        public override bool TakeDamage(float damageAmount, Ship attacker)
        {
            bool destroyed = base.TakeDamage(damageAmount, attacker); // Use base ship behavior

            // Just to check if player ship is getting hit
            Debug.Log("[PlayerShip] Player took damage!");

            return destroyed;
        }

        public override bool CheckDestroyed()
        {
            bool isDestroyed = base.CheckDestroyed();

            if (isDestroyed) Leave();

            return isDestroyed;

        }

        public override void Leave()
        {
            base.Leave();
            JobController.Inst.LeaveMap();
        }
    }
}

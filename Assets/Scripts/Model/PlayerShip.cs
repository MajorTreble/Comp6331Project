using System.Runtime.InteropServices;
using Controller;
using Model.Environment;
using UnityEngine;
using Manager;

namespace Model
{

    public class PlayerShip : Ship
    {

        public GameObject laser = null;
        private GameObject laserPrefab;
        public Transform weapon_1;
        public float fireRate = 0.2f;
        private float fireCooldown = 0f;


        //Upgrade System, get curr values for the ship
        public float CurrMaxHealth { get { return oriData.maxHealth + UpgradeController.Inst.upgrData.maxHealth; } }
        public float CurrMaxShields { get { return oriData.maxShields + UpgradeController.Inst.upgrData.maxShields; } }
        public float CurrShieldRegen { get { return oriData.shieldRegen + UpgradeController.Inst.upgrData.shieldRegen; } }
        public float CurrAcc { get { return oriData.acc + UpgradeController.Inst.upgrData.acc; } }
        public float CurrMaxSpeed { get { return oriData.maxSpeed + UpgradeController.Inst.upgrData.maxSpeed; } }
        public float CurrTurnSpeed { get { return oriData.turnSpeed + UpgradeController.Inst.upgrData.turnSpeed; } }

        public void Awake()
        {
            this.faction = new Faction();
            this.faction.factionType = Faction.FactionType.Solo;

            GetWeapon();
            laserPrefab = GameManager.Instance.playerLaserPrefab;

            if (laserPrefab == null)
            {
                Debug.LogError("[PlayerShip] Player laser prefab is null.");
            }
        }

        public void Update()
        {
            ShieldRecover();
        }

        public override void SetStats()
        {
            health = CurrMaxHealth;
            shields = CurrMaxShields;
        }

        public override void ShieldRecover()
        {
            shields += CurrShieldRegen;
            shields = Mathf.Clamp(shields, 0, CurrMaxShields);
        }

        void GetWeapon()
        {
            weapon_1 = Utils.FindChildByName(this.transform, "Weapon1");

            if (weapon_1 == null)
            {
                Debug.Log("weapon not found, again");
                Invoke("GetWeapon", 0.3f);
                return;
            }

        }
        public void FireLaser(float playerSpeed = 0)
        {
            if (Time.time < fireCooldown) return;

            if (laserPrefab != null && weapon_1 != null)
            {
                GameObject laser = Instantiate(laserPrefab, weapon_1.position, weapon_1.rotation);
                PlayerLaserProjectile playerLaserProjectile = laser.GetComponent<PlayerLaserProjectile>();
                playerLaserProjectile.setPlayerSpeed(playerSpeed);
            }

            fireCooldown = Time.time + fireRate;
        }

        public override bool TakeDamage(float damageAmount)
        {
            bool destroyed = base.TakeDamage(damageAmount); // Use base ship behavior

            // Just to check if player ship is getting hit
            Debug.Log("[PlayerShip] Player took damage!");

            return destroyed;
        }

        public override void Leave()
        {
            base.Leave();
            JobController.Inst.LeaveMap();
        }
    }
}

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
        public Transform weapon_2;
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

            GetWeapon();
            laserPrefab = GameManager.Instance.playerLaserPrefab;

            if (laserPrefab == null)
            {
                Debug.LogError("[PlayerShip] Player laser prefab is null.");
            }

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

        void AimWeapons()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float distance = 250;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, distance))
            {
                if (!hit.transform.CompareTag("Player"))
                    distance = hit.distance;
            }

            Vector3 point = ray.GetPoint(distance);

            weapon_1.LookAt(point);
            weapon_2.LookAt(point);
        }

        public override void SetStats()
        {
            health = CurrMaxHealth;
            shields = CurrMaxShields;
            ammo = CurrMaxAmmo;
        }

        public override void ShieldRecover()
        {
            shields += CurrShieldRegen * Time.deltaTime;
            shields = Mathf.Clamp(shields, 0, CurrMaxShields);
        }

        void GetWeapon()
        {
            weapon_1 = Utils.FindChildByName(this.transform, "Weapon1");
            weapon_2 = Utils.FindChildByName(this.transform, "Weapon2");

            if (weapon_1 == null || weapon_2 == null)
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
                GameObject laser = Instantiate(laserPrefab);
                if (w1Shoot)
                {
                    laser.transform.position = weapon_1.position;
                    laser.transform.rotation = weapon_1.rotation;
                    w1Shoot = false;
                }
                else
                {
                    laser.transform.position = weapon_2.position;
                    laser.transform.rotation = weapon_2.rotation;
                    w1Shoot = true;
                }

                PlayerLaserProjectile playerLaserProjectile = laser.GetComponent<PlayerLaserProjectile>();

                playerLaserProjectile.SetPlayerProjectile(playerSpeed, CurrLaserDamage, this);
            }

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

        public override void Leave()
        {
            base.Leave();
            JobController.Inst.LeaveMap();
        }
    }
}

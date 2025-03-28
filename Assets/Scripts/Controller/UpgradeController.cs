using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Controller;
using Manager;
using Model;

namespace Controller
{
    public class UpgradeController : MonoBehaviour
    {
        public static UpgradeController Inst { get; private set; } //Singleton
        private void Awake()
        {
            if (Inst == null)
            {
                Inst = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }

        [System.Serializable]
        public class Upgrade
        {
            public string name = "placeholder_name";        
            public int lvl = 0; //1-10
            public int[] cost = new int[10];
            public float[] value = new float[10];

            public Upgrade(string name, int lvl, int[] cost, float[] value)
            {
                this.name = name;
                this.lvl = lvl;
                this.cost = cost;
                this.value = value;
            }
        }

        public List<Upgrade> upgrList = new List<Upgrade>();

        public void AddUpgrade(Upgrade up)
        {
            upgrList.Add(up);
        }

        public void UpdateValues()
        {
            for (int i = 0; i < upgrList.Count; i++)
            {
                UpdateValue(i);                
            }
        }

        void UpdateValue(int _entryIndex)
        {
            Upgrade up = upgrList[_entryIndex];
            PlayerController pc = GameManager.Instance.playerShip.GetComponent<PlayerController>();
            PlayerShip ship = GameManager.Instance.playerShip.GetComponent<PlayerShip>();
            float value = up.value[up.lvl];

            switch (up.name)
            {
                case "MaxHealth":
                    ship.maxHealth = value;
                    break;

                case "Ammo":                
                    ship.ammo = (int)value;
                    break;

                case "MaxShield":                
                    ship.maxShields = value;
                    break;

                case "Acc":                
                    pc.acc = value;
                    break;

                case "MaxSpeed":
                    pc.maxSpeed = value;
                    break;

                default:
                    Debug.Log("Error Change Value Upgrade");
                    break;
            }

        }
    }
}

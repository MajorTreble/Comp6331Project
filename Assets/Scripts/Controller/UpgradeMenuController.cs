using System.Collections;
using System.Collections.Generic;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.UI;



public class UpgradeMenuController : MonoBehaviour
{
    Transform upgradeList;

    void Start()
    {
        upgradeList = GameObject.Find("UpgradeList").transform;
    }
    public void UpgradeX(int _entryIndex)
    {
        int upgradeCost = -50;
        if(PlayerReputation.Inst.coins > upgradeCost)
        {
            Slider sld = Utils.FindChildByName(upgradeList.GetChild(_entryIndex), "Sld_Upgrade").GetComponent<Slider>(); 
            if(sld == null)
            {
                Debug.Log("upgrade slider not found");
                return;
            }

            if(sld.value < 1)
            {
                sld.value = sld.value + 0.1f;
                Debug.Log("UPGRADED!");
            }            
        }
    }
}

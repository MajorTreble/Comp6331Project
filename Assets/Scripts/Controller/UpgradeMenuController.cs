using System.Collections.Generic;
using System.Linq.Expressions;
using Controller;
using Manager;
using Model;
using UnityEngine;
using UnityEngine.UI;



public class UpgradeMenuController : MonoBehaviour
{
    public Transform upgradeList;

    public Text txt_Coins;

    public void Start()
    {
        CreateUpgrades();
        InstantiateUpgrades();

        UpgradeController.Inst.umc = this;

        upgradeList = GameObject.Find("UpgradeList").transform;

        txt_Coins = GameObject.Find("Txt_Coins").GetComponent<Text>();
        txt_Coins.text = "Coins:" + GameManager.Instance.reputation;
    }

    void CreateUpgrades()
    {
        UpgradeController uc = UpgradeController.Inst;
        if(uc.upgrList.Count > 0) return;

        uc.upgrList = new List<UpgradeController.Upgrade>();

        int[] cost = new int[]{100,150,225,337,506,759,1139,1708,2562,3844, 5766};//*1.5
        float[] valueMult = new float[]{1.0f, 1.3f,1.69f,2.2f,2.86f,3.71f,4.83f,6.27f,8.16f,10.6f,13.79f };//*1.3

        ShipData oriData = Resources.Load<ShipData>("Scriptable/Ships/Player");

        string upgrName = "MaxHealth";
        float baseValue = oriData.maxHealth;
        float[] value = new float[valueMult.Length];
        for (int i = 0; i < valueMult.Length; i++)
        {
            value[i] = baseValue * valueMult[i];            
        }
        uc.AddUpgrade(new UpgradeController.Upgrade(upgrName, 0, cost, value ));

        upgrName = "Ammo";
        //baseValue = ship.ammo;
        value = new float[valueMult.Length];
        for (int i = 0; i < valueMult.Length; i++)
        {
            value[i] = baseValue * valueMult[i];            
        }        
        uc.AddUpgrade(new UpgradeController.Upgrade(upgrName, 0, cost, value ));
       
        upgrName = "MaxShield";
        baseValue = oriData.maxShields;
        value = new float[valueMult.Length];
        for (int i = 0; i < valueMult.Length; i++)
        {
            value[i] = baseValue * valueMult[i];            
        }        
        uc.AddUpgrade(new UpgradeController.Upgrade(upgrName, 0, cost, value ));

        upgrName = "Acc";
        baseValue = oriData.acc;
        baseValue = 10;
        value = new float[valueMult.Length];
        for (int i = 0; i < valueMult.Length; i++)
        {
            value[i] = baseValue * valueMult[i];            
        }        
        uc.AddUpgrade(new UpgradeController.Upgrade(upgrName, 0, cost, value ));

        upgrName = "MaxSpeed";
        baseValue = oriData.maxSpeed;
        baseValue = 50;
        value = new float[valueMult.Length];
        for (int i = 0; i < valueMult.Length; i++)
        {
            value[i] = baseValue * valueMult[i];            
        }        
        uc.AddUpgrade(new UpgradeController.Upgrade(upgrName, 0, cost, value ));

        
    }

    void InstantiateUpgrades()
    {

        UpgradeController uc = UpgradeController.Inst;
        GameObject template = GameObject.Find("UpgradeEntryTemplate");
        
        for (int i = 0; i < uc.upgrList.Count; i++)
        {
            if(i>0)
                template = Instantiate(template, template.transform.parent);
            
           UpgradeController.Upgrade up = uc.upgrList[i];

            int lvl = uc.upgrList[i].lvl;
            Utils.FindChildByName(template.transform, "Sld_Upgrade").GetComponent<Slider>().value = 0.1f * lvl;
            Utils.FindChildByName(template.transform, "Txt_Cost").GetComponent<Text>().text = up.cost[lvl].ToString();
            Utils.FindChildByName(template.transform, "Txt_Name").GetComponent<Text>().text = up.name + "\n" + up.value[lvl].ToString();

            int index = i;
            Utils.FindChildByName(template.transform, "Btn_Upgrade").GetComponent<Button>().onClick.AddListener(() => UpgradeX(index));
        }
    }

    public void UpgradeX(int _entryIndex)
    {
        UpgradeController uc = UpgradeController.Inst;
        if(uc.upgrList[_entryIndex].lvl > 9) return;
        
        int upgradeCost = uc.upgrList[_entryIndex].cost[uc.upgrList[_entryIndex].lvl];

        PlayerReputation reputation = GameManager.Instance.reputation;
        if (reputation.coins > upgradeCost)
        {
            reputation.coins -= upgradeCost;
            uc.upgrList[_entryIndex].lvl += 1;
            UpdateList(_entryIndex);
        }
    }

    void UpdateList(int _entryIndex)
    {
        UpgradeController uc = UpgradeController.Inst;
        
        Transform entry = upgradeList.GetChild(_entryIndex);
        UpdateCoins();

        int lvl = uc.upgrList[_entryIndex].lvl;

        Utils.FindChildByName(entry, "Sld_Upgrade").GetComponent<Slider>().value = 0.1f * lvl;
        Utils.FindChildByName(entry, "Txt_Cost").GetComponent<Text>().text = uc.upgrList[_entryIndex].cost[lvl].ToString();
        Utils.FindChildByName(entry, "Txt_Name").GetComponent<Text>().text = uc.upgrList[_entryIndex].name + "\n" + uc.upgrList[_entryIndex].value[lvl].ToString();
    }    

    public void UpdateCoins()
    {
        txt_Coins.text = "Coins:" + GameManager.Instance.reputation.coins;
    }
}

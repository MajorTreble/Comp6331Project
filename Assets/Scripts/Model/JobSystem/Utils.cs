using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils 
{
    public static Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result = FindChildByName(child, name);
            if (result != null)
                return result;
        }
        return null;
    }  

    public static void ProgenyPulverizer(GameObject _father)
    {
        //Not the best solution but the easier at the moment;
        //If possible change it to pattern
        for (int i = 0; i < _father.transform.childCount; i++)
        {
            GameObject.Destroy(_father.transform.GetChild(i).gameObject);                 
        }

    }

    public static void DebugLog(string s)
    {
        bool debugOn = false;

        if(debugOn == false) return;

        Debug.Log(s);
    }
    
}

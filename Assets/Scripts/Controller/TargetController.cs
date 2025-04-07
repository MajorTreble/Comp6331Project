using System.Collections;
using System.Collections.Generic;

using Controller;
using Manager;
using Model;
using Model.AI;

using UnityEngine;
using UnityEngine.UI;

public class TargetController : MonoBehaviour
{
    public static TargetController Inst { get; private set; } //Singleton

    Vector2 targetOriSize;
    Vector2 hostileTargetOriSize;


    public bool showPortal;


    private void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
        }
        else Destroy(gameObject);


        showPortal = true;
    }




    public List<Transform> targets;
    public List<Image> targetsIcon;

    public List<AIShip> hostileTargets;
    public List<Image> hostileTargetsIcon;

    void Start()
    {
       

        Invoke("InstantiateTargets",1);
    }


    void InstantiateTargets()
    {
        showPortal = false;

        GameObject template = GameObject.Find("TargetTemplate");
        GameObject hostileTemplate = GameObject.Find("HostileTargetTemplate");

        if (template == null)
		{
            return;
		}

        GameObject go = template;
        targetOriSize = template.GetComponent<Image>().rectTransform.sizeDelta;
        hostileTargetOriSize = hostileTemplate.GetComponent<Image>().rectTransform.sizeDelta;

        if (JobController.Inst.currJob.jobType == JobType.Deliver)
        {
            targets.Add(GameManager.Instance.portal.transform); 
            showPortal = true;
        }else if(JobController.Inst.currJob.jobType == JobType.Mine)
        {
            foreach (GameObject s in SpaceEnvironmentController.Instance.activeMineableAsteroids)
            {                
                targets.Add(s.transform);            
            } 
        }else //HUNT AND DEFEND
        {
            foreach (Ship s in SpawningManager.Instance.shipList)
            {
                if(s is PlayerShip) continue;

                hostileTargets.Add(s.GetComponent<AIShip>());
                GameObject icon = Instantiate(hostileTemplate, hostileTemplate.transform.parent);
                icon.SetActive(false);
                hostileTargetsIcon.Add(icon.GetComponent<Image>());
                

                if (s.CompareTag(JobUtil.ToTag(JobController.Inst.currJob.jobTarget)))
                    targets.Add(s.transform);            
            }            
        }
        
        
        for (int i = 0; i < targets.Count; i++)
        {
            if(i>0)
                go = Instantiate(template, template.transform.parent);
            
            targetsIcon.Add(go.GetComponent<Image>());            
        }
    }

    void Update()
    {
        if(targets.Count == 0) return;

        if(showPortal)
        {            
            foreach (Image t in targetsIcon)
            {
                t.gameObject.SetActive(false);                
            }

            targetsIcon[0].gameObject.SetActive(true);
            SetTarget(GameManager.Instance.portal.transform.position, targetsIcon[0], targetOriSize);
        }else
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if(targets[i].gameObject.activeSelf)
                    SetTarget(targets[i].position, targetsIcon[i], targetOriSize);     
                else targetsIcon[i].gameObject.SetActive(false);
            }  
        }

        for (int i = 0; i < hostileTargets.Count; i++)
        {
            if (hostileTargets[i].transform.gameObject.activeSelf)
            {
                if (AIHelper.IsHostile(hostileTargets[i], GameManager.Instance.playerShip.GetComponent<PlayerShip>()))
                {
                    SetTarget(hostileTargets[i].transform.position, hostileTargetsIcon[i], hostileTargetOriSize);
                    hostileTargetsIcon[i].gameObject.SetActive(true);
                }
            }
            else hostileTargetsIcon[i].gameObject.SetActive(false);
        }
    }

    void SetTarget(Vector3 _pos, Image _icon, Vector2 _size)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(_pos);

        if (screenPos.z < 0)
        {
            screenPos.x = Screen.width - screenPos.x;
            screenPos.y = Screen.height - screenPos.y;
            
            screenPos.x = screenPos.x < Screen.width / 2 ? 0 : Screen.width;
            screenPos.y = screenPos.y < Screen.height / 2 ? 0 : Screen.height;
        }

    
        screenPos.x = Mathf.Clamp(screenPos.x, 0, Screen.width);
        screenPos.y = Mathf.Clamp(screenPos.y, 0, Screen.height);
        screenPos.z = 0;
        _icon.transform.position = screenPos;
        _icon.enabled = true;

        float dist = Vector3.Distance(Camera.main.transform.position, _pos);

        float scale = Mathf.Clamp(100 / dist, 0.25f, 1f); 
        Vector2 newSize = Vector2.Lerp(_size * 0.25f, _size, scale);

        _icon.rectTransform.sizeDelta = newSize;
    }
}

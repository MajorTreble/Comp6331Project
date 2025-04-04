using System.Collections;
using System.Collections.Generic;
using Controller;
using Manager;

using Model;

using UnityEngine;
using UnityEngine.UI;

public class TargetController : MonoBehaviour
{
    public static TargetController Inst { get; private set; } //Singleton

    Vector2 targetOriSize;


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

    void Start()
    {
       

        Invoke("InstantiateTargets",1);
    }


    void InstantiateTargets()
    {
        showPortal = false;

        GameObject template = GameObject.Find("TargetTemplate");
        
        if (template == null)
		{
            return;
		}

        GameObject go = template;
        targetOriSize = template.GetComponent<Image>().rectTransform.sizeDelta;

        if(JobController.Inst.currJob.jobType == JobType.Deliver)
        {
            targets.Add(GameManager.Instance.portal.transform); 
            showPortal = true;
        }else if(JobController.Inst.currJob.jobType == JobType.Mine)
        {
            foreach (GameObject s in SpaceEnvironmentController.Instance.activeObjects)
            {                
                targets.Add(s.transform);            
            } 
        }else //HUNT AND DEFEND
        {
            foreach (Ship s in SpawningManager.Instance.shipList)
            {
                if(s is PlayerShip) continue;

                if(s.CompareTag(JobUtil.ToTag(JobController.Inst.currJob.jobTarget)))
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
            SetTarget(GameManager.Instance.portal.transform.position, targetsIcon[0]);
        }else
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if(targets[i].gameObject.activeSelf)
                    SetTarget(targets[i].position, targetsIcon[i]);     
                else targetsIcon[i].gameObject.SetActive(false);
            }  
        }        
    }

    void SetTarget(Vector3 _pos, Image _icon)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(_pos);

        if (screenPos.z > 0)
        {
            screenPos.x = Mathf.Clamp(screenPos.x, 0, Screen.width);
            screenPos.y = Mathf.Clamp(screenPos.y, 0, Screen.height);
            _icon.transform.position = screenPos;
            _icon.enabled = true;

            float dist = Vector3.Distance(Camera.main.transform.position, _pos);

            float scale = Mathf.Clamp(100 / dist, 0.25f, 1f); 
            Vector2 newSize = Vector2.Lerp(targetOriSize*0.25f, targetOriSize, scale);

            _icon.rectTransform.sizeDelta = newSize;
        }
        else
        {
            _icon.enabled = false;
        }

    

    }
}

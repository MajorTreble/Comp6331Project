using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Manager;
using Controller;
using Model;
using System;

public class JobView : MonoBehaviour
{


    Transform jobOptions;
    GameObject jobTemplate;
    Text jobDescription;
    Text jobFeedback;


    public Transform reputationList;

    public int jobIndex = -1;

    public static JobView Inst { get; private set; } //Singleton
    private void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);        
    }
    
    public void SetTestButtons()
    {   
        GameObject go =  GameObject.Find("JobControl_For_Tests");

        if(go==null) return;

        Transform jobControl = go.transform;

        Utils.FindChildByName(jobControl, "LeaveTargetMap").GetComponent<Button>().onClick.AddListener(JobController.Inst.LeaveTargetMap);
        Utils.FindChildByName(jobControl, "DestroyTarget").GetComponent<Button>().onClick.AddListener(JobController.Inst.DestroyTarget);
    }


    void Start()
    {
        SetTestButtons();        
        UpdateJob();  
    }

    public void LookForJobFeeback()
    {
        GameObject go = GameObject.Find("JobFeedbackText");
        if(go != null)
            jobFeedback = go.GetComponent<Text>();
    }

   

    
    public void ViewJob(int _index)
    {
        jobIndex = _index;
        ViewJob(JobMenuController.Inst.jobs[jobIndex]);
    }
    

    public void ViewJob(Job job)
    {

        GameObject go = GameObject.Find("JobDescriptionText");
        if(go!=null) jobDescription = go.GetComponent<Text>();  

        if(job == null)
        {
            jobDescription.text = "";
            return;
        } 
            
                
        string jobText = "";
        jobText += "Name: " + job.name + "\n";            
        jobText += "Map: " + job.scenario.mapName + "\n";

        jobText += "Type: " + job.jobType + "\n";            
        jobText += "Target: " + job.jobTarget + "\n";                 
        jobText += "Quantity: " + job.quantity + "\n";

        jobText += "Coins: " + job.rewardCoins + "\n";
        jobText += "Reputation: " + job.allyFaction.title + "\n";
        jobText += "Rep Reward " + job.rewardRep + "\n";  

        if(JobController.Inst.jobStatus == JobStatus.Failed) 
        {
            jobDescription.color = Color.red;
        }
        else if(JobController.Inst.jobStatus == JobStatus.Concluded)
        {
            jobDescription.color = Color.green;
        }else 
            jobDescription.color = Color.white;


        jobDescription.text = jobText;
        
    }

    public void ListJobs()
    {
        JobMenuController.Inst.LoadJobs();
        
        jobTemplate = GameObject.Find("JobTemplate");
        GameObject jobButton = jobTemplate;
        
        for (int i = 0; i < JobMenuController.Inst.jobs.Length; i++)
        {
            if(i>0)
                jobButton = Instantiate(jobTemplate, jobTemplate.transform.parent);
            
            ButtonJobHandler handler = jobButton.GetComponent<ButtonJobHandler>();

            handler.SetJobButton(i, JobMenuController.Inst.jobs[i].jobName,  JobMenuController.Inst.jobs[i].jobType);
        }
    }

    public void ListReputations()
    {
        GameObject template = GameObject.Find("ReputationTemplate");
        GameObject go = template;

        //Debug.Log(PlayerReputation.Inst.reputations);        
        for (int i = 0; i < GameManager.Instance.reputation.reputations.Count; i++)
        {            
            if(i>0)
                go = Instantiate(template, template.transform.parent);
        }
        

        UpdateReputations();
    }

    void UpdateReputations()
    {
        for (int i = 0; i < GameManager.Instance.reputation.reputations.Count; i++)
        {
            UpdateReputation(i);            
        }
    }

    void UpdateReputation(int _index)
    {
        reputationList = GameObject.Find("ReputationList").transform;
        Transform tr = reputationList.GetChild(_index);
        Reputation rep = GameManager.Instance.reputation.reputations[_index];

        Slider sld = Utils.FindChildByName(tr, "Sld_Value").GetComponent<Slider>();
        sld.maxValue = 1000;
        sld.value = Mathf.Abs(rep.value);

        if(rep.value > 0)
        {
            Utils.FindChildByName(sld.transform, "Fill").GetComponent<Image>().color = Color.green;
            sld.direction = Slider.Direction.LeftToRight;
        }else
        {
            Utils.FindChildByName(sld.transform, "Fill").GetComponent<Image>().color = Color.red;
            sld.direction = Slider.Direction.RightToLeft;
        } 
        
        Text txt = Utils.FindChildByName(tr, "Txt_Name").GetComponent<Text>();
        txt.text = rep.fac.title + " [" + rep.value + "]";
    }


    public void AcceptJob()
    {
        if(JobController.Inst.currJob == null)
            JobMenuController.Inst.AcceptJob(jobIndex);

        UpdateButtons();
    }

    public void FinishJob()
    {
        if(JobController.Inst.currJob != null)
            JobMenuController.Inst.FinishJob();
        UpdateButtons();
        UpdateReputations();
    }

    public void Departure()
    {
        if(JobController.Inst.currJob != null)
		{
            //if (GameManager.Instance.SelectScenario(JobController.Inst.currJob.jobType, ScenarioDifficulty.Easy))
			//{
            GameManager.Instance.StartScenario();
            //}
        }
        UpdateButtons();
    }

    public void UpdateButtons()
    {        
        JobController jc = JobController.Inst;
        if(jobOptions == null) 
        {
            GameObject go = GameObject.Find("JobOptions");
            if(go!= null)
                jobOptions = go.transform;
            else
                return;
        }

        Utils.FindChildByName(jobOptions, "AcceptJob").GetComponent<Button>().interactable = (jc.currJob == null);
        Utils.FindChildByName(jobOptions, "FinishJob").GetComponent<Button>().interactable = (jc.currJob != null);        
        Utils.FindChildByName(jobOptions, "Departure").GetComponent<Button>().interactable = (jc.jobStatus == JobStatus.InProgress);
    }


    public void UpdateJob()
    {        
        UpdateButtons();

        JobController jc = JobController.Inst;
        if(jobFeedback==null) LookForJobFeeback();
        if(jobFeedback==null) 
        {
            Invoke("UpdateJob", 0.5f);
            return;
        }
        
        if(jc.currJob == null) 
        {
            jobFeedback.text = " ";
            return;
        }
        
        string jobText = "";

        switch (jc.currJob.jobType)
        {
            case JobType.Defend:
                jobText += "Defend";
            break;
            case JobType.Deliver:
                jobText += "Deliver";
                break;
            case JobType.Hunt:
                jobText += "Destroy";
                break;
            case JobType.Mine:
                jobText += "Mine";
                break;
            default:
                break;
        }

        jobText += " " + jc.currJob.quantity;
                  
        jobText += " " + jc.currJob.jobTarget;   

        jobText += jc.currJob.quantity > 1? "s": ""; 

        jobText += " - " + jc.currJobQtd + "/" + jc.currJob.quantity;
        

        Color color = Color.white;
        if(jc.jobStatus == JobStatus.Failed) 
        {
            color = Color.red;
            jobText += " [F]";
        }
        if(jc.jobStatus == JobStatus.Concluded)
        {
            color = Color.green;
            jobText += " [C]";
        }        

        if (jobFeedback)
        {
            jobFeedback.color = color;
            jobFeedback.text = jobText;
        }
        
    }

}

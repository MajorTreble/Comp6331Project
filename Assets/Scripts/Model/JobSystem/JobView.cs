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

    int jobIndex = -1;

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
        //Utils.FindChildByName(jobControl, "MineOre").GetComponent<Button>().onClick.AddListener(JobController.Inst.MineOre);        
        //Utils.FindChildByName(jobControl, "LeaveMap").GetComponent<Button>().onClick.AddListener(JobController.Inst.LeaveMap);
    }


    void Start()
    {
        SetTestButtons();

        jobOptions = GameObject.Find("JobOptions").transform;
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
        jobText += "Reputation: " + job.rewardType + "\n";
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
        string jobText = "";
        int index = 0;
        foreach (Job job in JobMenuController.Inst.jobs)
        {
            jobText += "["+ index++ +"]"+ "\t"; 
            jobText += "Name: " + job.name + "\t";            
            jobText += "Map: " + job.scenario.mapName + "\n";

            jobText += "Type: " + job.jobType + "\t";            
            jobText += "Target: " + job.jobTarget + "\t";                 
            jobText += "Quantity: " + job.quantity + "\n";

            jobText += "Coins: " + job.rewardCoins + "\t";
            jobText += "Reputation: " + job.rewardType + "\t";
            jobText += " " + job.rewardRep + "\n\n";            
        }
        //print(jobText);
        
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
    }

    public void Departure()
    {
        if(JobController.Inst.currJob != null)
            GameManager.Instance.StartScenario();
        UpdateButtons();
    }

    public void UpdateButtons()
    {        
        JobController jc = JobController.Inst;
        if(jobOptions == null) return;

        Utils.FindChildByName(jobOptions, "AcceptJob").GetComponent<Button>().interactable = (jc.currJob == null);
        Utils.FindChildByName(jobOptions, "FinishJob").GetComponent<Button>().interactable = (jc.currJob != null);        
        Utils.FindChildByName(jobOptions, "Departure").GetComponent<Button>().interactable = (jc.currJob != null);
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
        
        jobText += "Type: " + jc.currJob.jobType + "\n";            
        jobText += "Target: " + jc.currJob.jobTarget + "\n";                 
        jobText += "Quantity: " + jc.currJobQtd + "/" + jc.currJob.quantity;
        //jobText += jc.jobStatus;

        Color color = Color.white;
        if(jc.jobStatus == JobStatus.Failed) 
        {
            color = Color.red;
            jobText += "[F]";
        }
        if(jc.jobStatus == JobStatus.Concluded)
        {
            color = Color.green;
            jobText += "[C]";
        }        

        if (jobFeedback)
        {
            jobFeedback.color = color;
            jobFeedback.text = jobText;
        }
        
        //print(jobText);
    }
}

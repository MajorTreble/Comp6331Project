using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Manager;
using Controller;
using Model;
using System;
using System.Dynamic;

public class JobView : MonoBehaviour
{

    bool oldJobVersion = false;


    Transform jobOptions;
    GameObject jobTemplate;
    //Transform jobTarget;
    Text jobDescription;
    Text jobFeedback;


    Faction[] allFac;
    string[] allDif;
    JobType[] allJobType;
    Scenario[] allScen;


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


    public void Start()
    {
        SetTestButtons();
        UpdateJob();  
    }

    public void SetConfigurations()
    {
        Transform jobConfigurations = GameObject.Find("JobConfigurations").transform;
        Job customJob = JobController.Inst.customJob;

        allFac = Resources.LoadAll<Faction>("Scriptable/Faction");
        allDif = new string[] { "Easy", "Medium", "Hard" };
        allJobType = (JobType[])System.Enum.GetValues(typeof(JobType));
        allScen = Resources.LoadAll<Scenario>("Scriptable/Scenario");

        Transform config = Utils.FindChildByName(jobConfigurations, "JobAlly").transform; 
        Dropdown dd = Utils.FindChildByName(config, "drop").GetComponent<Dropdown>();
        dd.onValueChanged.AddListener(UpdateJobConfiguration);
        dd.ClearOptions();
        foreach (Faction faction in allFac)
        {
            dd.options.Add(new Dropdown.OptionData(faction.title));
        }
        dd.value = (int)customJob.allyFaction.factionType;
        dd.RefreshShownValue(); 

        config = Utils.FindChildByName(jobConfigurations, "JobDifficulty").transform; 
        dd = Utils.FindChildByName(config, "drop").GetComponent<Dropdown>();
        dd.onValueChanged.AddListener(UpdateJobConfiguration);
        dd.ClearOptions();
        foreach (String dif in allDif)
        {
            dd.options.Add(new Dropdown.OptionData(dif));
        }                
        dd.value = (int)customJob.dangerValue;
        dd.RefreshShownValue(); 

        config = Utils.FindChildByName(jobConfigurations, "JobType").transform; 
        dd = Utils.FindChildByName(config, "drop").GetComponent<Dropdown>();
        dd.onValueChanged.AddListener(UpdateJobConfiguration);
        dd.ClearOptions();
        foreach (JobType type in allJobType)
        {
            dd.options.Add(new Dropdown.OptionData(type.ToString()));
        }        
        dd.value = (int)customJob.jobType;
        dd.RefreshShownValue(); 
        

        config = Utils.FindChildByName(jobConfigurations, "JobEnemy").transform; 
        //jobTarget = config;
        dd = Utils.FindChildByName(config, "drop").GetComponent<Dropdown>();
        dd.onValueChanged.AddListener(UpdateJobConfiguration);
        dd.ClearOptions();
        foreach (Faction faction in allFac)
        {
            dd.options.Add(new Dropdown.OptionData(faction.title));
        }        
        dd.value = (int)customJob.enemyFaction.factionType;
        dd.RefreshShownValue(); 


        config = Utils.FindChildByName(jobConfigurations, "JobScenario").transform; 
        dd = Utils.FindChildByName(config, "drop").GetComponent<Dropdown>();
        dd.onValueChanged.AddListener(UpdateJobConfiguration);
        dd.ClearOptions();        
        foreach (Scenario scenario in allScen)
        {
            dd.options.Add(new Dropdown.OptionData(scenario.mapName));
        }
        for (int i = 0; i < dd.options.Count; i++)
        {
            if (dd.options[i].text == customJob.scenario.name)
            {
                dd.value = i;
            }
        }
        dd.RefreshShownValue(); 
    }

    
    void UpdateJobConfiguration(int value)
    {
        Transform departure = GameObject.Find("Departure").transform;
        bool same = GetOptionValue("JobAlly") == GetOptionValue("JobEnemy");
        string text = same ? "Ally and Enemy can not be the same": "Departure";   
        
        departure.GetComponent<Button>().interactable = !same;
        departure.GetChild(0).GetComponent<Text>().text = text;
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
        if(!oldJobVersion) return;

        GameObject go = GameObject.Find("JobDescriptionText");
        if(go!=null) jobDescription = go.GetComponent<Text>();  

        if(job == null)
        {
            jobDescription.text = "";
            return;
        }    

        string jobText = "";
        if(oldJobVersion)
        {
            jobText += "Name: " + job.name + "\n";            
            jobText += "Map: " + job.scenario.mapName + "\n";

            jobText += "Type: " + job.jobType + "\n";            
            jobText += "Target: " + job.jobTarget + "\n";                 
            jobText += "Quantity: " + job.quantity + "\n";

            jobText += "Coins: " + job.rewardCoins + "\n";
            jobText += "Reputation: " + job.allyFaction.title + "\n";
            jobText += "Rep Reward " + job.rewardRep + "\n";  
        }else
        {    
            JobController jc = JobController.Inst;

            switch (job.jobType)
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

            jobText += " " + job.quantity;
                    
            jobText += " " + job.jobTarget;   

            jobText += job.quantity > 1? "s": ""; 

            jobText += " - " + jc.currJobQtd + "/" + job.quantity;
            

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
        if(!oldJobVersion) return;

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

    public void UpdateReputations()
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

    int GetOptionValue(string option)
    {
        Transform jobConfigurations = GameObject.Find("JobConfigurations").transform;
        Transform config = Utils.FindChildByName(jobConfigurations, option).transform;
        Dropdown dd = Utils.FindChildByName(config, "drop").GetComponent<Dropdown>();
        return dd.value;

    }


    public void AcceptJob()
    {        
        if(oldJobVersion)
        {
            if(JobController.Inst.currJob == null)
                JobMenuController.Inst.AcceptJob(jobIndex);
        }else
        {
            if(JobController.Inst.currJob == null)
            {
                JobController.Inst.customJob.allyFaction = allFac[GetOptionValue("JobAlly")];
                JobController.Inst.customJob.dangerValue = 1+GetOptionValue("JobDifficulty");
                JobController.Inst.customJob.jobType = allJobType[GetOptionValue("JobType")];
                JobController.Inst.customJob.enemyFaction = allFac[GetOptionValue("JobEnemy")];
                JobController.Inst.customJob.scenario = allScen[GetOptionValue("JobScenario")];                   

                switch (JobController.Inst.customJob.dangerValue)
                {
                    case 1:
                    JobController.Inst.customJob.rewardCoins = 100;
                    JobController.Inst.customJob.rewardRep = 75;
                    JobController.Inst.customJob.quantity = 1;
                    if(JobController.Inst.customJob.jobType == JobType.Mine)
                        JobController.Inst.customJob.quantity = 5;
                    break;                    
                    case 2:
                    JobController.Inst.customJob.rewardCoins = 250;
                    JobController.Inst.customJob.rewardRep = 17;
                    JobController.Inst.customJob.quantity = 3;
                    if(JobController.Inst.customJob.jobType == JobType.Mine)
                        JobController.Inst.customJob.quantity = 10;
                    break;
                    case 3:
                    JobController.Inst.customJob.rewardCoins = 625;
                    JobController.Inst.customJob.rewardRep = 400;
                    JobController.Inst.customJob.quantity = 5;//hunt or defend                    
                    if(JobController.Inst.customJob.jobType == JobType.Mine)
                        JobController.Inst.customJob.quantity = 15;
                        
                    break;
                    default:
                    Debug.LogWarning("Error on danger value");
                    break;
                }

                JobTarget target = JobTarget.Self;
                switch (JobController.Inst.customJob.jobType)
                {
                    case JobType.Defend:
                    target = (JobTarget)(int)JobController.Inst.customJob.allyFaction.factionType;
                    break;
                    case JobType.Deliver:
                    target = JobTarget.Self; 
                    JobController.Inst.customJob.quantity = 1;
                    break;
                    case JobType.Hunt:
                    target = (JobTarget)(int)JobController.Inst.customJob.enemyFaction.factionType;
                    break;
                    case JobType.Mine:
                    target = JobTarget.Asteroid;
                    break;
                    default:
                    Debug.LogWarning("Error on Job Type");
                    break;
                }

                JobController.Inst.customJob.jobTarget = target;


                JobMenuController.Inst.AcceptJob(JobController.Inst.customJob);
            }
        } 
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
        AcceptJob();
        if(JobController.Inst.currJob != null)
		{
            GameManager.Instance.StartScenario();         
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

        if(jc.currJob.jobType != JobType.Deliver)
        {
            jobText += " " + jc.currJob.quantity;
                  
            jobText += " " + jc.currJob.jobTarget;   

            jobText += jc.currJob.quantity > 1? "s": ""; 

            jobText += " - " + jc.currJobQtd + "/" + jc.currJob.quantity;
        }
        

        Color color = Color.white;
        if(jc.jobStatus == JobStatus.Failed) 
        {
            color = Color.red;
            jobText += " [F]";
        }else if(jc.jobStatus == JobStatus.Concluded)
        {
            color = Color.green;
            jobText += " [C]";
        }else if(GameManager.Instance.playerShip.GetComponent<PlayerShip>().ammo < 1)
        {
            color = Color.yellow;
            jobText += "\n No ammo, press [G] to Give Up Job";
        }

        if (jobFeedback)
        {
            jobFeedback.color = color;
            jobFeedback.text = jobText;
        }

        
    }

}

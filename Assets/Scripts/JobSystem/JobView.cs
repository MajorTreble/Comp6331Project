using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Manager;

public class JobView : MonoBehaviour
{


    Transform jobControllerObject;
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
    


    void Start()
    {
        DontDestroyOnLoad(this);

        jobControllerObject = GameObject.Find("Canvas").transform;
        jobTemplate = GameObject.Find("JobTemplate");
        jobDescription = GameObject.Find("JobDescriptionText").GetComponent<Text>();
        jobFeedback = GameObject.Find("JobFeedbackText").GetComponent<Text>();

    }

    

    public void ViewJob(int _index)
    {
        jobIndex = _index;

        
        JobController jc = JobController.Inst;
        string jobText = "";
        jobText += "Name: " + jc.jobs[jobIndex].name + "\n";            
        jobText += "Map: " + jc.jobs[jobIndex].mapModel.mapName + "\n";

        jobText += "Type: " + jc.jobs[jobIndex].jobType + "\n";            
        jobText += "Target: " + jc.jobs[jobIndex].jobTarget + "\n";                 
        jobText += "Quantity: " + jc.jobs[jobIndex].quantity + "\n";

        jobText += "Coins: " + jc.jobs[jobIndex].rewardCoins + "\n";
        jobText += "Reputation: " + jc.jobs[jobIndex].rewardType + "\n";
        jobText += "Rep Reward " + jc.jobs[jobIndex].rewardRep + "\n";  

        jobDescription.text = jobText;
        
    }

    public void ListJobs()
    {
        string jobText = "";
        int index = 0;
        foreach (JobModel job in JobController.Inst.jobs)
        {
            jobText += "["+ index++ +"]"+ "\t"; 
            jobText += "Name: " + job.name + "\t";            
            jobText += "Map: " + job.mapModel.mapName + "\n";

            jobText += "Type: " + job.jobType + "\t";            
            jobText += "Target: " + job.jobTarget + "\t";                 
            jobText += "Quantity: " + job.quantity + "\n";

            jobText += "Coins: " + job.rewardCoins + "\t";
            jobText += "Reputation: " + job.rewardType + "\t";
            jobText += " " + job.rewardRep + "\n\n";            
        }
        //print(jobText);
        
        GameObject jobButton = jobTemplate;
        for (int i = 0; i < JobController.Inst.jobs.Length; i++)
        {
            if(i>0)
                jobButton = Instantiate(jobTemplate, jobTemplate.transform.parent);
            
            ButtonJobHandler handler = jobButton.GetComponent<ButtonJobHandler>();

            handler.SetJobButton(i, JobController.Inst.jobs[i].jobName,  JobController.Inst.jobs[i].jobType);
        }
    }

    public void AcceptJob()
    {
        JobController.Inst.AcceptJob(jobIndex);
    }

    public void FinishJob()
    {
        JobController.Inst.FinishJob();
    }

    public void Departure()
    {
        print("Departure");

        GameManager.Instance.StartScenario();
    }



    public void UpdateJob()
    {        
        JobController jc = JobController.Inst;
        if(jc.currJob == null) 
        {
            jobFeedback.text = " ";
            return;
        }
        string jobText = "";
        
        jobText += "Type: " + jc.currJob.jobType + "\t";            
        jobText += "Target: " + jc.currJob.jobTarget + "\t";                 
        jobText += "Quantity: " + jc.currJobQtd + "/" + jc.currJob.quantity + "\n";
        jobText += jc.jobStatus;

        if(jc.jobStatus == JobController.JobStatus.Failed) jobText += "[COLOR RED]";
        if(jc.jobStatus == JobController.JobStatus.Concluded) jobText += "[COLOR GREEN]";

        if (jobFeedback)
            jobFeedback.text = jobText;
        
        //print(jobText);
    }
}

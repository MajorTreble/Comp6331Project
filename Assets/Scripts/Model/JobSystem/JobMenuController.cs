using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Controller;
using Model;

public enum JobStatus {NotSelected, InProgress, Failed, Concluded};
public class JobMenuController : MonoBehaviour
{
    public Job[] jobs;   
    
    
    

    public static JobMenuController Inst { get; private set; } //Singleton
    private void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
            //DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }


    private void Start() 
    {
        //DontDestroyOnLoad(this);       
    }

    public void LoadJobs()
    {
        jobs = Resources.LoadAll<Job>("Scriptable/Jobs");
    }


    public void AcceptJob(int _index)
    {
        if(_index < 0)return;
        
        JobController jc = JobController.Inst;
        jc.currJob = jobs[_index];
        jc.jobStatus = JobStatus.InProgress;
        JobView.Inst.UpdateJob();



    }

    
    

    public void FinishJob()
    {
        JobController jc = JobController.Inst;
        if(jc.currJob==null) return;
        
        PlayerReputation player = PlayerReputation.Inst;
        
        switch (jc.jobStatus)
        {
            case JobStatus.Concluded:
                player.coins += jc.currJob.rewardCoins;
                player.ChangeReputation(jc.currJob.rewardType, jc.currJob.rewardRep);

            break;
            case JobStatus.Failed:
                player.ChangeReputation(jc.currJob.rewardType, -jc.currJob.rewardRep/2);//Looses half reputation on a failed attempt
            break;            
            default:
                jc.FailJob();
                FinishJob();
            break;
        }

        jc.currJob = null;
        jc.jobStatus = JobStatus.NotSelected;
        jc.currJobQtd = 0;
        JobView.Inst.ViewJob(null);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobController : MonoBehaviour
{
    public JobModel[] jobs;
    
    public JobModel currJob;
    public int currJobQtd;
    public enum JobStatus {NotSelected, InProgress, Failed, Concluded};
    public JobStatus jobStatus = JobStatus.NotSelected;

    public static JobController Inst { get; private set; } //Singleton
    private void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }


    private void Start() 
    {
        DontDestroyOnLoad(this);

        jobs = Resources.LoadAll<JobModel>("Scriptable/Jobs");
        JobView.Inst.ListJobs();


    }


    public void AcceptJob(int _index)
    {
        if(_index < 0)return;
        currJob = jobs[_index];
        Debug.Log("Job changed to: " + currJob.name);
        jobStatus = JobStatus.InProgress;
        JobView.Inst.UpdateJob();
    }

    
    public void UpdateJob(int qtd)
    {
        currJobQtd += qtd;
        if(currJobQtd >= currJob.quantity)
        {
            jobStatus = JobStatus.Concluded;
        }else if (currJobQtd < 0)
        {
            jobStatus = JobStatus.Failed;
        }

        JobView.Inst.UpdateJob();
    }

    public void FailJob()
    {
        jobStatus = JobStatus.Failed;
        JobView.Inst.UpdateJob();
    }

    public void FinishJob()
    {
        if(currJob==null) return;
        
        TemporaryPlayer player = TemporaryPlayer.Inst;
        
        switch (jobStatus)
        {
            case JobStatus.Concluded:
                player.coins += Inst.currJob.rewardCoins;
                player.ChangeReputation(currJob.rewardType, Inst.currJob.rewardRep);

            break;
            case JobStatus.Failed:
                player.ChangeReputation(currJob.rewardType, -currJob.rewardRep/2);//Looses half reputation on a failed attempt
            break;            
            default:
                FailJob();
                FinishJob();
            break;
        }

        currJob = null;
        currJobQtd = 0;
        JobView.Inst.UpdateJob();

    }





}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Controller;
using Manager;
using Model;
using UnityEngine.Experimental.Rendering;

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
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        if(JobController.Inst.jobStatus == JobStatus.Failed || JobController.Inst.jobStatus == JobStatus.Concluded)
            FinishJob();
    }


    public void LoadJobs()
    {
        Job[] allJobs = Resources.LoadAll<Job>("Scriptable/Jobs");
        if (allJobs == null || allJobs.Length == 0)
        {
            Debug.LogError("No jobs found in the specified path.");
            return;
        }
        jobs = new Job[4];        
        Faction[] allFac = Resources.LoadAll<Faction>("Scriptable/Faction");
        for (int i = 0; i < 4; i++)
        {                 
            List<Job> factionJobs = new List<Job>();
            
            int playerReputation = GameManager.Instance.reputation.reputations[i].value;            
            foreach (Job job in allJobs)
            {
                if(job.allyFaction  != allFac[i]) continue;                
                switch (job.dangerValue)
                {
                    case 1:
                        if(playerReputation > -500 && playerReputation < 150) factionJobs.Add(job);
                        break;
                    case 2:
                        if(playerReputation > 0 && playerReputation < 600) factionJobs.Add(job);
                        break;
                    case 3:
                        if(playerReputation > 400 && playerReputation < 2000) factionJobs.Add(job);
                        break;
                    default:
                        Debug.Log("ERROR - PlayerRep" + playerReputation + "  |  Danger Value " + job.dangerValue);
                        break;
                }
            }
            if (factionJobs.Count > 0)
            {
                Job randomJob = factionJobs[Random.Range(0, factionJobs.Count)];
                jobs[i] = randomJob;
            }else
            {
                Debug.LogError("factionJobs Count = 0");
            }     
        }
    }


    public void AcceptJob(int _index)
    {
        if(_index < 0)return;

        AcceptJob(jobs[_index]);
        
        
    }

    public void AcceptJob(Job _job)
    {

        JobController jc = JobController.Inst;
        jc.currJob = _job;
        jc.jobStatus = JobStatus.InProgress;
        JobView.Inst.UpdateJob();


        GameManager.Instance.currentScenario = jc.currJob.scenario;

    }

    public void FinishJob()
    {
        JobController jc = JobController.Inst;
        if(jc.currJob==null) return;

        PlayerReputation player = GameManager.Instance.reputation;
        
        switch (jc.jobStatus)
        {
            case JobStatus.Concluded:
                player.coins += jc.currJob.rewardCoins;
                player.ChangeReputation(jc.currJob.allyFaction, jc.currJob.rewardRep);

            break;
            case JobStatus.Failed:
                player.ChangeReputation(jc.currJob.allyFaction, -jc.currJob.rewardRep/2);//Looses half reputation on a failed attempt
            break;            
            default:
                jc.FailJob();
                FinishJob();
            break;
        }

        jc.currJob = null;
        jc.jobStatus = JobStatus.NotSelected;
        jc.currJobQtd = 0;
        UpgradeController.Inst.umc.UpdateCoins();
        JobView.Inst.jobIndex = -1;
        JobView.Inst.ViewJob(null);
        JobView.Inst.UpdateReputations();

    }

    
    public void BackToMenu()
    {
        FinishJob();
        GameManager.Instance.MenuScenario();  
    }
}

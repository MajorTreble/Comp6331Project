using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Manager;
using Model;

public class JobController : MonoBehaviour
{
    public static JobController Inst { get; private set; } //Singleton

    public JobStatus jobStatus = JobStatus.NotSelected;

    public int currJobQtd;

    public Job currJob;

    private void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
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

    bool CheckCurJob()
    {
        if(currJob == null)
        {
            Debug.LogWarning("NO CUR JOB");
            return false;
        }
        
        return true;
    }
    
    
    //Job Events
    public void TargetLeftMap()
    {//called once the target leaves the map
        if(!CheckCurJob()) return;

        if(currJob.jobType == JobType.Hunt)
        {
            UpdateJob(-1);
        }

        if(currJob.jobType == JobType.Defend)
        {
            UpdateJob(1);
        }
        if(currJob.jobType == JobType.Deliver)
        {
            UpdateJob(1);
        }
        
    }

    public void TargetDestoyed()
    {//called once the target destroyed
        if(!CheckCurJob()) return;

        if(currJob.jobType == JobType.Hunt)
        {
            UpdateJob(1);
        }

        if(currJob.jobType == JobType.Defend)
        {
            UpdateJob(-1);
        }
    }

    public void OreMined()
    {//called once the player  get any resource
        if(!CheckCurJob()) return;

        if(currJob.jobType == JobType.Mine)
        {
            UpdateJob(1);
        }
    }


    
    public void LeaveMap()
    {
        //Player go to the map limit, finishing it.
        if(!CheckCurJob()) return;


        if(currJob.jobType == JobType.Deliver)
        {
            JobController.Inst.TargetLeftMap();
        } 
        
        if(jobStatus != JobStatus.Concluded) 
            FailJob();
        
        GameManager.Instance.StopScenario();
    }



    
    // Control for tests
    // Those methods are called once player do some kind of action
    // Kill, Mine, etc
    // For tests this will be controlled by buttons and UI
    public void DestroyTarget()
    {
        TargetDestoyed();
    }
    public void LeaveTargetMap()
    {
        TargetLeftMap();
    }

    public void MineOre()
    {
        OreMined();        
    }
}

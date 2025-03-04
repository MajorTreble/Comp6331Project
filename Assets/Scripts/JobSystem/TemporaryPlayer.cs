using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Reputation{
    public RepType type;
    public int value;

    public Reputation(RepType _type, int _value)
    {
        type = _type;
        value = _value;
    }
    public void ChangeValue(int _value)
    {
        value += _value;
    }
}


public class TemporaryPlayer : MonoBehaviour
{       
    
    public List<Reputation> reputations = new List<Reputation>();
    public int coins;


     public static TemporaryPlayer Inst { get; private set; } //Singleton
    private void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
    

    public void Start()
    {
        Debug.LogWarning("player and camera scripts are deactivated for testing");

        reputations.Add(new Reputation (RepType.Faction1, 0));
        reputations.Add(new Reputation (RepType.Faction2, 0));
        reputations.Add(new Reputation (RepType.Pirate, 0));
        reputations.Add(new Reputation (RepType.Self, 0));     
    }

    private void Update() 
    {
        //if(Input.GetKeyDown(KeyCode.Q)) LeaveMap();
        //if(Input.GetKeyDown(KeyCode.W)) ChooseJob();
        //if(Input.GetKeyDown(KeyCode.E)) DeliverJob();

        //if(Input.GetKeyDown(KeyCode.A)) DestroyTarget();
        //if(Input.GetKeyDown(KeyCode.S)) TargetLeaveMap();
        //if(Input.GetKeyDown(KeyCode.D)) MineOre();        
    }


    public void LeaveMap()
    {
        //Player go to the map limit, finishing it.
        if(JobController.Inst.currJob == null)
        {
            Debug.LogWarning("Curr job is null");
            return;
        }


        if(JobController.Inst.currJob.jobType == JobType.Deliver)
        {
            TargetLeftMap();
        } 
        
        if(JobController.Inst.jobStatus != JobController.JobStatus.Concluded) 
            JobController.Inst.FailJob();
            
        Debug.LogWarning("Teleport to hangar");
    }

    //public void ChooseJob()
    //{
    //    if(JobController.Inst.jobStatus != JobController.JobStatus.NotSelected) return;

    //    JobController.Inst.AcceptJob(JobController.Inst.jobs[0]);
    //}
    public void DeliverJob()
    {//called once player click on deliver job
        
    }

     public void ChangeReputation(RepType _type, int _value)
    {
        Reputation rep = reputations.Find(i => i.type == _type);
        if (rep != null)
            rep.ChangeValue(_value);
        else
            Debug.LogWarning($"Item of type {_type} not found.");
        
    }







    //Job Events
    public void TargetLeftMap()
    {//called once the target leaves the map
        if(JobController.Inst.currJob.jobType == JobType.Hunt)
        {
            JobController.Inst.UpdateJob(-1);
        }

        if(JobController.Inst.currJob.jobType == JobType.Defend)
        {
            JobController.Inst.UpdateJob(1);
        }
        if(JobController.Inst.currJob.jobType == JobType.Deliver)
        {
            JobController.Inst.UpdateJob(1);
        }
        
    }

    public void TargetDestoyed()
    {//called once the target destroyed
        if(JobController.Inst.currJob.jobType == JobType.Hunt)
        {
            JobController.Inst.UpdateJob(1);
        }

        if(JobController.Inst.currJob.jobType == JobType.Defend)
        {
            JobController.Inst.UpdateJob(-1);
        }
    }

    public void OreMined()
    {//called once the player  get any resource
        if(JobController.Inst.currJob.jobType == JobType.Mine)
        {
            JobController.Inst.UpdateJob(1);
        }
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

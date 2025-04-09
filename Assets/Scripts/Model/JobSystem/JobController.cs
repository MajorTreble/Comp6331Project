using UnityEngine;

using Manager;
using Model;
using Model.AI;
using Model.Environment;


namespace Controller
{
    public class JobController : MonoBehaviour
    {
        public static JobController Inst { get; private set; } //Singleton

        public JobStatus jobStatus = JobStatus.NotSelected;
        public int currJobQtd;
        public Job currJob;
        public Job customJob;

        private void Awake()
        {
            if (Inst == null)
            {
                Inst = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);

            Asteroid.OnDestroyed += OnDestroyed;
        }

        private void OnDestory()
        {
            Asteroid.OnDestroyed -= OnDestroyed;
        }

        public void UpdateJob(int qtd)
        {
            currJobQtd += qtd;
            if(currJobQtd >= currJob.quantity)
            {
                jobStatus = JobStatus.Concluded;
                TargetController.Inst.showPortal = true;
            }else if (currJobQtd < 0)
            {
                jobStatus = JobStatus.Failed;                
                TargetController.Inst.showPortal = true;
            }

            JobView.Inst.UpdateJob();
        }

        public void StartJob()
        {
            if (currJob.jobType == JobType.Deliver)
            {
                ForceHostile(GameManager.Instance.playerShip.GetComponent<Ship>());
            }
            else if (currJob.jobType == JobType.Defend)
            {
                foreach (Ship ship in SpawningManager.Instance.shipList)
                {
                    if (ship.faction == currJob.allyFaction)
                    {
                        ForceHostile(ship);

                        AIShip aiShip = ship.GetComponent<AIShip>();
                        if (aiShip)
                        {
                            aiShip.disableCombat = true;
                        }
                    }
                }
            }
        }

        public void OnDestroyed(Asteroid asteroid)
        {
            OnObjDestroyed(asteroid.transform.tag);

            if (currJob.jobType == JobType.Mine)
            {
                ForceHostile(GameManager.Instance.playerShip.GetComponent<Ship>());
            }
        }

        protected void ForceHostile(Ship hostileShip)
        {
            foreach (Ship ship in SpawningManager.Instance.shipList)
            {
                if (ship.faction == currJob.enemyFaction)
                {
                    AIShip aiShip = ship.GetComponent<AIShip>();
                    if (aiShip)
                    {
                        aiShip.SetHostile(hostileShip);
                    }
                }
            }
        }

        public void FailJob()
        {
            jobStatus = JobStatus.Failed;
            if(TargetController.Inst != null)
                TargetController.Inst.showPortal = true;
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
        //To be called automatically
        public void OnObjDestroyed(string _tag)
        {
            //Debug.Log(_tag + " - " + currJob.jobTarget+ " - " + JobUtil.ToTag(currJob.jobTarget));
            if(_tag == JobUtil.ToTag(currJob.jobTarget))
            {
                TargetDestoyed();
            }
        }
        public void OnObjLeave(string _tag)
        {
            if(_tag == JobUtil.ToTag(currJob.jobTarget))
            {
                TargetLeftMap();
            }
        }
        

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

            if(currJob.jobType == JobType.Mine)
            {
                UpdateJob(1);
            }
        }

        


        
        public void LeaveMap()
        {
            //Player go to the map limit, finishing it.
            if(!CheckCurJob()) return;
            
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
            //TargetDestoyed();
            foreach (Ship s in SpawningManager.Instance.shipList)
            {
                if(s.CompareTag("Player")) continue;
                if(s.transform.gameObject.activeSelf == true)
                {
                    s.TakeDamage(s.oriData.maxHealth, null);
                    break;
                }                
            }     
        }
        public void LeaveTargetMap()
        {
            //TargetLeftMap();
            foreach (Ship s in SpawningManager.Instance.shipList)
            {
                if(s.CompareTag("Player")) continue;
                if(s.transform.gameObject.activeSelf == true)
                {
                    s.Leave();
                    break;
                }                
            } 
        }
    }
}

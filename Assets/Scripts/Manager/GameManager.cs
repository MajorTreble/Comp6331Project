using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Controller;
using Model;
using Model.AI;
using Model.Data;

namespace Manager
{

    public class GameManager : MonoBehaviour, IDataPersistence
    {
        public static GameManager Instance { get; private set; }

        public PlayerReputation reputation = null;

        public GameObject playerPrefab;
        public GameObject blackHolePrefab;

        public GameObject playerShip;
        public GameObject portal;

        public GameObject playerLaserPrefab;

        public Vector3 playerSpawnPosition;
        public Quaternion playerSpawnRotation;

        public bool isNewGame = true;
        public bool hasPlayedTutorial = false;

        public bool onMenu = false;

        public Scenario tutorialScenario = null;
        public Scenario quickPlayScenario = null;
        public List<Scenario> scenarios = new List<Scenario>();

        public Scenario currentScenario = null;


        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        protected void Start()
        {
            PersistenceManager.Instance.dataPersistence.Add(this);
        }

        // IDataPersistence
        public void Load(GameData gameData)
        {
            this.isNewGame = gameData.isNewGame;
            this.hasPlayedTutorial = gameData.hasPlayedTutorial;
            this.reputation = gameData.reputation;
        }

        // IDataPersistence
        public void Save(ref GameData gameData)
        {
            gameData.isNewGame = this.isNewGame;
            gameData.hasPlayedTutorial = this.hasPlayedTutorial;
            gameData.reputation = this.reputation;
        }

        public void Play()
        {
            PersistenceManager.Instance.NewGame();

            if (isNewGame)
            {
                isNewGame = false;
            }

            reputation.Reset();
            UpgradeController.Inst.upgrData.Reset();

            Job[] jobs = Resources.LoadAll<Job>("Scriptable/Tutorial");

            JobController jc = JobController.Inst;
            jc.currJob = jobs[0];
            jc.jobStatus = JobStatus.InProgress;

            currentScenario = tutorialScenario;
            StartScenario();
        }

        public void Load()
        {
            PersistenceManager.Instance.LoadGame();

            SceneManager.LoadScene(1);
        }

        public void Save()
        {
            PersistenceManager.Instance.SaveGame();
        }

        public void QuickPlay()
        {
            isNewGame = false;
            hasPlayedTutorial = true;

            Job[] jobs = Resources.LoadAll<Job>("Scriptable/Jobs");

            JobController jc = JobController.Inst;
            jc.currJob = jobs[0];
            jc.jobStatus = JobStatus.InProgress;

            currentScenario = quickPlayScenario;
            StartScenario();
        }

        public bool SelectScenario(JobType job, ScenarioDifficulty difficulty)
        {
            foreach (Scenario scenario in scenarios)
            {
                if (scenario.supportedJobType.Contains(job) && scenario.difficulty == difficulty)
                {
                    currentScenario = scenario;
                    return true;
                }
            }

            return false;
        }

        public void StartScenario()
        {
            SceneManager.LoadScene(currentScenario.sceneName);
        }

        public void StopScenario()
        {
            SceneManager.LoadScene(1);
        }

        public void MenuScenario()
        {
            SceneManager.LoadScene(0);
        }

        public void SpawnPlayer(Vector3 spawnPosition, Quaternion spawnQuaternion)
        {
            SpawnParams spawnParams = new SpawnParams(playerPrefab, spawnPosition, spawnQuaternion);
            playerShip = SpawningManager.Instance.Spawn(spawnParams);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            onMenu = scene.name == "MainMenu";

            if (scene.name != "MainMenu" && scene.name != "Harbor")
            {
                SetupScenario(JobController.Inst.currJob);

                UpgradeController.Inst.UpdateValues();

                if (JobController.Inst.currJob == null)
                {
                    QuickPlay();
                }
            }


            if (scene.name == "Harbor")
            {
                JobStatus jobStatus = JobController.Inst.jobStatus;
                JobView.Inst.ListJobs();
                JobView.Inst.ListReputations();
                JobView.Inst.SetConfigurations();
                if (jobStatus == JobStatus.Concluded || jobStatus == JobStatus.Failed)
                    JobView.Inst.ViewJob(JobController.Inst.currJob);
            }

            JobView.Inst.SetTestButtons();
            JobView.Inst.LookForJobFeeback();
            JobView.Inst.UpdateJob();

        }

        public void SetupScenario(Job job)
        {
            portal = GameObject.Find("HarborPortal");

            SpawningManager.Instance.SpawnScenario(currentScenario, job.allyFaction, job.enemyFaction);
            SpawnPlayer(playerSpawnPosition, playerSpawnRotation);

            if (currentScenario.name == "Tutorial")
            {
                foreach (Ship ship in SpawningManager.Instance.shipList)
                {
                    AIShip aiShip = ship.GetComponent<AIShip>();
                    if (aiShip == null)
                    {
                        continue;
                    }

                    aiShip.SetHostile(playerShip.GetComponent<Ship>());
                }

                GameObject.Find("Canvas").transform.Find("Tutorial").gameObject.SetActive(true);
                Time.timeScale = 0;
            }
        }
    }
}
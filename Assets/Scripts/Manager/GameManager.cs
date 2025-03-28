using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Controller;
using Model;
using Model.Data;

namespace Manager
{

    public class GameManager : MonoBehaviour, IDataPersistence
    {
        public static GameManager Instance { get; private set; }

        public GameObject playerPrefab;
        public GameObject playerShip;

        public Vector3 playerSpawnPosition;
        public Quaternion playerSpawnRotation;

        public bool isNewGame = true;

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

		private void Start()
		{
            PersistenceManager.Instance.dataPersistence.Add(this);
        }

        // IDataPersistence
        public void Load(GameData gameData)
		{
            this.isNewGame = gameData.isNewGame;
		}

        // IDataPersistence
        public void Save(ref GameData gameData)
		{
            gameData.isNewGame = this.isNewGame;
        }

        public void Play()
		{
            PersistenceManager.Instance.LoadGame();

            if (isNewGame)
            {
                isNewGame = false;
            }

            SceneManager.LoadScene(1);
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
            Job[] jobs = Resources.LoadAll<Job>("Scriptable/Jobs");

            JobController jc = JobController.Inst;
            jc.currJob = jobs[0];
            jc.jobStatus = JobStatus.InProgress;
            StartScenario();
        }

        public void StartScenario()
        {
            SceneManager.LoadScene(2);
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
            if (scene.name != "MainMenu" && scene.name != "Harbor")
			{
                SpawnPlayer(playerSpawnPosition, playerSpawnRotation);

                if(JobController.Inst.currJob == null)
                {
                    QuickPlay();
                }
            }


            if(scene.name == "Harbor")
            {
                JobStatus jobStatus = JobController.Inst.jobStatus;
                JobView.Inst.ListJobs();
                if(jobStatus == JobStatus.Concluded || jobStatus == JobStatus.Failed)
                    JobView.Inst.ViewJob(JobController.Inst.currJob);
            }

            JobView.Inst.SetTestButtons();
            JobView.Inst.LookForJobFeeback();
            JobView.Inst.UpdateJob();

        }
    }
}
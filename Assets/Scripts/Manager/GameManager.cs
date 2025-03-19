using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Model.Data;
using Model;
using UnityEngine.Diagnostics;
using UnityEditor.Playables;

namespace Manager
{

    public class GameManager : MonoBehaviour, IDataPersistence
    {
        public static GameManager Instance { get; private set; }

        public GameObject playerPrefab;
		public GameObject playerShip;

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

        public void StartScenario()
        {
            SceneManager.LoadScene(2);
        }

        public void StopScenario()
        {
            SceneManager.LoadScene(1);
        }

        public void SpawnPlayer(Vector3 spawnPosition, Quaternion spawnQuaternion)
        {
            GameObject.Instantiate(playerPrefab, spawnPosition, spawnQuaternion);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!JobMenuController.Inst)
			{
                return;
			}

            JobStatus jobStatus = JobController.Inst.jobStatus;

            if(scene.name == "Harbor")
            {
                JobView.Inst.ListJobs();
                if(jobStatus == JobStatus.Concluded || jobStatus == JobStatus.Failed)
                    JobView.Inst.ViewJob(JobController.Inst.currJob);
            }

            JobView.Inst.SetTestButtons();
            JobView.Inst.LookForJobFeeback();
            JobView.Inst.UpdateJob();
           

            if (jobStatus == JobStatus.NotSelected)
			    return;
                
            //JobMenuController.Inst.AcceptJob(0);
            Job job = JobController.Inst.currJob;
            GameObject ui = GameObject.Find("Canvas");

            Text jobText =  Utils.FindChildByName(ui.transform, "JobFeedbackText").GetComponent<Text>();
        
            jobText.text = job.jobName;
        
        }
    }
}
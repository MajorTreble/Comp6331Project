using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Model.Data;

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
            if (!JobController.Inst)
			{
                return;
			}

            JobController.JobStatus jobStatus = JobController.Inst.jobStatus;
            if (jobStatus == JobController.JobStatus.NotSelected)
			{
                JobController.Inst.AcceptJob(1);
            }

            JobModel job = JobController.Inst.currJob;

            GameObject ui = GameObject.Find("Canvas");
            ui.transform.Find("Job Name").GetComponent<Text>().text = job.jobName;
        }
    }

}
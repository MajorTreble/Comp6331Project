using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Model.Data;

namespace Manager
{

    public class PersistenceManager : MonoBehaviour
    {
        public static PersistenceManager Instance { get; private set; }

		public string dataDirPath = "save";
		public string dataFileName = "savegame.txt";

        private GameData gameData;
		public List<IDataPersistence> dataPersistence;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

			dataPersistence = new List<IDataPersistence>();

		}

        public void NewGame()
		{
			gameData = new GameData();
        }

        public void LoadGame()
        {
			string fullPath = Path.Combine(Application.persistentDataPath, dataDirPath);
			fullPath = Path.Combine(fullPath, dataFileName);
			
			if (!File.Exists(fullPath))
			{
				return;
			}
			
			string dataJSON = "";
			try
			{
				using (FileStream stream = new FileStream(fullPath, FileMode.Open))
				{
					using (StreamReader reader = new StreamReader(stream))
					{
						dataJSON = reader.ReadToEnd();
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogError("Error while loading: " + fullPath + "\n" + e);
			}
			
			gameData = JsonUtility.FromJson<GameData>(dataJSON);

			if (gameData == null)
			{
				NewGame();
			}
			
			foreach(IDataPersistence dataPersistence in dataPersistence)
			{
				dataPersistence.Load(gameData);
			}	
        }

        public void SaveGame()
        {
			foreach(IDataPersistence dataPersistence in dataPersistence)
			{
				dataPersistence.Save(ref gameData);
			}

			string fullPath = Path.Combine(Application.persistentDataPath, dataDirPath);
			fullPath = Path.Combine(fullPath, dataFileName);

			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
				
				string dataJSON = JsonUtility.ToJson(gameData, true);
				
				using (FileStream stream = new FileStream(fullPath, FileMode.Create))
				{
					using (StreamWriter writer = new StreamWriter(stream))
					{
						writer.Write(dataJSON);
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogError("Error while saving: " + fullPath + "\n" + e);
			}
        }
		
		private void OnApplicationQuit()
		{
			SaveGame();
		}
    }

}
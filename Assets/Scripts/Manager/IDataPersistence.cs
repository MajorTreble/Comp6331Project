using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Model.Data;

namespace Manager
{

    public interface IDataPersistence
    {
       public void Load(GameData gameData);
       public void Save(ref GameData gameData);
    }

}
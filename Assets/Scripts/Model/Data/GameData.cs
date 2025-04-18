using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Model.Data
{

	[System.Serializable]
    public class GameData
    {
        public bool isNewGame = true;
        public bool hasPlayedTutorial = false;

        public PlayerReputation reputation = null;
    }

}
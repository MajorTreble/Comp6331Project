using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
	[CreateAssetMenu(fileName = "Faction", menuName = "ScriptableObjects/Faction", order = 1)]
	public class Faction : ScriptableObject
	{
		public enum FactionType {Colonial, Earth, Pirates, Solo }

		public string title;
		public string description;

		public FactionType factionType;

		public GameObject[] prefabs = { null, null, null };
		public AIBehavior behavior = null;
	}

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
	[CreateAssetMenu(fileName = "Faction", menuName = "ScriptableObjects/Faction", order = 1)]
	public class Faction : ScriptableObject
	{
		public enum FactionType { Earth, Colonial, Pirates, Solo }

		public string title;
		public string description;

		public GameObject[] prefabs = { null, null, null };
		public AIBehavior behavior = null;
	}

}
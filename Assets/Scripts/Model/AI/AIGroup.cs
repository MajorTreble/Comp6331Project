using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model.AI
{
    public class AIGroup
    {
        public AIShip leader = null;
        public List<AIShip> ships = new List<AIShip>();
        public GroupMode groupMode = GroupMode.None;
    }

}
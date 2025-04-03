using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Model.AI
{
    public class AIGroup
    {
        public AIShip leader = null;
        public List<AIShip> ships = new List<AIShip>();
        public GroupMode groupMode = GroupMode.None;

        // Ships that were hostile to us
        public List<Ship> hostileShips = new List<Ship>();

        public static bool GetBestTarget(AIShip self, ref LastKnownPosition lkp)
        {
            HashSet<Ship> shipsToCheck = new HashSet<Ship>();
            shipsToCheck.UnionWith(self.hostileShips);

            AIGroup group = self.group;
            if (group != null)
            {
                shipsToCheck.UnionWith(group.hostileShips);
            }

            List<LastKnownPosition> bestList = new List<LastKnownPosition>();
            foreach (Ship ship in shipsToCheck)
            {
                LastKnownPosition bestLKP = null;
                if (AIGroup.GetBestLKP(self, ship, ref bestLKP))
                {
                    Debug.Assert(bestLKP != null);
                    bestList.Add(bestLKP);
                }
            }

            bool result = bestList.Count > 0;
            if (result)
            {
                Debug.Assert(bestList[0] != null);
                bestList = bestList.OrderBy(x => x.distance).OrderBy(x => x.visibility).ToList();
                lkp = bestList[0];
            }
            return result;
        }

        public static bool GetBestLKP(AIShip self, Ship target, ref LastKnownPosition best)
        {
            best = self.LKP.Find((x) => x.target == target);

            AIGroup group = self.group;
            if (best != null && best.visibility == LKPVisibility.Seen || group == null)
            {
                return true;
            }

            foreach (AIShip aiShip in group.ships)
            {
                if (aiShip == self)
                {
                    continue;
                }

                LastKnownPosition lkp = aiShip.LKP.Find((x) => x.target == target);
                if (lkp != null && lkp.visibility == LKPVisibility.Seen)
                {
                    best = lkp;
                    return true;
                }
                if (lkp != null &&
                    (best == null ||
                    (best.visibility != LKPVisibility.SeenRecently && lkp.visibility == LKPVisibility.SeenRecently)))
                {
                    best = lkp;
                }
            }

            return best != null;
        }
    }

}
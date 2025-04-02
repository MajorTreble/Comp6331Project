using System.Collections.Generic;
using Model.Environment;
using UnityEngine;

namespace Model.AI.Environment
{
    public class MagneticFragmentGroupAI : MonoBehaviour
    {
        [Header("Fragment Management")]
        public List<MagneticFragment> fragments = new List<MagneticFragment>();
        public List<Rigidbody> orbitingObjects = new List<Rigidbody>();

        [Header("AI Logic")]
        public BlackHoleFuzzyLogic fuzzyLogic;

        void Update()
        {
            if (fuzzyLogic == null)
            {
                Debug.LogWarning("[FragmentGroupAI] No fuzzy logic assigned.");
                return;
            }

            fuzzyLogic.orbitingMass = orbitingObjects.Count;
            fuzzyLogic.averageProximity = CalculateAverageFragmentDistance();
        }

        float CalculateAverageFragmentDistance()
        {
            if (fragments.Count < 2)
                return 999f;

            float total = 0f;
            int count = 0;

            for (int i = 0; i < fragments.Count; i++)
            {
                for (int j = i + 1; j < fragments.Count; j++)
                {
                    total += Vector3.Distance(
                        fragments[i].transform.position,
                        fragments[j].transform.position);
                    count++;
                }
            }

            return count > 0 ? total / count : 999f;
        }

        public void RegisterFragment(MagneticFragment frag)
        {
            if (!fragments.Contains(frag))
                fragments.Add(frag);
        }

        public void RegisterOrbitingObject(Rigidbody rb)
        {
            if (!orbitingObjects.Contains(rb))
                orbitingObjects.Add(rb);
        }

        public void RemoveFragment(MagneticFragment frag)
        {
            fragments.Remove(frag);
        }

        public void RemoveOrbitingObject(Rigidbody rb)
        {
            orbitingObjects.Remove(rb);
        }

        public Vector3 GetFragmentCentroid()
        {
            if (fragments.Count == 0)
                return transform.position;

            Vector3 sum = Vector3.zero;
            foreach (var frag in fragments)
            {
                if (frag != null)
                    sum += frag.transform.position;
            }

            return sum / fragments.Count;
        }
    }

}

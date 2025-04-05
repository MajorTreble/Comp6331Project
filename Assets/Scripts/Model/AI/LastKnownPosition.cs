using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model.AI
{
    public enum LKPVisibility
    {
        Seen,
        SeenRecently,
        NotSeen,
    }

    public class LastKnownPosition
    {
        // If SeenRecently for this delay, become NotSeen
        public static float visibilityDelay = 10.0f;

        public Ship owner = null;
        public Ship target = null;

        public LKPVisibility visibility = LKPVisibility.NotSeen;
        public Vector3 position = Vector3.zero;
        public float distance = 0;
        public float time = 0;

        public LastKnownPosition(Ship owner, Ship target)
        {
            this.owner = owner;
            this.target = target;
        }

        public void SetVisibility(Ship target, LKPVisibility visibility)
        {
            if (this.target != target)
            {
                return;
            }

            if (this.visibility == LKPVisibility.Seen && visibility == LKPVisibility.NotSeen)
            {
                this.visibility = LKPVisibility.SeenRecently;
            }
            if (this.visibility != LKPVisibility.SeenRecently)
            {
                this.visibility = visibility;
            }

            // Distance from last known position
            this.distance = Vector3.Distance(owner.transform.position, position);
        }

        public void SetVisibility(Ship target, LKPVisibility visibility, Vector3 position, float distance, float time)
        {
            if (this.target != target)
            {
                return;
            }

            this.visibility = visibility;
            this.position = position;
            this.distance = distance;

            if (visibility == LKPVisibility.Seen)
            {
                this.time = time;
            }
        }

        public void Update(float time)
        {
            if (visibility == LKPVisibility.NotSeen)
            {
                return;
            }

            if ((time - this.time) > visibilityDelay)
            {
                visibility = LKPVisibility.NotSeen;
            }
        }
    }

}
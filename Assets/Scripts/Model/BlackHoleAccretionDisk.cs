using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class BlackHoleAccretionDisk : MonoBehaviour
    {
        public float rotationSpeed = 150f; // Speed of rotation

        void Update()
        {
            transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}


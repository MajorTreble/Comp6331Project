using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controller
{

    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float turnSpeed = 100f;
        public float mouseSensitivity = 2f;

        private float verticalInput;
        private float horizontalInput;
        private float mouseX;
        private float mouseY;

        

        void Update()
        {
            verticalInput = Input.GetAxis("Vertical");
            horizontalInput = Input.GetAxis("Horizontal");

            Vector3 desiredMovement = Vector3.forward * verticalInput * moveSpeed * Time.deltaTime;
            desiredMovement.y = Mathf.Clamp(desiredMovement.y, 0, 2);

            transform.Translate(desiredMovement);
            transform.Rotate(Vector3.up * horizontalInput * turnSpeed * Time.deltaTime);

            mouseX = Input.GetAxis("Mouse X");
            mouseY = Input.GetAxis("Mouse Y");

            transform.Rotate(Vector3.up * mouseX * mouseSensitivity);
            transform.Rotate(Vector3.left * mouseY * mouseSensitivity);
        }


    }     
    
}
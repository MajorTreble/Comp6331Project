using UnityEngine;

using Manager;
using Model;

namespace Controller
{

    public class PlayerController : MonoBehaviour
    {
        public float acc;
        public float currSpeed;
        public float maxSpeed;        
        public float turnSpeed;
        public float mouseSensitivity;

        bool isSpacePressed = false;


        Vector3 Velocity;

        //GameObject playerShip;
        Rigidbody playerShip;

        void Start()
        {
            //GameObject playerShip = GameManager.Instance.playerShip;
            playerShip = GameManager.Instance.playerShip.GetComponent<Rigidbody>();  

            acc = acc == 0 ? 10 : acc;
            maxSpeed = maxSpeed == 0 ? 50 : maxSpeed;
            turnSpeed = turnSpeed == 0 ? 10 : turnSpeed;
            mouseSensitivity = mouseSensitivity == 0 ? 10 : mouseSensitivity;

        }

        void LateUpdate()
        {            
            if (playerShip == null)
            {
                playerShip = GameManager.Instance.playerShip.GetComponent<Rigidbody>();
                return;
            }

            //Some redundancies on the code, and mouse not working, 
            //Input 
            float vertical = Input.GetAxis("Vertical");
            float horizontal = Input.GetAxis("Horizontal");
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            //Mov
            currSpeed = Mathf.Clamp(currSpeed + (vertical * acc * Time.deltaTime), 0, maxSpeed);

            //Rot
            //playerShip.transform.Rotate(Vector3.left, mouseY * turnSpeed * Time.deltaTime);   //X    
            //playerShip.transform.Rotate(Vector3.up, mouseX * turnSpeed * Time.deltaTime);   //Y
            //playerShip.transform.Rotate(Vector3.forward, horizontal * turnSpeed * Time.deltaTime); //Z            
            Vector3 rotation = new Vector3(-mouseY, mouseX, horizontal) * turnSpeed * Time.deltaTime * mouseSensitivity;
            
            //Transform based movement 
            //playerShip.transform.Rotate(rotation);    
            //playerShip.transform.position += (playerShip.transform.forward*currSpeed);

            //RigidBody based movement
            playerShip.MovePosition(playerShip.position + (playerShip.transform.forward * currSpeed * Time.deltaTime));
            Quaternion deltaRotation = Quaternion.Euler(rotation);
            playerShip.MoveRotation(playerShip.rotation * deltaRotation);    

            //Laser
            bool laser = Input.GetKey(KeyCode.Space)||Input.GetKey(KeyCode.Mouse0);
            playerShip.GetComponent<PlayerShip>().ShowLaser(laser);

            if(Input.GetKey(KeyCode.E)) Stabilize();
        }


        void Stabilize()
        {
            currSpeed = currSpeed = Mathf.Clamp(currSpeed + (-acc * Time.deltaTime), 0, maxSpeed);

            playerShip.velocity = Vector3.Lerp(playerShip.velocity, Vector3.zero, Time.deltaTime);
            playerShip.angularVelocity = Vector3.Lerp(playerShip.angularVelocity, Vector3.zero, Time.deltaTime);            
            playerShip.rotation = Quaternion.Lerp(playerShip.rotation, Quaternion.identity, Time.deltaTime);
        }
    }      
  
}
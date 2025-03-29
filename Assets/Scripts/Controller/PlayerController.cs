using UnityEngine;

using Manager;
using Model;
using UnityEngine.UI;

namespace Controller
{

    public class PlayerController : MonoBehaviour
    {
        public float acc;
        public float currSpeed;

        public int throttle;
        public float maxSpeed;        
        public float turnSpeed;
        public float mouseSensitivity;


        Vector3 Velocity;
        Rigidbody playerShip;

        void Start()
        {
            playerShip = GameManager.Instance.playerShip.GetComponent<Rigidbody>();  

            acc = acc == 0 ? 1 : acc;
            maxSpeed = maxSpeed == 0 ? 50 : maxSpeed;
            turnSpeed = turnSpeed == 0 ? 10 : turnSpeed;
            mouseSensitivity = mouseSensitivity == 0 ? 10 : mouseSensitivity;

        }

        void LateUpdate()
        {            
            if(GameManager.Instance.onMenu)
                return;

            if (playerShip == null)
            {
                playerShip = GameManager.Instance.playerShip.GetComponent<Rigidbody>();
                return;
            }

            //Input 
            float vertical = Input.GetAxis("Vertical");
            float horizontal = Input.GetAxis("Horizontal");
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                throttle += 1;
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                throttle -= 1;

            throttle = Mathf.Clamp(throttle, -1,3);
            float desiredMov = maxSpeed * throttle/3;
            //Mov
            currSpeed = Mathf.Lerp(currSpeed, desiredMov, acc * Time.deltaTime);

            //Rot
            Vector3 rotation = new Vector3(-mouseY, mouseX, horizontal) * turnSpeed * Time.deltaTime * mouseSensitivity;
        
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
using UnityEngine;
using Manager;
using System;

public class CameraFollow : MonoBehaviour
{
    public Transform player;

    [Header("View Settings")]
    public Vector3 thirdPersonOffset = new Vector3(0, 5, -10);
    public Vector3 cockpitOffset = new Vector3(0, 1, 1);

    [Header("Cockpit View")]
    public bool cockpitView = false;

    public float smoothSpeed = 0.125f;

    public static CameraFollow Inst { get; private set; } //Singleton
    void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        if (GameManager.Instance.playerShip != null)
        {
            player = GameManager.Instance.playerShip.transform;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            cockpitView = !cockpitView;
        }
    }

    public void LateUpdate()
    {
        MoveCamera();
    }

    public void  MoveCamera()
    {
        if (player == null)
            return;

        Vector3 offset = cockpitView ? cockpitOffset : thirdPersonOffset;
        Vector3 desiredPosition = player.position + player.rotation * offset;
        Quaternion desiredRotation = Quaternion.LookRotation(player.forward, Vector3.up);

        if (cockpitView)
        {
            // Stick to cockpit immediately (no smoothing)
            transform.position = desiredPosition;
            transform.rotation = desiredRotation;
        }
        else
        {
            // Smooth follow for third-person view
            transform.position = desiredPosition;
            transform.rotation = desiredRotation;
            //The jittering was caused by high speeds and the smoothness, if the smooth is took out the jitter stops
            //Maybe with the cinemachine this resolves better. 

            //transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed *Time.deltaTime);
            //transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, smoothSpeed *Time.deltaTime);
        }
    }
}

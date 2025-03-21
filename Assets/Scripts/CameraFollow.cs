using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Manager;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;
    public float smoothSpeed = 0.125f; 

    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        player = GameManager.Instance.playerShip.transform;
    }

    void LateUpdate()
    {
        if (player == null)
		{
            return;
		}

        Vector3 desiredPosition = player.position + player.rotation * offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;

        Quaternion desiredRotation = Quaternion.LookRotation(player.forward, Vector3.up);
        Quaternion smoothedRotation = Quaternion.Slerp(transform.rotation, desiredRotation, smoothSpeed);
        transform.rotation = smoothedRotation;
    }
}

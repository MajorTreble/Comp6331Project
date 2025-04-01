using System.Collections;
using System.Collections.Generic;
using Model;
using UnityEngine;

using Manager;

public class PortalController : MonoBehaviour
{

    Transform player;

    void Start()
    {
        player = GameManager.Instance.playerShip.transform;
    }
    void LateUpdate()
    {
        transform.LookAt(player); 

        CheckColisions();       
    }

    void CheckColisions() 
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, this.transform.localScale.x/2);

        foreach (Collider hit in hits)
        {
            Ship ship = hit.gameObject.GetComponent<Ship>();

            if(ship == null)
            {
                if(hit.transform.parent != null)
                    ship = hit.transform.parent.gameObject.GetComponent<Ship>();
            } 
          

            if(ship!= null)
            {
                ship.Leave();
            }                
        }           
    }
}

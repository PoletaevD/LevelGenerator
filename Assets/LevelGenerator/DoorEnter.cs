using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorEnter : MonoBehaviour
{


    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.tag == "Player")
        {
            // ПРоста чтобы потестить
            GetComponentInParent<Door>().GoThrowDoor();
        }
    }
}

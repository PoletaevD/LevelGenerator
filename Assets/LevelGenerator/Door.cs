using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    Section section;
    Exit exit;

    private void Awake()
    {
        section = GetComponentInParent<Section>();
        exit = GetComponentInParent<Exit>();
    }

    public void GoThrowDoor()
    {
        //Some conditions, event etc

        exit.targetSection.GoToThisSection(Exit.GetTargetExitType(exit.m_ExitType));
        section.gameObject.SetActive(false);
    }
}

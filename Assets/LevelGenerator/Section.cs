using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Exit;

public class Section : MonoBehaviour
{
    public enum SectionType { SpawnRoom, CommonRoom, TreasureRoom, Shop, BossRoom }
    [SerializeField]
    SectionType sectionType;
    public SectionType _SectionType { get { return sectionType; } }

    [SerializeField]
    Door doorPrefab;
    [SerializeField]
    int chanceOfDeadEnd = 30;

    HashSet<Exit> exits = new HashSet<Exit>();
    public HashSet<Exit> Exits { get { return exits; } }

    private void Awake()
    {
        foreach (var exit in transform.GetComponentsInChildren<Exit>())
        {
            exits.Add(exit);
        }
    }

    public void Init()
    {
        foreach (var exit in GetTakenExits())
        {
            Instantiate(doorPrefab, exit.transform.position, exit.transform.rotation, exit.transform);
        }
    }

    public void GoToThisSection(ExitType exitType)
    {
        Exit exit = new List<Exit>(Exits).Find(x => x.m_ExitType == exitType);

        if (exit == null)
            return;

        // TODO: make normal))
        gameObject.SetActive(true);

        // User enter and etc there
    }

    public List<Exit> GetFreeExits(bool includeDeadEnds = true)
    {
        List<Exit> freeExits = new List<Exit>();

        foreach (var exit in exits)
        {
            if (exit.targetSection != null)
                continue;

            if (Random.Range(0, 100) > chanceOfDeadEnd || !includeDeadEnds)
                freeExits.Add(exit);
        }

        return freeExits;
    }

    public List<Exit> GetTakenExits()
    {
        List<Exit> takenExits = new List<Exit>();

        foreach (var exit in exits)
        {
            if (exit.targetSection == null)
                continue;

            takenExits.Add(exit);
        }

        return takenExits;
    }

    public void SetConnection(Section section, Exit exit)
    {
        exit.targetSection = section;

        Exit anotherExit = section.GetExitByType(GetTargetExitType(exit.m_ExitType));
        anotherExit.targetSection = this;
    } 

    public Exit GetExitByType(ExitType exitType)
    {
        foreach (var exit in exits)
        {
            if (exit.m_ExitType == exitType)
                return exit;
        }

        return null;
    }
}

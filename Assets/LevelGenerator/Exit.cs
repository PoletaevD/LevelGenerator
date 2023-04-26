using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour
{
    public enum ExitType { Down, Top, Left, Right, none}
    [SerializeField]
    ExitType exitType;
    public ExitType m_ExitType { get { return exitType; } }

    [SerializeField]
    Transform exitPoint;
    public Transform ExitPoint { get { return exitPoint; } }

    public Section targetSection { get; set; }

    public static ExitType GetTargetExitType(ExitType exitType)
    {
        return exitType switch
        {
            ExitType.Down => ExitType.Top,
            ExitType.Top => ExitType.Down,
            ExitType.Left => ExitType.Right,
            ExitType.Right => ExitType.Left,
            _ => ExitType.none,
        };
    }
}

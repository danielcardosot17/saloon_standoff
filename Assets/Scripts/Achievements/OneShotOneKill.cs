using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneShotOneKill : Achievement
{
    public OneShotOneKill()
    {
        achievementName = "...OneShotOneKill...";
    }

    public override bool Condition()
    {
        return false;
    }
}
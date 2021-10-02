using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoBulletsNeeded : Achievement
{
    public NoBulletsNeeded()
    {
        achievementName = "...NoBulletsNeeded...";
    }

    public override bool Condition()
    {
        return false;
    }
}
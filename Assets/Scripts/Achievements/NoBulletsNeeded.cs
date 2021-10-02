using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoBulletsNeeded : Achievement
{
    public NoBulletsNeeded()
    {
        achievementName = "...No Bullets Needed...";
    }

    public override bool Condition()
    {
        return BattleSystem.Instance.LocalPlayer.BulletsUsed == 0;
    }
}
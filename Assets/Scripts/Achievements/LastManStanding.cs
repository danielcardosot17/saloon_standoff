using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastManStanding : Achievement
{
    public LastManStanding()
    {
        achievementName = "...Last Man Standing...";
    }

    public override bool Condition()
    {
        return BattleSystem.Instance.OnlyOneAlive();
    }
}

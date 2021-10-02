using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneShotOneKill : Achievement
{
    public OneShotOneKill()
    {
        achievementName = "...One Shot One Kill...";
    }

    public override bool Condition()
    {
        return BattleSystem.Instance.LocalPlayer.KillCount == BattleSystem.Instance.LocalPlayer.BulletsUsed;
    }
}
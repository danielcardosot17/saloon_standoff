using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pacifist : Achievement
{
    public Pacifist()
    {
        achievementName = "...Pacifist...";
    }

    public override bool Condition()
    {
        return BattleSystem.Instance.LocalPlayer.KillCount == 0;
    }
}
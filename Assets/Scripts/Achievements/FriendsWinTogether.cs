using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendsWinTogether : Achievement
{
    public FriendsWinTogether()
    {
        achievementName = "...FriendsWinTogether...";
    }

    public override bool Condition()
    {
        return BattleSystem.Instance.PlayersWhoGotTheCocktail.Count > 1;
    }
}
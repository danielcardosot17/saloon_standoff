using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastManStanding : Achievement
{
    public LastManStanding()
    {
        achievementName = "...LastManStanding...";
    }

    public override bool Condition()
    {
        return true;
    }
}

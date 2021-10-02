using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Achievement : MonoBehaviour
{
    public string achievementName;

    public virtual bool Condition()
    {
        return false;
    }
}

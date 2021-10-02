using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    private List<Achievement> achievements;

    public List<Achievement> Achievements { get => achievements; private set => achievements = value; }
    
    private void Awake() {
        achievements = GetComponents<Achievement>().ToList();  
    }
}

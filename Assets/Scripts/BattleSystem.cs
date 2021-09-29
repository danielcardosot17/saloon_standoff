using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    [SerializeField] private CountdownTimer countDownTimer;
    // Start is called before the first frame update
    void Start()
    {
        countDownTimer.StartCountdown();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

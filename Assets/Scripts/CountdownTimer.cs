using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountdownTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private float countdowTime;
    [SerializeField] private float preTime;
    [SerializeField] private string getReadyText;
    [SerializeField] private string goActionText;
    [SerializeField] private string readySound;
    [SerializeField] private string goSound;
    private float timeLeft;
    private int tickSeconds;
    private float readyLength;
    private float goLength;

    private void Awake()
    {
        readyLength = AudioManager.Instance.GetClipLength(readySound);
        goLength = AudioManager.Instance.GetClipLength(goSound);
        ResetCountdown();
    }

    public void StartCountdown()
    {
        StartCoroutine(CountdownCorroutine());
    }

    public void ResetCountdown()
    {
        tickSeconds = (int) preTime;
        timeLeft = countdowTime;
        timerText.text = getReadyText;
    }

    private IEnumerator CountdownCorroutine()
    {
        while(timeLeft - Time.deltaTime > 0)
        {
            timeLeft -= Time.deltaTime;
            if((int) (timeLeft - readyLength) < tickSeconds)
            {
                PlayReadySound();
                tickSeconds = (int) (timeLeft - readyLength);
            }
            timerText.text = timeLeft.ToString("0");
            yield return null;
        }
        timeLeft = 0f;
        timerText.text = goActionText;
        PlayGoSound();
    }

    private void PlayReadySound()
    {
        AudioManager.Instance.PlayDelayed(readySound);
    }
    private void PlayGoSound()
    {
        AudioManager.Instance.PlayDelayed(goSound);
    }
}

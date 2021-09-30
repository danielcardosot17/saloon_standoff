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
    private bool isCounting = false;
    private bool finishedCounting = false;
    public bool IsCounting { get => isCounting; private set => isCounting = value; }
    public bool FinishedCounting { get => finishedCounting; private set => finishedCounting = value; }

    private void Start()
    {
        readyLength = AudioManager.Instance.GetClipLength(readySound);
        goLength = AudioManager.Instance.GetClipLength(goSound);
        ResetCountdown();
    }

    public void StartCountdown()
    {
        isCounting = true;
        StartCoroutine(DoAfterTimeCoroutine(preTime,() => {
            StartCoroutine(CountdownCorroutine());
        }));
    }

    public void ResetCountdown()
    {
        finishedCounting = false;
        isCounting = false;
        tickSeconds = (int) countdowTime;
        timeLeft = countdowTime;
        timerText.text = getReadyText;
    }

    private IEnumerator CountdownCorroutine()
    {
        while(timeLeft - Time.deltaTime > 0)
        {
            if((int) (timeLeft - readyLength) < tickSeconds)
            {
                timerText.text = tickSeconds.ToString("0");
                PlayReadySound();
                tickSeconds = (int) (timeLeft - readyLength);
            }
            timeLeft -= Time.deltaTime;
            yield return null;
        }
        timeLeft = 0f;
        timerText.text = goActionText;
        PlayGoSound();
        finishedCounting = true;
        isCounting = false;
    }

    private void PlayReadySound()
    {
        AudioManager.Instance.PlayDelayed(readySound);
    }
    private void PlayGoSound()
    {
        AudioManager.Instance.PlayDelayed(goSound);
    }
    public static IEnumerator DoAfterTimeCoroutine(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        action();
    }
}

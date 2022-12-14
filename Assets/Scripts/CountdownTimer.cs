using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CountdownTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private float countdownTime;
    [SerializeField] private float preTime;
    [SerializeField] private string getReadyText;
    [SerializeField] private string goActionText;
    [SerializeField] private string readySound;
    [SerializeField] private string tickSound;
    [SerializeField] private string goSound;
    private float timeLeft;
    private int tickSeconds;
    private float tickLength;
    private float goLength;
    private bool isCounting = false;
    private bool finishedCounting = false;
    public bool IsCounting { get => isCounting; private set => isCounting = value; }
    public bool FinishedCounting { get => finishedCounting; private set => finishedCounting = value; }

    private void Start()
    {
        tickLength = AudioManager.Instance.GetClipLength(tickSound);
        goLength = AudioManager.Instance.GetClipLength(goSound);
        ResetCountdown();
    }

    public void StartCountdown()
    {
        PlayReadySound();
        isCounting = true;
        StartCoroutine(DoAfterTimeCoroutine(preTime,() => {
            StartCoroutine(CountdownCorroutine());
        }));
    }

    public void ResetCountdown()
    {
        ActivateTimerCanvas();
        finishedCounting = false;
        isCounting = false;
        tickSeconds = (int) countdownTime;
        timeLeft = countdownTime;
        timerText.text = getReadyText;
    }

    public void ActivateTimerCanvas()
    {
        timerText.transform.root.gameObject.SetActive(true);
    }
    public void DeactivateTimerCanvas()
    {
        timerText.transform.root.gameObject.gameObject.SetActive(false);
    }

    private IEnumerator CountdownCorroutine()
    {
        while(timeLeft - Time.deltaTime > 0)
        {
            if((int) (timeLeft - tickLength) < tickSeconds)
            {
                timerText.text = tickSeconds.ToString("0");
                PlayTickSound();
                tickSeconds = (int) (timeLeft - tickLength);
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

    private void PlayTickSound()
    {
        AudioManager.Instance.PlayDelayed(tickSound);
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

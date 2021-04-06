using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerBar : MonoBehaviour
{
    [SerializeField] float maxValue;
    private float currentValue = 0;
    internal bool isTimerActive = false;

    [SerializeField] Image mask;
    [SerializeField] Image fill;
    [SerializeField] bool isHueCyclingOnce;

    void Start()
    {
        // ResetTimer();
    }

    void Update()
    {
        if (isTimerActive)
        {
            currentValue -= Time.deltaTime;
            if (currentValue < 0)
            {
                currentValue = 0;
                OnTimerFinished();
                isTimerActive = false;
            }
            FillBar();
        }
    }

    public void OnTimerFinished()
    {
        UpdateGameOverScoreText();
        Timing.RunCoroutine(GameManager.instance.GameOver(), "GameOver");
    }

    public void ResetTimer()
    {
        currentValue = maxValue;
        FillBar();
    }

    public void FillBar()
    {
        FillBar(currentValue / maxValue);
    }

    public void FillBar(float fillAmount)
    {
        mask.fillAmount = fillAmount;
        if (isHueCyclingOnce)
        {
            fill.color = Color.HSVToRGB(Mathf.Lerp(0f, 1f, fillAmount), 1, 1);
        }
    }

    public void UpdateGameOverScoreText()
    {
        if (GameManager.instance.isMultiplayer)
        {
            GameManager.instance.gameOverScoreText.text = "Player " + ((BizzBuzzClassification.number + 1) % 2 + 1) + " lost!\n" +
                BizzBuzzClassification.number + " is " +
                BizzBuzzClassification.GetClassificationText(BizzBuzzClassification.number);
        }
    }
}
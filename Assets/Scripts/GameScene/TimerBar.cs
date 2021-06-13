using MEC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerBar : MonoBehaviour
{
    internal float maxValue = 5;
    private float currentValue = 0;
    internal bool isTimerActive = false;

    [SerializeField] Image mask;
    [SerializeField] Image fill;
    [SerializeField] bool isHueCyclingOnce;
    // End of generic fields

    [SerializeField] int player;
    [SerializeField] GameOverMenu gameOverMenuScript;

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
                // isTimerActive = false;
                OnTimerFinished();
            }
            FillBar();
        }
    }

    public void OnTimerFinished()
    {
        BizzBuzzButton randomPlayerButton = BizzBuzzButton.buttonsByPlayer[player - 1][0];
        if (!GameManager.isResettingTimerEachRound)
        {
            randomPlayerButton.GameOver(false);
            randomPlayerButton.GetComponent<BizzBuzzButtonEffects>().PlayButtonSound(BizzBuzzButtonEffects.ButtonType.Incorrect);
        }
        else
        {
            randomPlayerButton.GetComponent<BizzBuzzButtonEffects>().PlayIncorrectEffects(true);
            randomPlayerButton.LoseLifeAndGoToNextRound();
        }
        gameOverMenuScript.UpdateLoseText(player);
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

    public void AddTime(float addedTime)
    {
        currentValue += addedTime;
        if (currentValue > maxValue)
        {
            currentValue = maxValue;
        }
    }

    public void SetBarColor(Color color)
    {
        fill.color = color;
    }
}
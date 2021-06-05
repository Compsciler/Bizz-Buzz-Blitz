using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Stopwatch : MonoBehaviour
{
    private float currentValue = 0;
    private int roundedDecimalPlaces = 2;
    internal bool isStopwatchActive = false;

    private int strikesCount = 0;
    private float stopwatchWithPenaltyAdded = 0;
    private float strikePenalty = 0;
    private string strikePenaltyText = "";
    private float penaltyPerStrike = 0.05f;
    private Color strikePenaltyColor = new Color(1f, 0.5f, 0);
    internal string stopwatchWithPenaltyAddedText;

    [SerializeField] internal TMP_Text stopwatchText;

    void Start()
    {
        
    }

    void Update()
    {
        if (isStopwatchActive)
        {
            currentValue += Time.deltaTime;
            stopwatchText.text = GetStopwatchText();

            if (strikesCount > 0)
            {
                UpdateStopwatchWithPenaltyAddedText();
            }
            else
            {
                stopwatchWithPenaltyAdded = currentValue;
                stopwatchWithPenaltyAddedText = stopwatchText.text;
            }
        }
    }

    public void AddStrikePenalty()
    {
        strikesCount++;
        strikePenalty = strikesCount * penaltyPerStrike;
        strikePenaltyText = "(+" + Mathf.RoundToInt(strikePenalty * 100) + "%)";
        strikePenaltyText = ExtensionMethods.GetColoredRichText(strikePenaltyText, strikePenaltyColor);
        stopwatchText.text = GetStopwatchText();
        UpdateStopwatchWithPenaltyAddedText();
    }

    public float CalculateStopwatchWithPenaltyAdded()
    {
        return currentValue * (strikePenalty + 1);
    }
    public void UpdateStopwatchWithPenaltyAddedText()
    {
        stopwatchWithPenaltyAdded = CalculateStopwatchWithPenaltyAdded();
        stopwatchWithPenaltyAddedText = stopwatchText.text + " = " +
            ExtensionMethods.RoundWithTrailingDecimalZeroes(stopwatchWithPenaltyAdded, roundedDecimalPlaces) + "s";
    }

    public string GetStopwatchText()
    {
        return "Time: " + 
            ExtensionMethods.RoundWithTrailingDecimalZeroes(currentValue, roundedDecimalPlaces) +
            "s " + strikePenaltyText;
    }
}
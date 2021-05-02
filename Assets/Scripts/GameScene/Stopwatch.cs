using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Stopwatch : MonoBehaviour
{
    private float currentValue = 0;
    private int roundedDecimalPlaces = 2;
    internal bool isStopwatchActive = false;

    [SerializeField] TMP_Text stopwatchText;

    void Start()
    {
        
    }

    void Update()
    {
        if (isStopwatchActive)
        {
            currentValue += Time.deltaTime;
            stopwatchText.text = "Time: " + ExtensionMethods.RoundWithTrailingDecimalZeroes(currentValue, roundedDecimalPlaces) + "s";
        }
    }
}
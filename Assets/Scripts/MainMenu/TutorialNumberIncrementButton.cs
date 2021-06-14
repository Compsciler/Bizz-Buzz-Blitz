using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialNumberIncrementButton : MonoBehaviour
{
    [SerializeField] int increment;

    [SerializeField] int maxNumber;

    [SerializeField] TMP_InputField numberInput;

    void Start()
    {
        maxNumber = (int)Mathf.Pow(10, numberInput.characterLimit) - 1;
    }

    void Update()
    {
        
    }

    public void IncrementNumber()
    {
        int number;
        if (!int.TryParse(numberInput.text, out number))
        {
            number = 0;
        }
        number += increment;
        if (number < 1)
        {
            number = 1;
        }
        if (number > maxNumber)
        {
            number = maxNumber;
        }
        numberInput.text = number.ToString();
    }
}
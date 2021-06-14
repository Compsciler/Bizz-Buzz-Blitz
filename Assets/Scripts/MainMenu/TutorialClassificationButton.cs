using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialClassificationButton : MonoBehaviour
{
    [SerializeField] string ruleName;

    [SerializeField] Color ruleOffColor;
    [SerializeField] Color ruleOnColor;

    [SerializeField] int descriptionTextsIndex;
    [SerializeField] GameObject descriptionTextsHolder;
    private GameObject[] descriptionTexts;

    [SerializeField] TMP_InputField numberInput;

    private Image buttonImage;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        descriptionTexts = descriptionTextsHolder.GetChildren();

        numberInput.characterValidation = (TMP_InputField.CharacterValidation)InputField.CharacterValidation.Integer;
        // numberInput.text = "1";
    }

    void Update()
    {
        int number;
        if (!int.TryParse(numberInput.text, out number))
        {
            return;
        }
        if (BizzBuzzClassification.ClassifyNum(number, ruleName))
        {
            buttonImage.color = ruleOnColor;
        }
        else
        {
            buttonImage.color = ruleOffColor;
        }
    }

    public void SetDescriptionTextActive()
    {
        descriptionTexts[descriptionTextsIndex].SetActive(true);
    }

    public void ResetMenuPresses()
    {
        SetEachActive(descriptionTexts, false);
    }

    private void SetEachActive(GameObject[] gameObjects, bool value)
    {
        foreach (GameObject go in gameObjects)
        {
            go.SetActive(value);
        }
    }
}
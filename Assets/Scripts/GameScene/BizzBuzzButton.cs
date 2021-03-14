﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BizzBuzzButton : MonoBehaviour
{
    [SerializeField] int player;
    [SerializeField] bool[] buttonRuleValues;

    [SerializeField] GameObject numberGO;

    [SerializeField] TMP_Text buttonText;
    private static List<GameObject> neitherRuleButtons;

    void Awake()
    {
        if (neitherRuleButtons == null)
        {
            neitherRuleButtons = new List<GameObject>();
        }
    }
    
    void Start()
    {
        if (buttonRuleValues.SequenceEqual(new bool[] {false, false})){
            neitherRuleButtons.Add(gameObject);
        }
        SetPlayerNeitherRuleButtonText(1);
    }

    void Update()
    {
        
    }

    public void ButtonClick()
    {
        if (GameManager.instance.isMultiplayer)
        {
            switch (player)
            {
                case 1:
                    if (!StateManager.instance.StateEquals<Player1ActiveState>())
                    {
                        return;
                    }
                    break;
                case 2:
                    if (!StateManager.instance.StateEquals<Player2ActiveState>())
                    {
                        return;
                    }
                    break;
            }
        }

        if (IsButtonCorrect())
        {
            BizzBuzzClassification.number++;
            numberGO.GetComponent<TMP_Text>().text = BizzBuzzClassification.number.ToString();
            SetPlayerNeitherRuleButtonText(player % GameManager.instance.playerTotal + 1);

            if (GameManager.instance.isMultiplayer)
            {
                numberGO.GetComponent<RectTransform>().Rotate(0, 0, 180);

                if (StateManager.instance.StateEquals<Player1ActiveState>())
                {
                    StateManager.instance.SetState(new Player2ActiveState());
                }
                else
                {
                    StateManager.instance.SetState(new Player1ActiveState());
                }
            }
        }
        else
        {
            StateManager.instance.SetState(new GameOverState());
            Debug.Log("Game over!");
        }
    }

    public bool IsButtonCorrect()
    {
        bool[] correctRuleValues = BizzBuzzClassification.ClassifyNum(BizzBuzzClassification.number);
        return buttonRuleValues.SequenceEqual(correctRuleValues);
    }

    public void SetPlayerNeitherRuleButtonText(int playerNum)
    {
        foreach (GameObject go in neitherRuleButtons)
        {
            if (go.GetComponent<BizzBuzzButton>().player == playerNum)
            {
                go.GetComponent<BizzBuzzButton>().buttonText.text = BizzBuzzClassification.number.ToString();
            }
        }
    }
}
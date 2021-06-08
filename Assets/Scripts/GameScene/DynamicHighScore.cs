using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DynamicHighScore : MonoBehaviour
{
    private int highScore;
    private bool isEndlessMode;
    [SerializeField] TMP_Text highScoreText;

    void Start()
    {
        isEndlessMode = BizzBuzzButton.IsEndlessGameMode();
        highScore = HighScoreLogger.instance.GetHighScore(HighScoreLogger.instance.gameMode);
        if (!isEndlessMode && highScore == 0)
        {
            highScoreText.text = "High score: None";
        }
        else
        {
            highScoreText.text = "High score: " + highScore;
        }
    }

    void Update()
    {
        if (isEndlessMode)
        {
            int currentHighScore = BizzBuzzButton.roundNum;
            if (currentHighScore > highScore)
            {
                highScore = currentHighScore;
                highScoreText.text = "High score: " + ExtensionMethods.GetColoredRichText(highScore.ToString(), "00A86B");
            }
        }
    }
}
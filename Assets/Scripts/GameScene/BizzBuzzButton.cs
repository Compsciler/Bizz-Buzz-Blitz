using MEC;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BizzBuzzButton : MonoBehaviour
{
    [SerializeField] internal int player;
    [SerializeField] internal bool[] buttonRuleValues;

    [SerializeField] GameObject numberGO;

    [SerializeField] TMP_Text buttonText;
    internal static List<GameObject> neitherRuleButtons;

    [SerializeField] TimerBar[] timerBars;
    [SerializeField] Stopwatch stopwatch;

    private int losingPlayer = -1;

    internal static int number = 1;
    internal static int targetNum = -1;  // -1 means endless game mode

    [SerializeField] LifeController lifeController;
    [SerializeField] GameOverMenu gameOverMenuScript;

    private BizzBuzzButtonEffects bizzBuzzButtonEffectsScript;

    void Awake()
    {
        if (neitherRuleButtons == null)
        {
            neitherRuleButtons = new List<GameObject>();
        }
    }
    
    void Start()
    {
        bizzBuzzButtonEffectsScript = GetComponent<BizzBuzzButtonEffects>();

        UpdateNumberText();
        if (buttonRuleValues.SequenceEqual(new bool[] {false, false})){
            neitherRuleButtons.Add(gameObject);
        }
        SetRuleButtonText();
        SetPlayerNeitherRuleButtonText(1);
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
            bizzBuzzButtonEffectsScript.PlayCorrectEffects();

            GoToNextNumber();
        }
        else
        {
            bizzBuzzButtonEffectsScript.PlayIncorrectEffects();

            lifeController.LoseLife();
            if (lifeController.lives > 0)
            {
                GoToNextNumber();
            }
            else
            {
                PauseTimersAndStopwatches();
                losingPlayer = player;
                gameOverMenuScript.UpdateLoseText(losingPlayer, buttonRuleValues);
                Timing.RunCoroutine(GameManager.instance.GameOver(), "GameOver");
            }
        }
    }

    public void GoToNextNumber()
    {
        if (number >= targetNum && !IsGameModeEndless())
        {
            PauseTimersAndStopwatches();
            gameOverMenuScript.UpdateWinText();
            Timing.RunCoroutine(GameManager.instance.GameOver(), "GameOver");
            return;
        }

        number++;  // Add method for other game modes
        UpdateNumberText();

        int nextPlayer = player % GameManager.instance.playerTotal + 1;
        SetPlayerNeitherRuleButtonText(nextPlayer);
        timerBars[player - 1].isTimerActive = false;
        timerBars[player - 1].FillBar(0);
        timerBars[nextPlayer - 1].ResetTimer();
        timerBars[nextPlayer - 1].isTimerActive = true;

        if (GameManager.instance.isMultiplayer)
        {
            numberGO.GetComponent<RectTransform>().Rotate(0, 0, 180f);

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

    public bool IsButtonCorrect()
    {
        bool[] correctRuleValues = BizzBuzzClassification.ClassifyNum(number);
        return buttonRuleValues.SequenceEqual(correctRuleValues);
    }

    public void UpdateNumberText()
    {
        numberGO.GetComponent<TMP_Text>().text = number.ToString();
    }

    public void SetRuleButtonText()
    {
        buttonText.text = BizzBuzzClassification.GetClassificationText(buttonRuleValues);
    }

    public void SetPlayerNeitherRuleButtonText(int playerNum)
    {
        foreach (GameObject go in neitherRuleButtons)
        {
            if (go.GetComponent<BizzBuzzButton>().player == playerNum)
            {
                go.GetComponent<BizzBuzzButton>().buttonText.text = number.ToString();
            }
        }
    }

    public static bool IsGameModeEndless()
    {
        return targetNum == -1;
    }

    public void PauseTimersAndStopwatches()
    {
        timerBars[player - 1].isTimerActive = false;
        stopwatch.isStopwatchActive = false;
    }
}
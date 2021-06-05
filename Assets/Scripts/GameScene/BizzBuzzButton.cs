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
    private float fourDigitNumberFontSize = 450f;

    [SerializeField] TMP_Text buttonText;
    internal static List<BizzBuzzButton> buttons;
    internal static List<List<BizzBuzzButton>> buttonsByPlayer;
    internal static List<BizzBuzzButton> neitherRuleButtons;

    [SerializeField] TimerBar[] timerBars;
    [SerializeField] Stopwatch stopwatch;

    private int losingPlayer = -1;

    internal static int number = 1;
    internal static int targetRoundNum = -1;  // -1 means endless game mode
    internal static int roundNum = 1;
    internal static int nextRuleChangeRound = int.MaxValue;
    internal static bool isDisplayingRoundNum = false;
    internal int preRuleChangeEffectTweenID;

    internal static bool areNumbersRandomRange = false;
    internal static int randomNumberRangeSize = 100;
    internal static int randomNumberRangeRoundInterval = 20;
    private static List<int> randomNumberRange;

    [SerializeField] LifeController lifeController;
    [SerializeField] GameOverMenu gameOverMenuScript;
    [SerializeField] TMP_Text roundText;

    private BizzBuzzButtonEffects bizzBuzzButtonEffectsScript;

    void Awake()
    {
        SetUpButtonLists();
    }
    
    void Start()
    {
        bizzBuzzButtonEffectsScript = GetComponent<BizzBuzzButtonEffects>();

        if (areNumbersRandomRange)
        {
            GoToNextNumber();
        }

        UpdateNumberText();
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

            GoToNextRound();
        }
        else
        {
            bizzBuzzButtonEffectsScript.PlayIncorrectEffects(false);

            LoseLifeAndGoToNextRound();
        }
    }

    public void LoseLifeAndGoToNextRound()
    {
        lifeController.LoseLife();
        if (lifeController.lives > 0)
        {
            GoToNextRound();
        }
        else
        {
            PauseTimersAndStopwatches();
            losingPlayer = player;
            gameOverMenuScript.UpdateLoseText(losingPlayer, buttonRuleValues);
            Timing.RunCoroutine(GameManager.instance.GameOver(), "GameOver");
        }
    }

    public void GoToNextRound()
    {
        if (roundNum >= targetRoundNum && !IsGameModeEndless())
        {
            PauseTimersAndStopwatches();
            GameManager.instance.isGameWon = true;
            gameOverMenuScript.UpdateWinText();
            Timing.RunCoroutine(GameManager.instance.GameOver(), "GameOver");
            return;
        }

        roundNum++;
        if (isDisplayingRoundNum)
        {
            roundText.text = "Round number: " + roundNum;
        }

        int prevNumber = number;
        GoToNextNumber();
        UpdateNumberText();

        if (roundNum >= nextRuleChangeRound)
        {
            bizzBuzzButtonEffectsScript.PlayRuleChangeEffects();

            BizzBuzzClassification.UpdateRulesUsed();
            SetPlayerNeitherRuleButtonText(player, prevNumber);
        }
        if (roundNum >= nextRuleChangeRound - GameManager.instance.playerTotal)
        {
            foreach (BizzBuzzButton button in buttonsByPlayer[player % GameManager.instance.playerTotal])
            {
                button.preRuleChangeEffectTweenID = button.GetComponent<BizzBuzzButtonEffects>().PlayPreRuleChangeEffects();
            }
        }

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

    public void GoToNextNumber()
    {
        if (areNumbersRandomRange)
        {
            if ((roundNum - 1) % randomNumberRangeRoundInterval == 0)
            {
                int randomNumberRangeMin = randomNumberRangeSize * (roundNum - 1) / randomNumberRangeRoundInterval + 1;
                randomNumberRange = Enumerable.Range(randomNumberRangeMin, randomNumberRangeSize).ToList();
            }
            number = randomNumberRange[Random.Range(0, randomNumberRange.Count)];
            randomNumberRange.Remove(number);
            return;
        }
        number++;
    }

    public bool IsButtonCorrect()
    {
        bool[] correctRuleValues = BizzBuzzClassification.ClassifyNum(number);
        return buttonRuleValues.SequenceEqual(correctRuleValues);
    }

    public void UpdateNumberText()
    {
        numberGO.GetComponent<TMP_Text>().text = number.ToString();
        if (number >= 1000)
        {
            numberGO.GetComponent<TMP_Text>().fontSize = fourDigitNumberFontSize;
        }
    }

    public void SetRuleButtonText()
    {
        buttonText.text = BizzBuzzClassification.GetClassificationText(buttonRuleValues);
    }

    public static void SetPlayerNeitherRuleButtonText(int playerNum)
    {
        SetPlayerNeitherRuleButtonText(playerNum, number);
    }

    public static void SetPlayerNeitherRuleButtonText(int playerNum, int n)
    {
        foreach (BizzBuzzButton neitherRuleButton in neitherRuleButtons)
        {
            if (neitherRuleButton.player == playerNum)
            {
                neitherRuleButton.buttonText.text = n.ToString();
            }
        }
    }

    public static bool IsGameModeEndless()
    {
        return targetRoundNum == -1;
    }

    public static float CalculateTargetScore()
    {
        BizzBuzzButton randomButton = buttons[0];
        return randomButton.stopwatch.CalculateStopwatchWithPenaltyAdded();
    }
    public static int CalculateEndlessScore()
    {
        return roundNum;
    }

    public void PauseTimersAndStopwatches()
    {
        timerBars[player - 1].isTimerActive = false;
        stopwatch.isStopwatchActive = false;
    }

    public void SetUpButtonLists()
    {
        if (buttons == null)
        {
            buttons = new List<BizzBuzzButton>();
        }
        buttons.Add(this);
        if (buttonsByPlayer == null)
        {
            buttonsByPlayer = new List<List<BizzBuzzButton>>();
            for (int i = 0; i < GameManager.instance.playerTotal; i++)
            {
                buttonsByPlayer.Add(new List<BizzBuzzButton>());
            }
        }
        buttonsByPlayer[player - 1].Add(this);
        if (neitherRuleButtons == null)
        {
            neitherRuleButtons = new List<BizzBuzzButton>();
        }
        if (buttonRuleValues.SequenceEqual(new bool[] {false, false}))
        {
            neitherRuleButtons.Add(this);
        }
    }
}
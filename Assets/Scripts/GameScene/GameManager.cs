using System.Collections.Generic;
using UnityEngine;
using MEC;
using UnityEngine.SceneManagement;
using System;
using GreatArcStudios;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    internal static GameManager instance;
    private Camera mainCamera;
    public bool isUsingGameOver = true;
    internal bool isGameWon = false;

    /* //<
    public float gameCountdownTime = 3f;
    public GameObject countdownGameMask;
    public GameObject[] disableAfterCountdown;
    public GameObject pauseButton;
    public GameObject adMenu;
    */

    public GameObject gameOverMenu;
    public TMP_Text gameOverText;
    public TMP_Text gameOverScoreText;
    public AudioClip countdownEndSound;
    public float countdownEndSoundVolume;
    public AudioClip winSound;
    public float winSoundVolume;
    public AudioClip loseSound;
    public float loseSoundVolume;
    public GameObject fadingMask;
    public float fadeInTime;
    public float minTransparency;
    public float maxTransparency;

    [SerializeField] float gameOverTimeScale = 0.5f;

    private int defaultGameMode = 100;

    [Header("Additional Game Settings")]
    [SerializeField] internal bool areParticlesOn = true;
    [SerializeField] internal bool isSFX_On = true;
    //< [SerializeField] internal bool isTutorial = false;
    internal bool isMultiplayer = false;
    internal int playerTotal = 1;
    internal int[] targetRoundNums = {50, 50, 100, 200, 100, 100, 50, 100, 200};
    
    internal static bool isResettingTimerEachRound = true;  // Chess clock if false
    internal static float resettingTimerTime = 5f;
    internal static float nonResettingTimerTime = 60f;
    internal static float nonResettingMaxTimeDelayAddedEachRound = 0;  // Bronstein delay
    internal static int maxLives = 3;

    [SerializeField] GameObject[] enableOnMultiplayer;
    [SerializeField] GameObject[] disableOnMultiplayer;
    [SerializeField] TimerBar[] timerBars;
    [SerializeField] LifeController[] lifeControllers;
    [SerializeField] Stopwatch stopwatch;
    [SerializeField] GameObject numberGO;
    [SerializeField] float numberMultiplayerY;
    [SerializeField] float numberMultiplayerFontSize;
    [SerializeField] TMP_Text gameModeText;
    [SerializeField] TMP_Text targetRoundText;
    [SerializeField] TMP_Text roundText;

    void Awake()  // Script Execution Order = -2
    {
        if (instance == null)
        {
            instance = this;
        }

        if (SceneManager.GetActiveScene().buildIndex != Constants.mainMenuBuildIndex)
        {
            try
            {
                ApplyGameModeSettings(HighScoreLogger.instance.gameMode);
            }
            catch (NullReferenceException)  // When not starting game from main menu or GameManager.cs exists in main menu
            {
                ApplyGameModeSettings(defaultGameMode);
            }
        }
    }

    void Start()
    {
        mainCamera = Camera.main;

        StateManager.instance.SetState(new ReadyingState());
        // StateManager.instance.SetState(new Player1ActiveState());

        // SceneManager.sceneUnloaded -= OnSceneUnloaded;  // Why can't the delegate be reset here?
        SceneManager.sceneUnloaded += OnSceneUnloaded;  // Adding OnSceneUnloaded() to delegate call when scene unloaded
    }

    void Update()
    {
        
    }

    public IEnumerator<float> GameOver()
    {
        if (StateManager.instance.StateEquals<GameOverState>())
        {
            yield break;
        }
        StateManager.instance.SetState(new GameOverState());
        if (PlayerPrefs.GetInt("StoreReviewRequestTotal", 0) == 0)
        {
            RateGame.isReadyToRequestStoreReview = true;
        }
        /* //<
        pauseButton.GetComponent<Button>().interactable = false;
        if (isTutorial)
        {
            Timing.KillCoroutines();

            gameOverText.text = "TUTORIAL\nCOMPLETE";
            scoreText.text = "";
            gameOverMenu.SetActive(true);
            Debug.Log("Tutorial Complete!");

            
            yield return Timing.WaitForOneFrame;
        }
        else */
        if (isUsingGameOver)
        {
            //< JUST BEFORE REVIVE SCREEN
            foreach (BizzBuzzButton button in BizzBuzzButton.buttons)
            {
                button.GetComponent<BizzBuzzButtonEffects>().CancelPreRuleChangeEffects(button.preRuleChangeEffectTweenID);
            }

            Timing.PauseCoroutines();  // Not perfect solution if second chance used, hopefully no coroutines will be used during Game Over screen
            Timing.ResumeCoroutines("GameOver");

            Time.timeScale = gameOverTimeScale;
            yield return Timing.WaitUntilDone(Timing.RunCoroutine(FadeObjectsBehindMenu()));

            /* //<
            if ((AdManager.instance.adsWatchedTotal < AdManager.instance.maxAdsWatchedPerGame) && Constants.isMobilePlatform)
            {
                adMenu.SetActive(true);
                yield return Timing.WaitUntilDone(Timing.RunCoroutine(AdManager.instance.InfiniteWaitToBreakFrom().CancelWith(adMenu)));
                Debug.Log("FINISHED");
                if (AdManager.instance.isAdCompleted)
                {
                    StateManager.instance.SetState(new PlayingState());
                    fadingMask.SetActive(false);
                    pauseButton.GetComponent<Button>().interactable = true;
                    Debug.Log("YIELD BREAK");
                    yield break;
                }
            }
            */

            // GAME OVER SCREEN
            gameOverMenu.SetActive(true);
            AudioManager.instance.musicSource.Pause();
            if (isGameWon)
            {
                AudioManager.instance.SFX_Source.PlayOneShot(winSound, winSoundVolume);
            }
            else
            {
                AudioManager.instance.SFX_Source.PlayOneShot(loseSound, loseSoundVolume);
            }
            Debug.Log("Game Over!");

            if (!isMultiplayer)
            {
                if (BizzBuzzButton.IsEndlessGameMode())
                {
                    HighScoreLogger.instance.UpdateHighScore(true, BizzBuzzButton.CalculateEndlessScore(), false);
                }
                else if (isGameWon)
                {
                    HighScoreLogger.instance.UpdateHighScore(false, Mathf.CeilToInt(BizzBuzzButton.CalculateTargetScore()), false);  // RIP float high scores
                }
            }
        }
    }

    void ApplyGameModeSettings(int gameMode)
    {
        switch (gameMode)
        {
            case 0:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Bizz", "Buzz"}, int.MaxValue);
                // BizzBuzzButton.targetRoundNum = 50;
                break;
            case 1:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Fuzz", "Bazz"}, int.MaxValue);
                // BizzBuzzButton.targetRoundNum = 50;
                break;
            case 2:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Bizz", "Fuzz"}, int.MaxValue);
                // BizzBuzzButton.targetRoundNum = 100;
                break;
            case 3:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Bizz", "Buzz"}, int.MaxValue);
                // BizzBuzzButton.targetRoundNum = 200;
                break;
            case 4:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Buzz", "Bazz"}, int.MaxValue);
                // BizzBuzzButton.targetRoundNum = 100;
                break;
            case 5:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Dupe", "Pow"}, int.MaxValue);
                // BizzBuzzButton.targetRoundNum = 100;
                break;
            case 6:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Semi", "Pyth"}, int.MaxValue);
                // BizzBuzzButton.targetRoundNum = 50;
                break;
            case 7:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Bizz", "Pow"}, 25);
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Buzz", "Pyth"}, 25);
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Bazz", "Semi"}, 25);
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Fuzz", "Dupe"}, 25);
                // BizzBuzzButton.targetRoundNum = 100;
                break;
            case 8:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Bizz", "Pow"}, 50);
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Buzz", "Pyth"}, 50);
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Bazz", "Semi"}, 50);
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Fuzz", "Dupe"}, 50);
                // BizzBuzzButton.targetRoundNum = 200;
                break;
            case 50:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Bizz", "Buzz"}, int.MaxValue);
                BizzBuzzButton.areNumbersRandomRange = true;
                break;
            case 51:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Fuzz", "Bazz"}, int.MaxValue);
                BizzBuzzButton.areNumbersRandomRange = true;
                break;
            case 52:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Dupe", "Semi"}, int.MaxValue);
                BizzBuzzButton.areNumbersRandomRange = true;
                break;
            case 53:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Bizz", "Buzz"}, int.MaxValue);
                BizzBuzzButton.areNumbersRandomRange = true;
                BizzBuzzButton.randomNumberRangeSize = 1000;
                BizzBuzzButton.randomNumberRangeRoundInterval = 100;
                break;
            case 54:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Pow", "Pyth"}, int.MaxValue);  // OEIS A266927
                BizzBuzzButton.areNumbersRandomRange = true;
                break;
            case 55:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"RandomIsDivisbleByOrContainsDigit", "RandomIsDivisbleByOrContainsDigit"}, 20);
                BizzBuzzButton.areNumbersRandomRange = true;
                break;
            case 56:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Random", "Random"}, 20);
                BizzBuzzButton.areNumbersRandomRange = true;
                break;
            case 57:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"RandomIsDivisbleByOrContainsDigit", "RandomIsDivisbleByOrContainsDigit"}, 1);
                timerBars[0].maxValue = 1.5f;
                break;
            case 58:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Random", "Random"}, 1);
                timerBars[0].maxValue = 3f;
                break;
            case 100:  // Multiplayer modes start from 100
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Bizz", "Buzz"}, int.MaxValue);
                playerTotal = 2;
                break;
            case 101:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Fuzz", "Bazz"}, int.MaxValue);
                playerTotal = 2;
                break;
            case 102:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Dupe", "Semi"}, int.MaxValue);
                playerTotal = 2;
                break;
            case 103:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Pow", "Pyth"}, int.MaxValue);
                playerTotal = 2;
                break;
            case 104:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Bizz", "Buzz"}, int.MaxValue);
                BizzBuzzButton.areNumbersRandomRange = true;
                playerTotal = 2;
                break;
            case 105:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"RandomIsDivisbleByOrContainsDigit", "RandomIsDivisbleByOrContainsDigit"}, 10);
                playerTotal = 2;
                break;
            case 106:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Random", "Random"}, 10);
                playerTotal = 2;
                break;
            case 107:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"RandomIsDivisbleByOrContainsDigit", "RandomIsDivisbleByOrContainsDigit"}, 1);
                playerTotal = 2;
                break;
            case 108:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Random", "Random"}, 1);
                playerTotal = 2;
                break;
            case 109:
                BizzBuzzClassification.AddRuleInterval(new List<string>() {"Booz", "Bizz"}, int.MaxValue);
                playerTotal = 2;
                break;
        }
        isMultiplayer = playerTotal > 1;
        if (isMultiplayer)
        {
            foreach (GameObject go in enableOnMultiplayer)
            {
                go.SetActive(true);
            }
            foreach (GameObject go in disableOnMultiplayer)
            {
                go.SetActive(false);
            }
            RectTransform numberRectTransform = numberGO.GetComponent<RectTransform>();
            numberRectTransform.anchoredPosition = new Vector2(numberRectTransform.anchoredPosition.x, numberMultiplayerY);
            numberGO.GetComponent<TMP_Text>().fontSize = numberMultiplayerFontSize;
        }
        if (gameMode < HighScoreLogger.endlessGameModeMinNum)
        {
            BizzBuzzButton.targetRoundNum = targetRoundNums[gameMode];
        }
        if (BizzBuzzButton.IsEndlessGameMode())
        {
            gameModeText.text = "Game mode: Endless";
            BizzBuzzButton.isDisplayingRoundNum = true;
            roundText.text = "Round number: 1";
        }
        else
        {
            roundText.gameObject.SetActive(false);
            stopwatch.gameObject.SetActive(true);
            
            foreach (TimerBar timerBar in timerBars)
            {
                timerBar.enabled = false;
                foreach (GameObject go in timerBar.gameObject.GetChildren())
                {
                    go.SetActive(false);
                }
            }
            gameModeText.text = "Game mode: Target";
            targetRoundText.text = "Target round: " + BizzBuzzButton.targetRoundNum;
        }
        if (isMultiplayer)
        {
            ApplyMultiplayerCustomSettings();
        }
        else
        {
            ResetMultiplayerCustomSettings();
        }
    }

    public void ApplyMultiplayerCustomSettings()
    {
        float timerTime;
        if (isResettingTimerEachRound)
        {
            timerTime = resettingTimerTime;
        }
        else
        {
            timerTime = nonResettingTimerTime;
        }
        foreach (TimerBar timerBar in timerBars)
        {
            timerBar.maxValue = timerTime;
        }
        foreach (LifeController lifeController in lifeControllers)
        {
            lifeController.SetMaxLives(maxLives);
        }
    }
    public void ResetMultiplayerCustomSettings()
    {
        isResettingTimerEachRound = true;
        resettingTimerTime = 5f;
        nonResettingTimerTime = 60f;
        nonResettingMaxTimeDelayAddedEachRound = 0;
        maxLives = 3;

        DifficultySelectMenuBasic.timerTimeToggleButtonTextStringIndex = 3;
        DifficultySelectMenuBasic.nonResettingTimerTimeToggleButtonTextStringIndex = 3;
        DifficultySelectMenuBasic.nonResettingMaxTimeDelayAddedEachRoundToggleButtonTextStringIndex = 0;
    }

    public IEnumerator<float> FadeObjectsBehindMenu()
    {
        fadingMask.SetActive(true);
        float timer = 0;
        while (timer < fadeInTime)
        {
            Color maskColor = fadingMask.GetComponent<Image>().color;
            fadingMask.GetComponent<Image>().color = new Color(maskColor.r, maskColor.g, maskColor.b, Mathf.Lerp(minTransparency, maxTransparency, timer / fadeInTime));
            timer += Time.deltaTime;
            yield return Timing.WaitForOneFrame;
        }
    }

    public void ResetStaticVariables()
    {
        //< PauseManager.isPaused = false;
        Time.timeScale = 1;  // Resetting time scale when restarting or quitting game
        BizzBuzzClassification.ruleIntervalList = new List<RuleInterval>();
        BizzBuzzClassification.rulesUsed = null;
        BizzBuzzButton.number = 1;
        BizzBuzzButton.targetRoundNum = -1;
        BizzBuzzButton.roundNum = 1;
        BizzBuzzButton.nextRuleChangeRound = int.MaxValue;
        BizzBuzzButton.areNumbersRandomRange = false;
        BizzBuzzButton.randomNumberRangeSize = 100;
        BizzBuzzButton.randomNumberRangeRoundInterval = 20;
        BizzBuzzButton.buttons = null;
        BizzBuzzButton.buttonsByPlayer = null;
        BizzBuzzButton.neitherRuleButtons = null;
        Debug.Log("Static variables reset!");
    }

    public void OnSceneUnloaded(Scene currentScene)
    {
        ResetStaticVariables();
        SceneManager.sceneUnloaded -= OnSceneUnloaded;  // Resets delegate
    }
}
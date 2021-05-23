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
    public AudioClip gameOverSound;
    public float gameOverSoundVolume;
    public GameObject fadingMask;
    public float fadeInTime;
    public float minTransparency;
    public float maxTransparency;

    [SerializeField] float gameOverTimeScale = 0.5f;

    private int defaultGameMode = 0;

    [Header("Additional Game Settings")]
    [SerializeField] internal bool areParticlesOn = true;
    [SerializeField] internal bool areEffectsOn = true;
    //< [SerializeField] internal bool isTutorial = false;
    internal bool isMultiplayer = false;
    internal int playerTotal = 1;

    [SerializeField] GameObject[] enableOnMultiplayer;
    [SerializeField] GameObject[] disableOnMultiplayer;
    [SerializeField] GameObject numberGO;
    [SerializeField] float numberMultiplayerY;
    [SerializeField] float numberMultiplayerFontSize;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        try
        {
            ApplyGameModeSettings(HighScoreLogger.instance.gameMode);
        }
        catch (NullReferenceException)  // When not starting game from main menu
        {
            ApplyGameModeSettings(defaultGameMode);
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
        /* //<
        pauseButton.GetComponent<Button>().interactable = false;
        if (isTutorial)
        {
            Timing.KillCoroutines();

            gameOverText.text = "TUTORIAL\nCOMPLETE";
            scoreText.text = "";
            gameOverMenu.SetActive(true);
            Debug.Log("Tutorial Complete!");

            if (PlayerPrefs.GetInt("StoreReviewRequestTotal", 0) == 0)
            {
                RateGame.isReadyToRequestStoreReview = true;
            }
            yield return Timing.WaitForOneFrame;
        }
        else */
        if (isUsingGameOver)
        {
            //< JUST BEFORE REVIVE SCREEN
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
            //< spawnPeopleScript.UpdateGameOverScoreText();  //{Update score text
            AudioManager.instance.musicSource.Pause();
            AudioManager.instance.SFX_Source.PlayOneShot(gameOverSound, gameOverSoundVolume);
            Debug.Log("Game Over!");

            //< int newScore = -1;  //{Get new score
            //< HighScoreLogger.instance.UpdateHighScore(newScore, false);
        }
    }

    void ApplyGameModeSettings(int gameMode)
    {
        switch (gameMode)
        {
            case 0:
                break;
            case 100:  // Multiplayer modes start from 100
                isMultiplayer = true;
                playerTotal = 2;
                break;
        }
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
        BizzBuzzClassification.rulesUsed = null;
        BizzBuzzClassification.number = 1;
        BizzBuzzButton.neitherRuleButtons = null;
        Debug.Log("Static variables reset!");
    }

    public void OnSceneUnloaded(Scene currentScene)
    {
        ResetStaticVariables();
        SceneManager.sceneUnloaded -= OnSceneUnloaded;  // Resets delegate
    }
}
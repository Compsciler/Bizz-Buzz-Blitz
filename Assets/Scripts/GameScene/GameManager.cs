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
    [SerializeField] internal bool isGameActive = true;  // Time.timeScale will be used to check for pausing  // Change to false during start countdown?
    internal bool isGameActivePreviousFrame = true;
    // private float gameTimer = 0;  // Could use Time.timeSinceLevelLoad with Time.timeScale instead for simple functionality
    public bool isUsingGameOver = true;
    public float infectionSpreadRate = 3f;
    internal bool hasGameStarted = false;
    public float gameCountdownTime = 3f;
    public GameObject spawnManager;
    public GameObject countdownGameMask;
    public GameObject[] disableAfterCountdown;
    public GameObject pauseButton;
    public GameObject dialogueManager;
    public GameObject dialoguePanel;
    public GameObject adMenu;
    public GameObject peopleGO;
    public bool isResettingDelayedSymptoms;
    // internal static bool isReadyToRequestStoreReview = false;

    private GenerateWalls generateWallsScript;
    private SpawnPeople spawnPeopleScript;
    private TileDisabler tileDisablerScript;

    internal Dictionary<GameObject, float> infectedPathDistances;  // Disabled for now
    public GameObject gameOverMenu;
    public TMP_Text gameOverText;
    public TMP_Text scoreText;
    public AudioClip countdownEndSound;
    public float countdownEndSoundVolume;
    public AudioClip gameOverSound;
    public float gameOverSoundVolume;
    public GameObject fadingMask;
    public float fadeInTime;
    public float minTransparency;
    public float maxTransparency;

    private int defaultGameMode = 0;

    [Header("Additional Game Settings")]
    [SerializeField] internal float infectionDeathDuration = 40f;
    [SerializeField] internal bool canHealthyHeal = false;
    [SerializeField] internal bool areSymptomsDelayed = false;
    [SerializeField] internal float symptomDelayTime = 15f;  // No effect if areSymptomsDelayed is false
    [SerializeField] internal bool isChangingBackgroundColor = true;
    public Color[] backgroundColors;
    [SerializeField] internal bool areParticlesOn = true;
    [SerializeField] internal bool isTutorial = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        mainCamera = Camera.main;
        generateWallsScript = spawnManager.GetComponent<GenerateWalls>();
        spawnPeopleScript = spawnManager.GetComponent<SpawnPeople>();
        tileDisablerScript = spawnManager.GetComponent<TileDisabler>();

        try  // When starting game in GameScene scene
        {
            ApplyGameModeSettings(HighScoreLogger.instance.gameMode);
        }
        catch (NullReferenceException)
        {
            ApplyGameModeSettings(defaultGameMode);
        }

        // Timing.RunCoroutine(InfectionSpread());
        infectedPathDistances = new Dictionary<GameObject, float>();
        if (gameCountdownTime > 0)
        {
            countdownGameMask.SetActive(true);
        }
        Timing.RunCoroutine(BeginAfterCountdown());

        // SceneManager.sceneUnloaded -= OnSceneUnloaded;  // Why can't the delegate be reset here?
        SceneManager.sceneUnloaded += OnSceneUnloaded;  // Adding OnSceneUnloaded() to delegate call when scene unloaded
    }

    void Update()
    {
        if (isGameActive)
        {
            /*
            float minPathDistance = float.MaxValue;
            GameObject closestInfected = null;
            foreach (GameObject infected in infectedPathDistances.Keys)  // Remove dictionary if only used here
            {
                float pathDistance = infected.GetComponent<PersonController>().hospitalTileDistance;
                if (pathDistance < minPathDistance)
                {
                    minPathDistance = pathDistance;
                    closestInfected = infected;
                }
                // infectedPathDistances[infected] = infected.GetComponent<PersonController>().hospitalTileDistance;
            }
            // gameTimer += Time.deltaTime;
            */
        }
    }

    void LateUpdate()
    {
        if (isGameActivePreviousFrame != isGameActive)
        {
            Debug.Log("isGameActive was set to false");
        }
        isGameActivePreviousFrame = isGameActive;  // isGameActive set to false in GameOver(), which happens from InfectionProcess() coroutine; hopefully coroutines always execute before LateUpdate()
    }

    public IEnumerator<float> GameOver()
    {
        isGameActive = false;
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
        else if (isUsingGameOver)
        {
            // JUST BEFORE REVIVE SCREEN
            Timing.PauseCoroutines();  // Not perfect solution if second chance used, hopefully no coroutines will be used during Game Over screen
            Timing.ResumeCoroutines("GameOver");

            yield return Timing.WaitUntilDone(Timing.RunCoroutine(FadeObjectsBehindMenu()));

            if ((AdManager.instance.adsWatchedTotal < AdManager.instance.maxAdsWatchedPerGame) && Constants.isMobilePlatform)
            {
                adMenu.SetActive(true);
                yield return Timing.WaitUntilDone(Timing.RunCoroutine(AdManager.instance.InfiniteWaitToBreakFrom().CancelWith(adMenu)));
                Debug.Log("FINISHED");
                if (AdManager.instance.isAdCompleted)
                {
                    ResetInfectionTimers();
                    isGameActive = true;  // FIRST?
                    fadingMask.SetActive(false);
                    pauseButton.GetComponent<Button>().interactable = true;
                    Debug.Log("YIELD BREAK");
                    yield break;
                }
            }

            // GAME OVER SCREEN
            gameOverMenu.SetActive(true);
            spawnPeopleScript.UpdateGameOverScoreText();
            AudioManager.instance.musicSource.Pause();
            AudioManager.instance.SFX_Source.PlayOneShot(gameOverSound, gameOverSoundVolume);
            Debug.Log("Game Over!");

            int newScore = spawnPeopleScript.CalculateScore();
            HighScoreLogger.instance.UpdateHighScore(newScore, false);
        }
    }

    void ResetInfectionTimers()
    {
        List<GameObject> trulyInfectedPeople = new List<GameObject>();
        foreach (GameObject person in ExtensionMethods.GetChildren(peopleGO))
        {
            if (person.CompareTag("Infected"))
            {
                trulyInfectedPeople.Add(person);
            }
        }

        foreach (GameObject person in trulyInfectedPeople)
        {
            PersonController personControllerScript = person.GetComponent<PersonController>();
            GameObject infectionCylinder = personControllerScript.infectionCylinderScript.gameObject;
            if (areParticlesOn)
            {
                KillPausedCoroutine("PlayInfectionParticles " + personControllerScript.GetInstanceID());
                personControllerScript.infectionParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            if (areSymptomsDelayed && isResettingDelayedSymptoms)
            {
                int infectionProcessKillTotal = KillPausedCoroutine("InfectionProcess " + personControllerScript.GetInstanceID());
                if (infectionProcessKillTotal == 0)
                {
                    KillPausedCoroutine("DelaySymptoms " + personControllerScript.GetInstanceID());
                }
                Timing.RunCoroutine(personControllerScript.DelaySymptoms(), "DelaySymptoms " + personControllerScript.GetInstanceID());

                personControllerScript.meshRenderer.material = personControllerScript.infectedMaterials[0];
                infectionCylinder.SetActive(true);
                infectionCylinder.GetComponent<MeshRenderer>().enabled = true;
                // personControllerScript.infectionCylinderScript.isManuallyEnablingMeshRenderer = true;
            }
            else
            {
                KillPausedCoroutine("InfectionProcess " + personControllerScript.GetInstanceID());
                Timing.RunCoroutine(personControllerScript.InfectionProcess(), "InfectionProcess " + personControllerScript.GetInstanceID());
                // Timing.RunCoroutine(personControllerScript.infectionCylinderScript.SinusoidalRadius(), "SinusoidalRadius " + personControllerScript.GetInstanceID());

                infectionCylinder.SetActive(true);
            }
            Timing.ResumeCoroutines();
        }
    }

    int KillPausedCoroutine(string tag)
    {
        Timing.ResumeCoroutines(tag);
        return Timing.KillCoroutines(tag);
    }

    IEnumerator<float> BeginAfterCountdown()
    {
        yield return Timing.WaitForSeconds(gameCountdownTime);
        generateWallsScript.BeginGeneration();
        hasGameStarted = true;
        if (!isTutorial)
        {
            AudioManager.instance.SFX_Source.PlayOneShot(countdownEndSound, countdownEndSoundVolume);
            AudioManager.instance.musicSource.enabled = true;
        }
        foreach (GameObject go in disableAfterCountdown)
        {
            go.SetActive(false);
        }
        if (isTutorial)
        {
            dialogueManager.SetActive(true);
            dialoguePanel.SetActive(true);
            StartCoroutine(dialoguePanel.GetComponent<DialogueTrigger>().TriggerDialogue());
        }
    }

    IEnumerator<float> InfectionSpread()  // PauseCoroutine() if game paused
    {
        while (true)
        {
            GameObject[] infectedPeople = GameObject.FindGameObjectsWithTag("Infected");
            foreach (GameObject person in infectedPeople)
            {
                GameObject infectionCylinder = person.transform.GetChild(0).gameObject;
                // infectionCylinder.GetComponent<InfectionCylinder>().expandRadius();
                Timing.RunCoroutine(infectionCylinder.GetComponent<InfectionCylinder>().ExpandRadius());  // Does not work well with SinusoidalRadius()
            }
            yield return Timing.WaitForSeconds(infectionSpreadRate);
        }
    }

    void ApplyGameModeSettings(int gameMode)
    {
        switch (gameMode)
        {
            case 0:
                spawnPeopleScript.isUsingExceptionWaves = true;
                spawnPeopleScript.isUsingSemiQuickWaves = true;
                break;
            case 1:
                spawnPeopleScript.isUsingExceptionWaves = true;
                spawnPeopleScript.isUsingSemiQuickWaves = true;
                canHealthyHeal = true;
                generateWallsScript.wallTotal = 35;
                break;
            case 2:
                infectionDeathDuration = 35f;  // [15, 35] or [10, 40]?
                canHealthyHeal = true;
                areSymptomsDelayed = true;
                generateWallsScript.wallTotal = 35;  // 32?
                spawnPeopleScript.areWavesRandom = true;
                break;
            case 3:
                spawnPeopleScript.isUsingExceptionWaves = true;
                spawnPeopleScript.startRepeatRate = 15f;
                spawnPeopleScript.repeatRateDecrease = 1f;
                break;
            case 4:
                spawnPeopleScript.areSpawningMultiple = true;
                spawnPeopleScript.repeatRateDecreaseWaveInterval = 2;
                break;
            case 5:
                spawnPeopleScript.isUsingExceptionWaves = true;
                spawnPeopleScript.isUsingSemiQuickWaves = true;
                spawnPeopleScript.spawnPosListIndex = 1;
                break;
            case 6:
                spawnPeopleScript.isUsingExceptionWaves = true;
                spawnPeopleScript.isUsingSemiQuickWaves = true;
                generateWallsScript.wallTotal = 35;
                tileDisablerScript.tileSetEnables = new bool[]{false, true};
                break;
            case 7:
                infectionDeathDuration = 35f;
                canHealthyHeal = true;
                // areSymptomsDelayed = true;
                spawnPeopleScript.spawnPosListIndex = 2;
                // tileDisablerScript.tileSetEnables = new bool[] {true, false};
                generateWallsScript.wallTotal = 40;

                spawnPeopleScript.startRepeatRate = 25f;
                spawnPeopleScript.repeatRateDecreaseWaveInterval = 4;
                // spawnPeopleScript.areWavesRandom = true;
                spawnPeopleScript.randomWaveGroupSize = 8;
                spawnPeopleScript.areSpawningMultiple = true;
                spawnPeopleScript.multipleSpawnWaves = new int[4, 2]{{1, 0}, {1, 0}, {1, 0}, {2, 1}};
                break;
            case -1:
                isTutorial = true;
                spawnPeopleScript.startDelay = 28f;
                spawnPeopleScript.startRepeatRate = 33.5f;
                spawnPeopleScript.isFirstSpawnSoundDisabled = false;
                gameCountdownTime = 0;
                break;
        }
        if (isChangingBackgroundColor && gameMode >= 0)
        {
            mainCamera.backgroundColor = backgroundColors[gameMode];
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
        HospitalTile.isOccupied = false;
        HealProgressBar.isNewlyOccupied = false;
        HealProgressBar.isOccupiedByInfectedPerson = true;
        PersonController.infectedPeopleTotal = 0;
        PauseManager.isPaused = false;
        Time.timeScale = 1;  // Resetting time scale when restarting or quitting game
        Debug.Log("Static variables reset!");
    }

    public void OnSceneUnloaded(Scene currentScene)
    {
        ResetStaticVariables();
        SceneManager.sceneUnloaded -= OnSceneUnloaded;  // Resets delegate
    }
}